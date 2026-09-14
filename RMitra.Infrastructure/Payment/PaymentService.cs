using Dapper;
using Microsoft.Extensions.Options;
using RMitra.Application.Abstractions;
using RMitra.Application.Payment;
using RMitra.BuildingBlocks.Exceptions;
using RMitra.Domain.Common;
using PaymentEntity = RMitra.Domain.Payment.Payment;
using RMitra.Infrastructure.Options;
using RMitra.Infrastructure.Ordering;

namespace RMitra.Infrastructure.Payment;

public class PaymentService : IPaymentService
{
    private readonly ISqlConnectionFactory _connections;
    private readonly IPublicIdGenerator _ids;
    private readonly CommerceOptions _commerce;

    public PaymentService(ISqlConnectionFactory connections, IPublicIdGenerator ids, IOptions<CommerceOptions> commerce)
    {
        _connections = connections;
        _ids = ids;
        _commerce = commerce.Value;
    }

    public async Task<PaymentDto> CreateForOrderAsync(string orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var order = await OrderingService.RequireOrder(db, orderId);
        if (order.CustomerUserId != userId)
            throw AppException.Forbidden();
        if (order.PaymentMethod == PaymentMethods.Cod)
            throw AppException.Validation("COD orders do not create a gateway payment.");
        if (order.Status is not OrderStatuses.PaymentPending and not OrderStatuses.PaymentFailed)
            throw AppException.Validation("Order is not awaiting payment.");

        var attempts = await db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM payPayments WHERE OrderGuid=@Id", new { order.Id });
        if (attempts >= 3)
            throw AppException.Validation("Maximum 3 payment retries reached.");

        var payment = new PaymentEntity
        {
            Id = Guid.NewGuid(),
            PaymentId = await _ids.NextAsync("PAY"),
            OrderGuid = order.Id,
            UserId = userId,
            Amount = order.GrandTotal,
            Method = order.PaymentMethod,
            Status = PaymentStatuses.Pending,
            AttemptCount = attempts + 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await db.ExecuteAsync(
            @"INSERT INTO payPayments (Id, PaymentId, OrderGuid, UserId, Amount, Method, Status, AttemptCount, CreatedAt, UpdatedAt)
              VALUES (@Id, @PaymentId, @OrderGuid, @UserId, @Amount, @Method, @Status, @AttemptCount, @CreatedAt, @UpdatedAt)",
            payment);
        return Map(payment, order.OrderId);
    }

    public async Task<PaymentDto> ConfirmAsync(string paymentId, ConfirmPaymentRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var payment = await Require(db, paymentId);
        if (!request.Success)
            return await Fail(db, payment, request.FailureReason ?? "Signature or SDK confirmation failed.");

        return await Succeed(db, payment, request.GatewayReference);
    }

    public async Task<PaymentDto> ProcessWebhookAsync(PaymentWebhookRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var payment = await Require(db, request.PaymentId);
        if (request.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
            return await Succeed(db, payment, request.GatewayReference);

        return await Fail(db, payment, "Webhook marked payment failed.");
    }

    private async Task<PaymentDto> Succeed(System.Data.IDbConnection db, PaymentEntity payment, string? reference)
    {
        await db.ExecuteAsync(
            "UPDATE payPayments SET Status=@status, GatewayReference=@reference, UpdatedAt=SYSUTCDATETIME() WHERE Id=@Id",
            new { payment.Id, status = PaymentStatuses.Success, reference });
        await OrderingService.MarkPlaced(db, payment.OrderGuid, _commerce.KitchenAcceptMinutes);
        var orderId = await db.ExecuteScalarAsync<string>("SELECT OrderId FROM ordOrders WHERE Id=@OrderGuid", new { payment.OrderGuid });
        payment.Status = PaymentStatuses.Success;
        payment.GatewayReference = reference;
        return Map(payment, orderId ?? string.Empty);
    }

    private static async Task<PaymentDto> Fail(System.Data.IDbConnection db, PaymentEntity payment, string reason)
    {
        await db.ExecuteAsync(
            "UPDATE payPayments SET Status=@status, FailureReason=@reason, UpdatedAt=SYSUTCDATETIME() WHERE Id=@Id",
            new { payment.Id, status = PaymentStatuses.Failed, reason });
        await OrderingService.SetStatus(db, payment.OrderGuid, OrderStatuses.PaymentFailed, reason);
        var orderId = await db.ExecuteScalarAsync<string>("SELECT OrderId FROM ordOrders WHERE Id=@OrderGuid", new { payment.OrderGuid });
        payment.Status = PaymentStatuses.Failed;
        return Map(payment, orderId ?? string.Empty);
    }

    private static async Task<PaymentEntity> Require(System.Data.IDbConnection db, string paymentId) =>
        await db.QuerySingleOrDefaultAsync<PaymentEntity>("SELECT * FROM payPayments WHERE PaymentId=@paymentId", new { paymentId })
        ?? throw AppException.NotFound("Payment not found.");

    private static PaymentDto Map(PaymentEntity payment, string orderId) => new()
    {
        PaymentId = payment.PaymentId,
        OrderId = orderId,
        Amount = payment.Amount,
        Method = payment.Method,
        Status = payment.Status,
        GatewayReference = payment.GatewayReference
    };
}

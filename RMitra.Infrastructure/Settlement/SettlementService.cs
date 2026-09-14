using Dapper;
using Microsoft.Extensions.Options;
using RMitra.Application.Abstractions;
using RMitra.Application.Settlement;
using RMitra.BuildingBlocks.Exceptions;
using RMitra.Domain.Common;
using RMitra.Infrastructure.Options;

namespace RMitra.Infrastructure.Settlement;

public class SettlementService : ISettlementService
{
    private readonly ISqlConnectionFactory _connections;
    private readonly IPublicIdGenerator _ids;
    private readonly CommerceOptions _commerce;

    public SettlementService(ISqlConnectionFactory connections, IPublicIdGenerator ids, IOptions<CommerceOptions> commerce)
    {
        _connections = connections;
        _ids = ids;
        _commerce = commerce.Value;
    }

    public SettlementConfigDto GetConfig() => new()
    {
        CommissionPercent = _commerce.CommissionPercent,
        GstPercent = _commerce.GstPercent,
        RiderSharePercent = _commerce.RiderSharePercent,
        ExtraAfter3Km = _commerce.ExtraAfter3Km,
        ExtraAfter6Km = _commerce.ExtraAfter6Km
    };

    public async Task CreditOnDeliveredAsync(Guid orderGuid, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var exists = await db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM setlLedgerLines WHERE OrderGuid=@orderGuid", new { orderGuid });
        if (exists > 0)
            return;

        var order = await db.QuerySingleAsync<(Guid KitchenGuid, decimal ItemTotal, decimal DeliveryFee, string DeliveryMode)>(
            "SELECT KitchenGuid, ItemTotal, DeliveryFee, DeliveryMode FROM ordOrders WHERE Id=@orderGuid", new { orderGuid });

        var commission = Math.Round(order.ItemTotal * _commerce.CommissionPercent / 100m, 2);
        var gst = Math.Round(commission * _commerce.GstPercent / 100m, 2);
        var kitchenNet = order.ItemTotal - commission - gst;
        if (order.DeliveryMode == DeliveryModes.KitchenSelf)
            kitchenNet += order.DeliveryFee;

        await InsertLine(db, "KITCHEN", order.KitchenGuid, orderGuid, kitchenNet, "Kitchen net after 15% + GST");

        if (order.DeliveryMode == DeliveryModes.Rider)
        {
            var riderGuid = await db.ExecuteScalarAsync<Guid?>(
                "SELECT RiderGuid FROM delDeliveries WHERE OrderGuid=@orderGuid", new { orderGuid });
            if (riderGuid is not null)
            {
                var riderNet = Math.Round(order.DeliveryFee * _commerce.RiderSharePercent / 100m, 2);
                await InsertLine(db, "RIDER", riderGuid.Value, orderGuid, riderNet, "Rider 80% of delivery fee");
            }
        }
    }

    public async Task<LedgerDto> GetKitchenLedgerAsync(string kitchenId, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await Kitchen.KitchenService.RequireOwned(db, kitchenId, ownerUserId);
        return await LoadLedger(db, "KITCHEN", kitchen.Id);
    }

    public async Task<LedgerDto> GetRiderLedgerAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var riderId = await db.QuerySingleOrDefaultAsync<Guid?>(
            "SELECT Id FROM delRiders WHERE UserId=@userId", new { userId })
            ?? throw AppException.NotFound("Rider not found.");
        return await LoadLedger(db, "RIDER", riderId);
    }

    public async Task<IReadOnlyList<SettlementDto>> RunWeeklyAsync(CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var ist = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata");
        var nowIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ist);
        var startIst = nowIst.Date.AddDays(-(int)nowIst.DayOfWeek + (int)DayOfWeek.Monday);
        if (nowIst.DayOfWeek == DayOfWeek.Sunday)
            startIst = nowIst.Date.AddDays(-6);
        var endIst = startIst.AddDays(7).AddTicks(-1);
        var start = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(startIst, DateTimeKind.Unspecified), ist);
        var end = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endIst, DateTimeKind.Unspecified), ist);

        var parties = (await db.QueryAsync<(string PartyType, Guid PartyGuid, decimal Amount)>(
            @"SELECT PartyType, PartyGuid, SUM(Amount) AS Amount
              FROM setlLedgerLines
              WHERE CreatedAt>=@start AND CreatedAt<=@end
              GROUP BY PartyType, PartyGuid",
            new { start, end })).ToList();

        var batches = new List<SettlementDto>();
        foreach (var party in parties)
        {
            var batch = new SettlementDto
            {
                SettlementId = await _ids.NextAsync(PublicIdPrefixes.Settlement),
                PartyType = party.PartyType,
                Amount = party.Amount,
                Status = SettlementStatuses.Paid,
                PeriodStart = start,
                PeriodEnd = end,
                InvoiceUrl = $"/invoices/{Guid.NewGuid():N}.pdf",
                Utr = $"UTR{DateTime.UtcNow:yyyyMMddHHmmss}"
            };

            await db.ExecuteAsync(
                @"INSERT INTO setlSettlements
                    (Id, SettlementId, PartyType, PartyGuid, PeriodStart, PeriodEnd, Amount, Status, InvoiceUrl, Utr, CreatedAt)
                  VALUES
                    (@Id, @SettlementId, @PartyType, @PartyGuid, @PeriodStart, @PeriodEnd, @Amount, @Status, @InvoiceUrl, @Utr, SYSUTCDATETIME())",
                new
                {
                    Id = Guid.NewGuid(),
                    batch.SettlementId,
                    party.PartyType,
                    party.PartyGuid,
                    PeriodStart = start,
                    PeriodEnd = end,
                    party.Amount,
                    batch.Status,
                    batch.InvoiceUrl,
                    batch.Utr
                });
            batches.Add(batch);
        }

        return batches;
    }

    public async Task<IReadOnlyList<SettlementDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var rows = await db.QueryAsync<SettlementDto>(
            "SELECT SettlementId, PartyType, Amount, Status, PeriodStart, PeriodEnd, InvoiceUrl, Utr FROM setlSettlements ORDER BY CreatedAt DESC");
        return rows.ToList();
    }

    public async Task<SettlementDto> GetAsync(string settlementId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        return await db.QuerySingleOrDefaultAsync<SettlementDto>(
            "SELECT SettlementId, PartyType, Amount, Status, PeriodStart, PeriodEnd, InvoiceUrl, Utr FROM setlSettlements WHERE SettlementId=@settlementId",
            new { settlementId }) ?? throw AppException.NotFound("Settlement not found.");
    }

    private static async Task InsertLine(System.Data.IDbConnection db, string partyType, Guid partyGuid, Guid orderGuid, decimal amount, string description) =>
        await db.ExecuteAsync(
            @"INSERT INTO setlLedgerLines (PartyType, PartyGuid, OrderGuid, Amount, Description, CreatedAt)
              VALUES (@partyType, @partyGuid, @orderGuid, @amount, @description, SYSUTCDATETIME())",
            new { partyType, partyGuid, orderGuid, amount, description });

    private static async Task<LedgerDto> LoadLedger(System.Data.IDbConnection db, string partyType, Guid partyGuid)
    {
        var lines = (await db.QueryAsync<(string OrderId, decimal Amount, string Description, DateTime CreatedAt)>(
            @"SELECT o.OrderId, l.Amount, l.Description, l.CreatedAt
              FROM setlLedgerLines l
              INNER JOIN ordOrders o ON o.Id=l.OrderGuid
              WHERE l.PartyType=@partyType AND l.PartyGuid=@partyGuid
              ORDER BY l.CreatedAt DESC",
            new { partyType, partyGuid })).ToList();

        return new LedgerDto
        {
            PartyType = partyType,
            WeekTotal = lines.Sum(x => x.Amount),
            Lines = lines.Select(x => new LedgerLineDto
            {
                OrderId = x.OrderId,
                Amount = x.Amount,
                Description = x.Description,
                CreatedAt = x.CreatedAt
            }).ToList()
        };
    }
}

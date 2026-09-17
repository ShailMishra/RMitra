using Dapper;
using Microsoft.Extensions.Options;
using RMitra.Application.Abstractions;
using RMitra.Application.Delivery;
using RMitra.Application.Ordering;
using RMitra.Application.Settlement;
using RMitra.Application.Subscription;
using RMitra.BuildingBlocks.Exceptions;
using RMitra.BuildingBlocks.Security;
using RMitra.Domain.Catalog;
using RMitra.Domain.Common;
using RMitra.Domain.Ordering;
using RMitra.Infrastructure.Kitchen;
using RMitra.Infrastructure.Options;

namespace RMitra.Infrastructure.Ordering;

public class OrderingService : IOrderingService
{
    private readonly ISqlConnectionFactory _connections;
    private readonly IPublicIdGenerator _ids;
    private readonly ISubscriptionService _subscriptions;
    private readonly IRiderService _riders;
    private readonly ISettlementService _settlements;
    private readonly CommerceOptions _commerce;

    public OrderingService(
        ISqlConnectionFactory connections,
        IPublicIdGenerator ids,
        ISubscriptionService subscriptions,
        IRiderService riders,
        ISettlementService settlements,
        IOptions<CommerceOptions> commerce)
    {
        _connections = connections;
        _ids = ids;
        _subscriptions = subscriptions;
        _riders = riders;
        _settlements = settlements;
        _commerce = commerce.Value;
    }

    public async Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var cart = await GetOrCreateCart(db, userId);
        return await LoadCart(db, cart, userId, cancellationToken);
    }

    public async Task<CartDto> PutCartAsync(Guid userId, PutCartRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await KitchenService.Require(db, request.KitchenId);
        if (kitchen.Status != KitchenStatuses.Active)
            throw AppException.Unprocessable("Kitchen is closed or not active.", "KITCHEN_CLOSED");

        var cart = await GetOrCreateCart(db, userId);
        if (cart.KitchenGuid is not null && cart.KitchenGuid != kitchen.Id)
        {
            await db.ExecuteAsync("DELETE FROM tblCartItems WHERE CartId=@Id", new { cart.Id });
        }

        await db.ExecuteAsync("DELETE FROM tblCartItems WHERE CartId=@Id", new { cart.Id });
        foreach (var line in request.Lines)
        {
            var item = await db.QuerySingleOrDefaultAsync<MenuItem>(
                "SELECT * FROM tblMenuItems WHERE ItemId=@ItemId AND Status='PUBLISHED'", line)
                ?? throw AppException.NotFound("Published menu item not found.");
            if (item.KitchenGuid != kitchen.Id)
                throw AppException.Conflict("Cart can contain items from one kitchen only.", "CART_KITCHEN_MISMATCH");

            await db.ExecuteAsync(
                @"INSERT INTO tblCartItems (Id, CartId, MenuItemGuid, Name, UnitPrice, Quantity)
                  VALUES (@Id, @CartId, @MenuItemGuid, @Name, @UnitPrice, @Quantity)",
                new
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    MenuItemGuid = item.Id,
                    item.Name,
                    UnitPrice = item.Price,
                    line.Quantity
                });
        }

        await db.ExecuteAsync("UPDATE tblCarts SET KitchenGuid=@Id, UpdatedAt=SYSUTCDATETIME() WHERE Id=@CartId",
            new { kitchen.Id, CartId = cart.Id });
        cart.KitchenGuid = kitchen.Id;
        return await LoadCart(db, cart, userId, cancellationToken);
    }

    public async Task<OrderDto> PlaceOrderAsync(Guid userId, PlaceOrderRequest request, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (!PaymentMethods.All.Contains(request.PaymentMethod, StringComparer.OrdinalIgnoreCase))
            throw AppException.Validation("paymentMethod must be UPI, CARD, NETBANKING, WALLET, or COD.");

        using var db = _connections.Create();
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existingId = await db.ExecuteScalarAsync<string?>(
                "SELECT OrderId FROM tblOrders WHERE IdempotencyKey=@idempotencyKey AND CustomerUserId=@userId",
                new { idempotencyKey, userId });
            if (existingId is not null)
                return await LoadOrder(db, existingId);
        }

        var cart = await GetOrCreateCart(db, userId);
        var items = (await db.QueryAsync<CartItem>("SELECT * FROM tblCartItems WHERE CartId=@Id", new { cart.Id })).ToList();
        if (items.Count == 0 || cart.KitchenGuid is null)
            throw AppException.Validation("Cart is empty.");

        var kitchen = await db.QuerySingleAsync<Domain.Kitchen.Kitchen>("SELECT * FROM tblKitchens WHERE Id=@KitchenGuid", new { cart.KitchenGuid });
        if (kitchen.Status != KitchenStatuses.Active)
            throw AppException.Unprocessable("Kitchen is closed.", "KITCHEN_CLOSED");

        var address = await db.QuerySingleOrDefaultAsync<Domain.Customer.CustomerAddress>(
            "SELECT * FROM tblAddresses WHERE AddressId=@AddressId AND UserId=@userId",
            new { request.AddressId, userId }) ?? throw AppException.NotFound("Delivery address not found.");

        if (kitchen.Latitude is not null && kitchen.Longitude is not null)
        {
            var geo = new Security.GeoCalculator();
            var distance = geo.DistanceKm((double)address.Latitude, (double)address.Longitude, (double)kitchen.Latitude.Value, (double)kitchen.Longitude.Value);
            if (distance > kitchen.DeliveryRadiusKm)
                throw AppException.Unprocessable("Address is outside the kitchen delivery radius.", "OUT_OF_RANGE");
        }

        var cartDto = await LoadCart(db, cart, userId, cancellationToken);
        var status = request.PaymentMethod.Equals(PaymentMethods.Cod, StringComparison.OrdinalIgnoreCase)
            ? OrderStatuses.Placed
            : OrderStatuses.PaymentPending;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderId = await _ids.NextAsync(PublicIdPrefixes.Order),
            CustomerUserId = userId,
            KitchenGuid = kitchen.Id,
            KitchenPublicId = kitchen.KitchenId,
            AddressGuid = address.Id,
            DeliveryAddress = $"{address.Line1}, {address.City}, {address.State} {address.Pincode}",
            ItemTotal = cartDto.ItemTotal,
            DiscountAmount = cartDto.DiscountAmount,
            DeliveryFee = cartDto.DeliveryFee,
            TipAmount = request.Tip,
            GrandTotal = cartDto.GrandTotal + request.Tip,
            PaymentMethod = request.PaymentMethod.ToUpperInvariant(),
            Status = status,
            DeliveryMode = DeliveryModes.Rider,
            Instructions = request.Instructions,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AcceptedDeadlineAt = status == OrderStatuses.Placed ? DateTime.UtcNow.AddMinutes(_commerce.KitchenAcceptMinutes) : null
        };

        await db.ExecuteAsync(
            @"INSERT INTO tblOrders
                (Id, OrderId, CustomerUserId, KitchenGuid, KitchenPublicId, AddressGuid, DeliveryAddress,
                 ItemTotal, DiscountAmount, DeliveryFee, TaxAmount, TipAmount, GrandTotal, PaymentMethod,
                 Status, DeliveryMode, Instructions, IdempotencyKey, CreatedAt, UpdatedAt, AcceptedDeadlineAt)
              VALUES
                (@Id, @OrderId, @CustomerUserId, @KitchenGuid, @KitchenPublicId, @AddressGuid, @DeliveryAddress,
                 @ItemTotal, @DiscountAmount, @DeliveryFee, 0, @TipAmount, @GrandTotal, @PaymentMethod,
                 @Status, @DeliveryMode, @Instructions, @IdempotencyKey, @CreatedAt, @UpdatedAt, @AcceptedDeadlineAt)",
            order);

        foreach (var item in items)
        {
            await db.ExecuteAsync(
                @"INSERT INTO tblOrderItems (Id, OrderGuid, MenuItemGuid, Name, UnitPrice, Quantity, LineTotal)
                  VALUES (@Id, @OrderGuid, @MenuItemGuid, @Name, @UnitPrice, @Quantity, @LineTotal)",
                new
                {
                    Id = Guid.NewGuid(),
                    OrderGuid = order.Id,
                    item.MenuItemGuid,
                    item.Name,
                    item.UnitPrice,
                    item.Quantity,
                    LineTotal = item.UnitPrice * item.Quantity
                });
        }

        await History(db, order.Id, order.Status, "Order created");
        await db.ExecuteAsync("DELETE FROM tblCartItems WHERE CartId=@Id", new { cart.Id });
        await db.ExecuteAsync("UPDATE tblCarts SET KitchenGuid=NULL WHERE Id=@Id", new { cart.Id });
        return await LoadOrder(db, order.OrderId);
    }

    public async Task<IReadOnlyList<OrderDto>> GetMyOrdersAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var ids = role == Roles.KitchenOwner
            ? await db.QueryAsync<string>(
                @"SELECT o.OrderId FROM tblOrders o
                  INNER JOIN tblKitchens k ON k.Id=o.KitchenGuid
                  WHERE k.OwnerUserId=@userId ORDER BY o.CreatedAt DESC", new { userId })
            : await db.QueryAsync<string>(
                "SELECT OrderId FROM tblOrders WHERE CustomerUserId=@userId ORDER BY CreatedAt DESC", new { userId });

        var list = new List<OrderDto>();
        foreach (var id in ids)
            list.Add(await LoadOrder(db, id));
        return list;
    }

    public async Task<OrderDto> GetOrderAsync(string orderId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var order = await LoadOrder(db, orderId);
        if (role == Roles.Admin || order.KitchenId == await OwnerKitchen(db, userId) || await IsCustomer(db, orderId, userId))
            return order;
        if (role == Roles.KitchenOwner)
        {
            var owner = await db.ExecuteScalarAsync<Guid>(
                "SELECT k.OwnerUserId FROM tblOrders o INNER JOIN tblKitchens k ON k.Id=o.KitchenGuid WHERE o.OrderId=@orderId",
                new { orderId });
            if (owner == userId) return order;
        }
        if (await IsCustomer(db, orderId, userId)) return order;
        throw AppException.Forbidden();
    }

    public async Task<OrderDto> CancelAsync(string orderId, Guid userId, CancelOrderRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var order = await RequireOrder(db, orderId);
        if (order.CustomerUserId != userId)
            throw AppException.Forbidden();
        if (order.Status is not OrderStatuses.PaymentPending and not OrderStatuses.Placed)
            throw AppException.Validation("Customer can cancel only before the kitchen accepts.");

        await SetStatus(db, order.Id, OrderStatuses.Cancelled, request.Reason);
        return await LoadOrder(db, orderId);
    }

    public async Task<IReadOnlyList<OrderDto>> GetKitchenOrdersAsync(string kitchenId, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        await KitchenService.RequireOwned(db, kitchenId, ownerUserId);
        var ids = await db.QueryAsync<string>(
            @"SELECT o.OrderId FROM tblOrders o
              INNER JOIN tblKitchens k ON k.Id=o.KitchenGuid
              WHERE k.KitchenId=@kitchenId ORDER BY o.CreatedAt DESC", new { kitchenId });
        var list = new List<OrderDto>();
        foreach (var id in ids)
            list.Add(await LoadOrder(db, id));
        return list;
    }

    public async Task<OrderDto> AcceptAsync(string kitchenId, string orderId, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var order = await RequireKitchenOrder(db, kitchenId, orderId, ownerUserId);
        if (order.Status != OrderStatuses.Placed)
            throw AppException.Validation("Only PLACED orders can be accepted.");
        await SetStatus(db, order.Id, OrderStatuses.Accepted, "Kitchen accepted");
        return await LoadOrder(db, orderId);
    }

    public async Task<OrderDto> RejectAsync(string kitchenId, string orderId, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var order = await RequireKitchenOrder(db, kitchenId, orderId, ownerUserId);
        var next = order.PaymentMethod == PaymentMethods.Cod ? OrderStatuses.Rejected : OrderStatuses.Refunded;
        await SetStatus(db, order.Id, next, "Kitchen rejected");
        if (next == OrderStatuses.Refunded)
            await db.ExecuteAsync("UPDATE tblPayments SET Status='REFUNDED', UpdatedAt=SYSUTCDATETIME() WHERE OrderGuid=@Id", new { order.Id });
        return await LoadOrder(db, orderId);
    }

    public async Task<OrderDto> UpdateKitchenStatusAsync(string kitchenId, string orderId, Guid ownerUserId, string status, CancellationToken cancellationToken = default)
    {
        if (status is not OrderStatuses.Preparing and not OrderStatuses.Ready and not OrderStatuses.Delivered)
            throw AppException.Validation("Kitchen can set PREPARING, READY, or DELIVERED for self-delivery.");

        using var db = _connections.Create();
        var order = await RequireKitchenOrder(db, kitchenId, orderId, ownerUserId);
        if (status == OrderStatuses.Delivered)
        {
            if (order.DeliveryMode != DeliveryModes.KitchenSelf)
                throw AppException.Validation("Only kitchen self-delivery can be marked DELIVERED by the kitchen.");
            await SetStatus(db, order.Id, OrderStatuses.Delivered, "Kitchen self-delivery completed");
            await _settlements.CreditOnDeliveredAsync(order.Id, cancellationToken);
            return await LoadOrder(db, orderId);
        }

        if (order.Status is not OrderStatuses.Accepted and not OrderStatuses.Preparing)
            throw AppException.Validation("Order is not in a kitchen-updatable state.");

        await SetStatus(db, order.Id, status, $"Kitchen set {status}");
        if (status == OrderStatuses.Ready)
            await _riders.StartAssignmentForReadyOrderAsync(order.Id, cancellationToken);
        return await LoadOrder(db, orderId);
    }

    public async Task RateAsync(string orderId, Guid userId, RatingRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var order = await RequireOrder(db, orderId);
        if (order.CustomerUserId != userId)
            throw AppException.Forbidden();
        if (order.Status != OrderStatuses.Delivered)
            throw AppException.Validation("Rating is allowed only after delivery.");

        var exists = await db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM tblReviews WHERE OrderGuid=@Id", new { order.Id });
        if (exists > 0)
            throw AppException.Conflict("Order already reviewed.", "REVIEW_EXISTS");

        await db.ExecuteAsync(
            @"INSERT INTO tblReviews (Id, OrderGuid, KitchenGuid, CustomerUserId, Rating, RiderRating, Comments, CreatedAt)
              VALUES (@Id, @OrderGuid, @KitchenGuid, @CustomerUserId, @Rating, @RiderRating, @Comments, SYSUTCDATETIME())",
            new
            {
                Id = Guid.NewGuid(),
                OrderGuid = order.Id,
                order.KitchenGuid,
                CustomerUserId = userId,
                request.Rating,
                request.RiderRating,
                request.Comments
            });
    }

    internal static async Task SetStatus(System.Data.IDbConnection db, Guid orderGuid, string status, string remarks)
    {
        await db.ExecuteAsync("UPDATE tblOrders SET Status=@status, UpdatedAt=SYSUTCDATETIME() WHERE Id=@orderGuid", new { orderGuid, status });
        await History(db, orderGuid, status, remarks);
    }

    internal static async Task MarkPlaced(System.Data.IDbConnection db, Guid orderGuid, int acceptMinutes)
    {
        await db.ExecuteAsync(
            "UPDATE tblOrders SET Status=@status, AcceptedDeadlineAt=DATEADD(MINUTE,@acceptMinutes,SYSUTCDATETIME()), UpdatedAt=SYSUTCDATETIME() WHERE Id=@orderGuid",
            new { orderGuid, status = OrderStatuses.Placed, acceptMinutes });
        await History(db, orderGuid, OrderStatuses.Placed, "Payment successful");
    }

    private static async Task History(System.Data.IDbConnection db, Guid orderGuid, string status, string remarks) =>
        await db.ExecuteAsync(
            "INSERT INTO tblOrderStatusHistory (OrderGuid, Status, Remarks, CreatedAt) VALUES (@orderGuid, @status, @remarks, SYSUTCDATETIME())",
            new { orderGuid, status, remarks });

    private static async Task<Cart> GetOrCreateCart(System.Data.IDbConnection db, Guid userId)
    {
        var cart = await db.QuerySingleOrDefaultAsync<Cart>("SELECT * FROM tblCarts WHERE UserId=@userId", new { userId });
        if (cart is not null) return cart;
        cart = new Cart { Id = Guid.NewGuid(), UserId = userId, UpdatedAt = DateTime.UtcNow };
        await db.ExecuteAsync("INSERT INTO tblCarts (Id, UserId, UpdatedAt) VALUES (@Id, @UserId, @UpdatedAt)", cart);
        return cart;
    }

    private async Task<CartDto> LoadCart(System.Data.IDbConnection db, Cart cart, Guid userId, CancellationToken cancellationToken)
    {
        var items = (await db.QueryAsync<CartItem>("SELECT * FROM tblCartItems WHERE CartId=@Id", new { cart.Id })).ToList();
        var itemIds = items.Select(i => i.MenuItemGuid).ToList();
        var publicIds = itemIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await db.QueryAsync<(Guid Id, string ItemId)>("SELECT Id, ItemId FROM tblMenuItems WHERE Id IN @ids", new { ids = itemIds }))
                .ToDictionary(x => x.Id, x => x.ItemId);

        var lines = items.Select(x => new CartItemDto
        {
            ItemId = publicIds.GetValueOrDefault(x.MenuItemGuid, string.Empty),
            Name = x.Name,
            UnitPrice = x.UnitPrice,
            Quantity = x.Quantity
        }).ToList();

        var itemTotal = lines.Sum(x => x.LineTotal);
        var plus = await _subscriptions.HasActiveHomelyPlusAsync(userId, cancellationToken);
        var discount = plus ? Math.Round(itemTotal * _commerce.HomelyPlusItemDiscountPercent / 100m, 2) : 0;
        var delivery = plus && itemTotal >= _commerce.HomelyPlusFreeDeliveryMin ? 0 : _commerce.DeliveryFee;
        var kitchenId = cart.KitchenGuid is null
            ? null
            : await db.ExecuteScalarAsync<string>("SELECT KitchenId FROM tblKitchens WHERE Id=@KitchenGuid", new { cart.KitchenGuid });

        return new CartDto
        {
            KitchenId = kitchenId,
            Lines = lines,
            ItemTotal = itemTotal,
            DiscountAmount = discount,
            DeliveryFee = delivery,
            GrandTotal = itemTotal - discount + delivery,
            HomelyPlusApplied = plus
        };
    }

    internal static async Task<Order> RequireOrder(System.Data.IDbConnection db, string orderId) =>
        await db.QuerySingleOrDefaultAsync<Order>("SELECT * FROM tblOrders WHERE OrderId=@orderId", new { orderId })
        ?? throw AppException.NotFound("Order not found.");

    private static async Task<Order> RequireKitchenOrder(System.Data.IDbConnection db, string kitchenId, string orderId, Guid ownerUserId)
    {
        await KitchenService.RequireOwned(db, kitchenId, ownerUserId);
        var order = await RequireOrder(db, orderId);
        var matches = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM tblKitchens WHERE Id=@KitchenGuid AND KitchenId=@kitchenId",
            new { order.KitchenGuid, kitchenId });
        if (matches == 0)
            throw AppException.Forbidden();
        return order;
    }

    internal static async Task<OrderDto> LoadOrder(System.Data.IDbConnection db, string orderId)
    {
        var order = await RequireOrder(db, orderId);
        var items = await db.QueryAsync<OrderItem>("SELECT * FROM tblOrderItems WHERE OrderGuid=@Id", new { order.Id });
        var timeline = (await db.QueryAsync<string>(
            "SELECT Status FROM tblOrderStatusHistory WHERE OrderGuid=@Id ORDER BY CreatedAt", new { order.Id })).ToList();
        var tracking = await db.QuerySingleOrDefaultAsync<(decimal? Latitude, decimal? Longitude)?>(
            "SELECT Latitude, Longitude FROM tblDeliveries WHERE OrderGuid=@Id", new { order.Id });

        return new OrderDto
        {
            OrderId = order.OrderId,
            KitchenId = order.KitchenPublicId,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            DeliveryMode = order.DeliveryMode,
            DeliveryAddress = order.DeliveryAddress,
            ItemTotal = order.ItemTotal,
            DiscountAmount = order.DiscountAmount,
            DeliveryFee = order.DeliveryFee,
            TipAmount = order.TipAmount,
            GrandTotal = order.GrandTotal,
            Instructions = order.Instructions,
            CreatedAt = order.CreatedAt,
            Timeline = timeline,
            RiderLatitude = tracking?.Latitude,
            RiderLongitude = tracking?.Longitude,
            Items = items.Select(x => new CartItemDto
            {
                Name = x.Name,
                UnitPrice = x.UnitPrice,
                Quantity = x.Quantity
            }).ToList()
        };
    }

    private static async Task<bool> IsCustomer(System.Data.IDbConnection db, string orderId, Guid userId) =>
        await db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM tblOrders WHERE OrderId=@orderId AND CustomerUserId=@userId", new { orderId, userId }) > 0;

    private static async Task<string?> OwnerKitchen(System.Data.IDbConnection db, Guid userId) =>
        await db.ExecuteScalarAsync<string?>("SELECT KitchenId FROM tblKitchens WHERE OwnerUserId=@userId", new { userId });
}

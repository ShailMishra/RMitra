using System.ComponentModel.DataAnnotations;

namespace RMitra.Application.Ordering;

public class CartLineRequest
{
    [Required] public string ItemId { get; set; } = string.Empty;
    [Range(1, 20)] public int Quantity { get; set; } = 1;
}

public class PutCartRequest
{
    [Required] public string KitchenId { get; set; } = string.Empty;
    [MinLength(1)] public List<CartLineRequest> Lines { get; set; } = [];
}

public class CartDto
{
    public string? KitchenId { get; set; }
    public IReadOnlyList<CartItemDto> Lines { get; set; } = [];
    public decimal ItemTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal GrandTotal { get; set; }
    public bool HomelyPlusApplied { get; set; }
}

public class CartItemDto
{
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public class PlaceOrderRequest
{
    [Required] public string AddressId { get; set; } = string.Empty;
    [Required] public string PaymentMethod { get; set; } = "UPI";
    public decimal Tip { get; set; }
    public string? Instructions { get; set; }
}

public class OrderDto
{
    public string OrderId { get; set; } = string.Empty;
    public string KitchenId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string DeliveryMode { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal ItemTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal TipAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Instructions { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<CartItemDto> Items { get; set; } = [];
    public IReadOnlyList<string> Timeline { get; set; } = [];
    public decimal? RiderLatitude { get; set; }
    public decimal? RiderLongitude { get; set; }
}

public class CancelOrderRequest
{
    public string Reason { get; set; } = "Changed my mind";
}

public class RatingRequest
{
    [Range(1, 5)] public int Rating { get; set; }
    [Range(1, 5)] public int? RiderRating { get; set; }
    public string? Comments { get; set; }
}

public interface IOrderingService
{
    Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CartDto> PutCartAsync(Guid userId, PutCartRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto> PlaceOrderAsync(Guid userId, PlaceOrderRequest request, string? idempotencyKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetMyOrdersAsync(Guid userId, string role, CancellationToken cancellationToken = default);
    Task<OrderDto> GetOrderAsync(string orderId, Guid userId, string role, CancellationToken cancellationToken = default);
    Task<OrderDto> CancelAsync(string orderId, Guid userId, CancelOrderRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetKitchenOrdersAsync(string kitchenId, Guid ownerUserId, CancellationToken cancellationToken = default);
    Task<OrderDto> AcceptAsync(string kitchenId, string orderId, Guid ownerUserId, CancellationToken cancellationToken = default);
    Task<OrderDto> RejectAsync(string kitchenId, string orderId, Guid ownerUserId, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateKitchenStatusAsync(string kitchenId, string orderId, Guid ownerUserId, string status, CancellationToken cancellationToken = default);
    Task RateAsync(string orderId, Guid userId, RatingRequest request, CancellationToken cancellationToken = default);
}

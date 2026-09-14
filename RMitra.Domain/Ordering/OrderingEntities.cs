namespace RMitra.Domain.Ordering;

public class Cart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? KitchenGuid { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid MenuItemGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class Order
{
    public Guid Id { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public Guid CustomerUserId { get; set; }
    public Guid KitchenGuid { get; set; }
    public string KitchenPublicId { get; set; } = string.Empty;
    public Guid AddressGuid { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal ItemTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TipAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DeliveryMode { get; set; } = "RIDER";
    public string? Instructions { get; set; }
    public string? CancelReason { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? AcceptedDeadlineAt { get; set; }
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderGuid { get; set; }
    public Guid MenuItemGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

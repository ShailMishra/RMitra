namespace RMitra.Domain.Catalog;

public class Cuisine
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class MenuCategory
{
    public Guid Id { get; set; }
    public Guid KitchenGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class MenuItem
{
    public Guid Id { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public Guid KitchenGuid { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsVeg { get; set; }
    public string Status { get; set; } = "DRAFT";
    public string? PhotoUrl { get; set; }
    public int PreparationMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

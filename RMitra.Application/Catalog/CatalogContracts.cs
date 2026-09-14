using System.ComponentModel.DataAnnotations;

namespace RMitra.Application.Catalog;

public class CuisineDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class MenuCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class MenuItemDto
{
    public string ItemId { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsVeg { get; set; }
    public string Status { get; set; } = "DRAFT";
    public string? PhotoUrl { get; set; }
}

public class CreateCategoryRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class CreateMenuItemRequest
{
    [Required] public Guid CategoryId { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Range(1, 100000)] public decimal Price { get; set; }
    public bool IsVeg { get; set; } = true;
}

public class PatchMenuItemRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public bool? IsVeg { get; set; }
    public string? Status { get; set; }
}

public class StorefrontDto
{
    public string KitchenId { get; set; } = string.Empty;
    public string KitchenName { get; set; } = string.Empty;
    public string? OpenTime { get; set; }
    public string? CloseTime { get; set; }
    public bool DeliversToAddress { get; set; }
    public IReadOnlyList<MenuCategoryDto> Categories { get; set; } = [];
    public IReadOnlyList<MenuItemDto> Dishes { get; set; } = [];
}

public interface ICatalogService
{
    Task<IReadOnlyList<CuisineDto>> GetCuisinesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuCategoryDto>> GetCategoriesAsync(string kitchenId, CancellationToken cancellationToken = default);
    Task<MenuCategoryDto> AddCategoryAsync(string kitchenId, Guid ownerUserId, CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<MenuItemDto> AddItemAsync(string kitchenId, Guid ownerUserId, CreateMenuItemRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuItemDto>> GetItemsAsync(string kitchenId, CancellationToken cancellationToken = default);
    Task<MenuItemDto> PatchItemAsync(string itemId, Guid ownerUserId, PatchMenuItemRequest request, CancellationToken cancellationToken = default);
    Task AddPhotoAsync(string itemId, Guid ownerUserId, string fileUrl, CancellationToken cancellationToken = default);
    Task SoftDeleteItemAsync(string itemId, Guid ownerUserId, CancellationToken cancellationToken = default);
    Task<StorefrontDto> GetStorefrontAsync(string kitchenId, decimal? latitude, decimal? longitude, CancellationToken cancellationToken = default);
}

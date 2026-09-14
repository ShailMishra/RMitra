using Dapper;
using RMitra.Application.Abstractions;
using RMitra.Application.Catalog;
using RMitra.BuildingBlocks.Exceptions;
using RMitra.Domain.Catalog;
using RMitra.Domain.Common;
using RMitra.Infrastructure.Kitchen;

namespace RMitra.Infrastructure.Catalog;

public class CatalogService : ICatalogService
{
    private readonly ISqlConnectionFactory _connections;
    private readonly IPublicIdGenerator _ids;
    private readonly IGeoCalculator _geo;

    public CatalogService(ISqlConnectionFactory connections, IPublicIdGenerator ids, IGeoCalculator geo)
    {
        _connections = connections;
        _ids = ids;
        _geo = geo;
    }

    public async Task<IReadOnlyList<CuisineDto>> GetCuisinesAsync(CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var rows = await db.QueryAsync<Cuisine>("SELECT Code, Name, CAST(1 AS BIT) AS IsActive FROM mstCuisines ORDER BY Name");
        return rows.Select(x => new CuisineDto { Code = x.Code, Name = x.Name }).ToList();
    }

    public async Task<IReadOnlyList<MenuCategoryDto>> GetCategoriesAsync(string kitchenId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await KitchenService.Require(db, kitchenId);
        var rows = await db.QueryAsync<MenuCategory>(
            "SELECT * FROM catMenuCategories WHERE KitchenGuid=@Id AND IsActive=1 ORDER BY DisplayOrder, Name",
            new { kitchen.Id });
        return rows.Select(Map).ToList();
    }

    public async Task<MenuCategoryDto> AddCategoryAsync(string kitchenId, Guid ownerUserId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await KitchenService.RequireOwned(db, kitchenId, ownerUserId);
        await KitchenService.EnsureListingEnabled(db, kitchen.Id);

        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            KitchenGuid = kitchen.Id,
            Name = request.Name.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };
        await db.ExecuteAsync(
            "INSERT INTO catMenuCategories (Id, KitchenGuid, Name, DisplayOrder, IsActive, CreatedAt) VALUES (@Id, @KitchenGuid, @Name, @DisplayOrder, 1, SYSUTCDATETIME())",
            category);
        await MarkMenuSetup(db, kitchen);
        return Map(category);
    }

    public async Task<MenuItemDto> AddItemAsync(string kitchenId, Guid ownerUserId, CreateMenuItemRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await KitchenService.RequireOwned(db, kitchenId, ownerUserId);
        await KitchenService.EnsureListingEnabled(db, kitchen.Id);

        var item = new MenuItem
        {
            Id = Guid.NewGuid(),
            ItemId = await _ids.NextAsync("ITM"),
            KitchenGuid = kitchen.Id,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Description = request.Description,
            Price = request.Price,
            IsVeg = request.IsVeg,
            Status = "DRAFT",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await db.ExecuteAsync(
            @"INSERT INTO catMenuItems
                (Id, ItemId, KitchenGuid, CategoryId, Name, Description, Price, IsVeg, Status, PreparationMinutes, CreatedAt, UpdatedAt)
              VALUES
                (@Id, @ItemId, @KitchenGuid, @CategoryId, @Name, @Description, @Price, @IsVeg, @Status, 20, @CreatedAt, @UpdatedAt)",
            item);
        await MarkMenuSetup(db, kitchen);
        return Map(item);
    }

    public async Task<IReadOnlyList<MenuItemDto>> GetItemsAsync(string kitchenId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await KitchenService.Require(db, kitchenId);
        var rows = await db.QueryAsync<MenuItem>("SELECT * FROM catMenuItems WHERE KitchenGuid=@Id AND Status<>'DELETED' ORDER BY Name", new { kitchen.Id });
        return rows.Select(Map).ToList();
    }

    public async Task<MenuItemDto> PatchItemAsync(string itemId, Guid ownerUserId, PatchMenuItemRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var item = await db.QuerySingleOrDefaultAsync<MenuItem>("SELECT * FROM catMenuItems WHERE ItemId=@itemId", new { itemId })
                   ?? throw AppException.NotFound("Menu item not found.");
        var kitchen = await db.QuerySingleAsync<Domain.Kitchen.Kitchen>("SELECT * FROM kitKitchens WHERE Id=@KitchenGuid", new { item.KitchenGuid });
        if (kitchen.OwnerUserId != ownerUserId)
            throw AppException.Forbidden();
        await KitchenService.EnsureListingEnabled(db, kitchen.Id);

        if (string.Equals(request.Status, "PUBLISHED", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(item.PhotoUrl) && string.IsNullOrWhiteSpace(request.Name))
        {
            if (string.IsNullOrWhiteSpace(item.PhotoUrl))
                throw AppException.Unprocessable("One photo is required to publish.", "MENU_REQUIRED");
        }

        item.Name = request.Name ?? item.Name;
        item.Description = request.Description ?? item.Description;
        item.Price = request.Price ?? item.Price;
        item.IsVeg = request.IsVeg ?? item.IsVeg;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (request.Status.Equals("PUBLISHED", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(item.PhotoUrl))
                throw AppException.Unprocessable("One photo is required to publish.", "MENU_REQUIRED");
            item.Status = request.Status.ToUpperInvariant();
        }

        await db.ExecuteAsync(
            @"UPDATE catMenuItems
              SET Name=@Name, Description=@Description, Price=@Price, IsVeg=@IsVeg, Status=@Status, UpdatedAt=SYSUTCDATETIME()
              WHERE ItemId=@ItemId",
            item);
        return Map(item);
    }

    public async Task AddPhotoAsync(string itemId, Guid ownerUserId, string fileUrl, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var item = await db.QuerySingleOrDefaultAsync<MenuItem>("SELECT * FROM catMenuItems WHERE ItemId=@itemId", new { itemId })
                   ?? throw AppException.NotFound("Menu item not found.");
        var owner = await db.ExecuteScalarAsync<Guid>("SELECT OwnerUserId FROM kitKitchens WHERE Id=@KitchenGuid", new { item.KitchenGuid });
        if (owner != ownerUserId)
            throw AppException.Forbidden();

        await db.ExecuteAsync(
            "UPDATE catMenuItems SET PhotoUrl=@fileUrl, UpdatedAt=SYSUTCDATETIME() WHERE ItemId=@itemId",
            new { itemId, fileUrl });
    }

    public async Task SoftDeleteItemAsync(string itemId, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        await PatchItemAsync(itemId, ownerUserId, new PatchMenuItemRequest { Status = "DELETED" }, cancellationToken);
    }

    public async Task<StorefrontDto> GetStorefrontAsync(string kitchenId, decimal? latitude, decimal? longitude, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await KitchenService.Require(db, kitchenId);
        if (kitchen.Status != KitchenStatuses.Active)
            throw AppException.NotFound("Kitchen is not active.");

        var delivers = true;
        if (latitude is not null && longitude is not null && kitchen.Latitude is not null && kitchen.Longitude is not null)
        {
            var distance = _geo.DistanceKm((double)latitude.Value, (double)longitude.Value, (double)kitchen.Latitude.Value, (double)kitchen.Longitude.Value);
            delivers = distance <= kitchen.DeliveryRadiusKm;
        }

        var categories = await GetCategoriesAsync(kitchenId, cancellationToken);
        var items = (await db.QueryAsync<MenuItem>(
            "SELECT * FROM catMenuItems WHERE KitchenGuid=@Id AND Status='PUBLISHED'", new { kitchen.Id })).Select(Map).ToList();

        return new StorefrontDto
        {
            KitchenId = kitchen.KitchenId,
            KitchenName = kitchen.KitchenName,
            OpenTime = kitchen.OpenTime,
            CloseTime = kitchen.CloseTime,
            DeliversToAddress = delivers,
            Categories = categories,
            Dishes = items
        };
    }

    private static async Task MarkMenuSetup(System.Data.IDbConnection db, Domain.Kitchen.Kitchen kitchen)
    {
        if (kitchen.Status == KitchenStatuses.Approved)
        {
            await db.ExecuteAsync(
                "UPDATE kitKitchens SET Status=@status, UpdatedAt=SYSUTCDATETIME() WHERE Id=@Id",
                new { kitchen.Id, status = KitchenStatuses.MenuSetup });
        }
    }

    private static MenuCategoryDto Map(MenuCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        DisplayOrder = category.DisplayOrder
    };

    private static MenuItemDto Map(MenuItem item) => new()
    {
        ItemId = item.ItemId,
        CategoryId = item.CategoryId,
        Name = item.Name,
        Description = item.Description,
        Price = item.Price,
        IsVeg = item.IsVeg,
        Status = item.Status,
        PhotoUrl = item.PhotoUrl
    };
}

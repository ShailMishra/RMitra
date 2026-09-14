using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMitra.Api.ExtensionMethod;
using RMitra.Api.Uploads;
using RMitra.Application.Catalog;
using RMitra.BuildingBlocks.Security;

namespace RMitra.Api.Controllers;

[Authorize(Roles = Roles.KitchenOwner)]
[Route("/api/homely/menu")]
[Tags("Catalog")]
public class MenuController : ApiControllerBase
{
    private readonly ICatalogService _catalog;
    private readonly ICurrentUser _currentUser;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MenuController> _logger;

    public MenuController(ICatalogService catalog, ICurrentUser currentUser, IWebHostEnvironment env, ILogger<MenuController> logger)
    {
        _catalog = catalog;
        _currentUser = currentUser;
        _env = env;
        _logger = logger;
    }

    [HttpPost]
    [Route("upload_item_photo/{itemId}")]
    public async Task<IActionResult> UploadItemPhoto(string itemId, IFormFile file, CancellationToken cancellationToken)
    {
        var url = await LocalFileStore.SaveAsync(_env, file, cancellationToken);
        await _catalog.AddPhotoAsync(itemId, _currentUser.UserId, url, cancellationToken);
        return this.OkCustom(null, new { photoUrl = url }, _logger);
    }

    [HttpPost]
    [Route("update_item/{itemId}")]
    public async Task<IActionResult> UpdateItem(string itemId, [FromBody] PatchMenuItemRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _catalog.PatchItemAsync(itemId, _currentUser.UserId, request, cancellationToken), _logger);
    }

    [HttpPost]
    [Route("delete_item/{itemId}")]
    public async Task<IActionResult> DeleteItem(string itemId, CancellationToken cancellationToken)
    {
        await _catalog.SoftDeleteItemAsync(itemId, _currentUser.UserId, cancellationToken);
        return this.OkCustom(null, new { deleted = true }, _logger);
    }
}

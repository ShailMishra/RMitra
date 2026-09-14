namespace RMitra.Api.Uploads;

public static class LocalFileStore
{
    public static async Task<string> SaveAsync(IWebHostEnvironment env, IFormFile file, CancellationToken cancellationToken)
    {
        var folder = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(folder, name);
        await using var stream = File.Create(path);
        await file.CopyToAsync(stream, cancellationToken);
        return $"/uploads/{name}";
    }
}

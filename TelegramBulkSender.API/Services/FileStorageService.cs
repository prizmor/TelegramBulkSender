using Microsoft.AspNetCore.Http;

namespace TelegramBulkSender.API.Services;

public class FileStorageService
{
    private readonly string _uploadsRoot;

    public FileStorageService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredPath = configuration.GetValue<string>("UPLOADS_PATH");
        _uploadsRoot = configuredPath ?? Path.Combine(environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(_uploadsRoot);
    }

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(_uploadsRoot, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);
        return filePath;
    }

    public void Delete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}

using Microsoft.Extensions.Options;
using SmartJob.API.Exceptions;
using SmartJob.API.Options;

namespace SmartJob.API.Services;

public class LocalFileStorageService : ILocalFileStorageService
{
    private static readonly HashSet<string> AllowedResumeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx"
    };

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private readonly IWebHostEnvironment _environment;
    private readonly FileStorageOptions _options;

    public LocalFileStorageService(IWebHostEnvironment environment, IOptions<FileStorageOptions> options)
    {
        _environment = environment;
        _options = options.Value;
    }

    public Task<string> SaveAvatarAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        ValidateFile(file, AllowedImageExtensions, "avatar");
        return SaveFileAsync(file, _options.AvatarFolder, cancellationToken);
    }

    public async Task<(string relativeUrl, string storedFileName)> SaveResumeAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        ValidateFile(file, AllowedResumeExtensions, "resume");
        var relativeUrl = await SaveFileAsync(file, _options.ResumeFolder, cancellationToken);
        return (relativeUrl, Path.GetFileName(relativeUrl));
    }

    public void DeleteRelativeFile(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
        {
            return;
        }

        var relativePath = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var absolutePath = Path.Combine(webRootPath, relativePath);

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }

    private async Task<string> SaveFileAsync(IFormFile file, string folderName, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName);
        var uploadsRoot = Path.Combine(_environment.ContentRootPath, _options.RootPath, folderName);
        Directory.CreateDirectory(uploadsRoot);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(uploadsRoot, storedFileName);

        await using var stream = File.Create(absolutePath);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/{folderName}/{storedFileName}";
    }

    private static void ValidateFile(IFormFile file, HashSet<string> allowedExtensions, string fileType)
    {
        if (file.Length <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"The {fileType} file is empty.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!allowedExtensions.Contains(extension))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"Unsupported {fileType} file type: {extension}");
        }
    }
}

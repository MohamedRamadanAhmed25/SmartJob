namespace SmartJob.API.Services;

public interface ILocalFileStorageService
{
    Task<string> SaveAvatarAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<(string relativeUrl, string storedFileName)> SaveResumeAsync(IFormFile file, CancellationToken cancellationToken = default);
    void DeleteRelativeFile(string? relativeUrl);
}

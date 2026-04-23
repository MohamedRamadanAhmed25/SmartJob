using SmartJob.API.DTOs.Resumes;

namespace SmartJob.API.Services;

public interface IResumeService
{
    Task<IReadOnlyList<ResumeDto>> GetMyResumesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ResumeDto> UploadAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid resumeId, CancellationToken cancellationToken = default);
    Task<ResumeDto> SetDefaultAsync(Guid userId, Guid resumeId, CancellationToken cancellationToken = default);
}

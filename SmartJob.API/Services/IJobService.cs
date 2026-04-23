using SmartJob.API.DTOs.Jobs;

namespace SmartJob.API.Services;

public interface IJobService
{
    Task<PagedResult<JobDto>> GetJobsAsync(JobFilterRequest filter, CancellationToken cancellationToken = default);
    Task<PagedResult<JobDto>> GetRecommendedJobsAsync(Guid seekerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<JobDto> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobDto> CreateJobAsync(Guid employerId, CreateJobRequest request, CancellationToken cancellationToken = default);
    Task<JobDto> UpdateJobAsync(Guid id, Guid employerId, UpdateJobRequest request, CancellationToken cancellationToken = default);
    Task PatchStatusAsync(Guid id, Guid employerId, string status, CancellationToken cancellationToken = default);
    Task DeleteJobAsync(Guid id, Guid employerId, CancellationToken cancellationToken = default);
    Task<AIAnalysisDto> GetAIAnalysisAsync(Guid jobId, Guid seekerId, CancellationToken cancellationToken = default);
}

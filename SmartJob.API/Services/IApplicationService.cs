using SmartJob.API.DTOs.Applications;
using SmartJob.API.DTOs.Jobs;

namespace SmartJob.API.Services;

public interface IApplicationService
{
    Task<ApplicationDto> ApplyAsync(Guid seekerId, ApplyRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<ApplicationDto>> GetMyApplicationsAsync(Guid seekerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<ApplicationDto>> GetJobApplicationsAsync(Guid jobId, Guid employerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ApplicationDto> GetApplicationAsync(Guid id, Guid requesterId, CancellationToken cancellationToken = default);
    Task<ApplicationDto> PatchStatusAsync(Guid applicationId, Guid employerId, string status, CancellationToken cancellationToken = default);
}

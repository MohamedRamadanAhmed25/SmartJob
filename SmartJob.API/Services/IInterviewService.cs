using SmartJob.API.DTOs.Interviews;
using SmartJob.API.DTOs.Jobs;

namespace SmartJob.API.Services;

public interface IInterviewService
{
    Task<InterviewDto> ScheduleInterviewAsync(Guid employerId, ScheduleInterviewRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<InterviewDto>> GetMyInterviewsAsync(Guid seekerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<InterviewDto>> GetJobInterviewsAsync(Guid jobId, Guid employerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<InterviewDto> AcceptInterviewAsync(Guid id, Guid seekerId, CancellationToken cancellationToken = default);
    Task<InterviewDto> RejectInterviewAsync(Guid id, Guid seekerId, CancellationToken cancellationToken = default);
    Task<InterviewDto> RescheduleInterviewAsync(Guid id, Guid seekerId, RescheduleRequest request, CancellationToken cancellationToken = default);
    Task<InterviewDto> CompleteInterviewAsync(Guid id, Guid employerId, CancellationToken cancellationToken = default);
}

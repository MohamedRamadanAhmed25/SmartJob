using SmartJob.API.DTOs.Jobs;

namespace SmartJob.API.Services;

public interface IAIMatchService
{
    Task<AIAnalysisDto> AnalyzeAsync(Guid jobId, Guid seekerId, CancellationToken cancellationToken = default);
    Task<int> ComputeScoreAsync(Guid jobId, Guid seekerId, CancellationToken cancellationToken = default);
}

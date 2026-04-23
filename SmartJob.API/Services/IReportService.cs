using SmartJob.API.DTOs.Reports;

namespace SmartJob.API.Services;

public interface IReportService
{
    Task<ReportDto> CreateReportAsync(Guid reporterId, CreateReportRequest request, CancellationToken cancellationToken = default);
}

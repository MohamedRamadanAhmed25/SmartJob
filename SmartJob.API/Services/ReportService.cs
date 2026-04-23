using AutoMapper;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Reports;
using SmartJob.API.Exceptions;
using SmartJob.API.Models;

namespace SmartJob.API.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public ReportService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<ReportDto> CreateReportAsync(Guid reporterId, CreateReportRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ReportType>(request.ReportType, true, out var type))
            throw new ApiException(StatusCodes.Status400BadRequest, "Invalid report type.");

        var report = new Report
        {
            ReporterId = reporterId,
            ReportType = type,
            Description = request.Description.Trim(),
            Status = ReportStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _db.Reports.Add(report);
        await _db.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ReportDto>(report);
    }
}

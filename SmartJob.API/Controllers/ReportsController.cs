using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartJob.API.DTOs.Reports;
using SmartJob.API.Services;

namespace SmartJob.API.Controllers;

/// <summary>
/// Manages issue reporting and bug feedback from users.
/// </summary>
[Authorize]
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ICurrentUserService _currentUserService;

    public ReportsController(IReportService reportService, ICurrentUserService currentUserService)
    {
        _reportService = reportService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Submits a new issue report or bug feedback from the current user.
    /// </summary>
    /// <param name="request">Report details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created report details.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ReportDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReportDto>> CreateReport(CreateReportRequest request, CancellationToken cancellationToken)
    {
        var report = await _reportService.CreateReportAsync(_currentUserService.GetRequiredUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(CreateReport), new { id = report.Id }, report);
    }
}

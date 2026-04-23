using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartJob.API.DTOs.Applications;
using SmartJob.API.DTOs.Jobs;
using SmartJob.API.Services;

namespace SmartJob.API.Controllers;

/// <summary>
/// Manages applications submitted by job seekers for job postings.
/// </summary>
[Authorize]
[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly ICurrentUserService _currentUserService;

    public ApplicationsController(IApplicationService applicationService, ICurrentUserService currentUserService)
    {
        _applicationService = applicationService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Submits a new job application (Seeker only).
    /// </summary>
    /// <param name="request">Application details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "SeekerOnly")]
    [HttpPost]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApplicationDto>> Apply(ApplyRequest request, CancellationToken cancellationToken)
    {
        var app = await _applicationService.ApplyAsync(_currentUserService.GetRequiredUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetApplication), new { id = app.Id }, app);
    }

    /// <summary>
    /// Retrieves applications for the current seeker.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "SeekerOnly")]
    [HttpGet("my")]
    [ProducesResponseType(typeof(PagedResult<ApplicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ApplicationDto>>> GetMyApplications([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _applicationService.GetMyApplicationsAsync(_currentUserService.GetRequiredUserId(), page, pageSize, cancellationToken));
    }

    /// <summary>
    /// Retrieves applications for a specific job (Employer only).
    /// </summary>
    /// <param name="jobId">The unique identifier of the job.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "EmployerOnly")]
    [HttpGet("job/{jobId}")]
    [ProducesResponseType(typeof(PagedResult<ApplicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ApplicationDto>>> GetJobApplications(Guid jobId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _applicationService.GetJobApplicationsAsync(jobId, _currentUserService.GetRequiredUserId(), page, pageSize, cancellationToken));
    }

    /// <summary>
    /// Retrieves details for a specific job application.
    /// </summary>
    /// <param name="id">The unique identifier of the application.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The application details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApplicationDto>> GetApplication(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _applicationService.GetApplicationAsync(id, _currentUserService.GetRequiredUserId(), cancellationToken));
    }

    /// <summary>
    /// Updates the status of a job application (Employer only).
    /// </summary>
    /// <param name="id">The unique identifier of the application.</param>
    /// <param name="request">The new status value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated application details.</returns>
    [Authorize(Policy = "EmployerOnly")]
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApplicationDto>> PatchStatus(Guid id, PatchApplicationStatusRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _applicationService.PatchStatusAsync(id, _currentUserService.GetRequiredUserId(), request.Status, cancellationToken));
    }
}

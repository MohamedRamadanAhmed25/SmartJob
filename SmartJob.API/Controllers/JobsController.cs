using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartJob.API.DTOs.Jobs;
using SmartJob.API.Services;

namespace SmartJob.API.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ICurrentUserService _currentUserService;

    public JobsController(IJobService jobService, ICurrentUserService currentUserService)
    {
        _jobService = jobService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves a paginated list of jobs based on filter criteria.
    /// </summary>
    /// <param name="filter">Filter criteria for jobs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<JobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<JobDto>>> GetJobs([FromQuery] JobFilterRequest filter, CancellationToken cancellationToken)
    {
        return Ok(await _jobService.GetJobsAsync(filter, cancellationToken));
    }

    /// <summary>
    /// Retrieves a paginated list of recommended jobs for the current seeker.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "SeekerOnly")]
    [HttpGet("recommended")]
    [ProducesResponseType(typeof(PagedResult<JobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<JobDto>>> GetRecommendedJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _jobService.GetRecommendedJobsAsync(_currentUserService.GetRequiredUserId(), page, pageSize, cancellationToken));
    }

    /// <summary>
    /// Retrieves a specific job by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the job.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JobDto>> GetJobById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _jobService.GetJobByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Creates a new job posting (Employer only).
    /// </summary>
    /// <param name="request">Job details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "EmployerOnly")]
    [HttpPost]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<JobDto>> CreateJob(CreateJobRequest request, CancellationToken cancellationToken)
    {
        var job = await _jobService.CreateJobAsync(_currentUserService.GetRequiredUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetJobById), new { id = job.Id }, job);
    }

    /// <summary>
    /// Updates an existing job posting (Employer only).
    /// </summary>
    /// <param name="id">The unique identifier of the job.</param>
    /// <param name="request">Updated job details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "EmployerOnly")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JobDto>> UpdateJob(Guid id, UpdateJobRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _jobService.UpdateJobAsync(id, _currentUserService.GetRequiredUserId(), request, cancellationToken));
    }

    /// <summary>
    /// Partially updates the status of an existing job posting (Employer only).
    /// </summary>
    /// <param name="id">The unique identifier of the job.</param>
    /// <param name="request">The new status value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [Authorize(Policy = "EmployerOnly")]
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PatchStatus(Guid id, PatchJobStatusRequest request, CancellationToken cancellationToken)
    {
        await _jobService.PatchStatusAsync(id, _currentUserService.GetRequiredUserId(), request.Status, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes a job posting (Employer only).
    /// </summary>
    /// <param name="id">The unique identifier of the job.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [Authorize(Policy = "EmployerOnly")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteJob(Guid id, CancellationToken cancellationToken)
    {
        await _jobService.DeleteJobAsync(id, _currentUserService.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Retrieves a detailed AI analysis and match score for a job posting.
    /// </summary>
    /// <param name="id">The unique identifier of the job.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An AI analysis containing match score and breakdown.</returns>
    [Authorize]
    [HttpGet("{id}/ai-analysis")]
    [ProducesResponseType(typeof(AIAnalysisDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AIAnalysisDto>> GetAIAnalysis(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _jobService.GetAIAnalysisAsync(id, _currentUserService.GetRequiredUserId(), cancellationToken));
    }
}

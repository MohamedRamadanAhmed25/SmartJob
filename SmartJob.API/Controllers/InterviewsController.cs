using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartJob.API.DTOs.Interviews;
using SmartJob.API.DTOs.Jobs;
using SmartJob.API.Services;

namespace SmartJob.API.Controllers;

/// <summary>
/// Manages interview scheduling, acceptance, rejection, and rescheduling.
/// </summary>
[Authorize]
[ApiController]
[Route("api/interviews")]
public class InterviewsController : ControllerBase
{
    private readonly IInterviewService _interviewService;
    private readonly ICurrentUserService _currentUserService;

    public InterviewsController(IInterviewService interviewService, ICurrentUserService currentUserService)
    {
        _interviewService = interviewService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Schedules a new interview for a job application (Employer only).
    /// </summary>
    /// <param name="request">Interview schedule details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "EmployerOnly")]
    [HttpPost]
    [ProducesResponseType(typeof(InterviewDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<InterviewDto>> ScheduleInterview(ScheduleInterviewRequest request, CancellationToken cancellationToken)
    {
        var interview = await _interviewService.ScheduleInterviewAsync(_currentUserService.GetRequiredUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetMyInterviews), new { }, interview);
    }

    /// <summary>
    /// Retrieves interviews for the current seeker.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "SeekerOnly")]
    [HttpGet("my")]
    [ProducesResponseType(typeof(PagedResult<InterviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InterviewDto>>> GetMyInterviews([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _interviewService.GetMyInterviewsAsync(_currentUserService.GetRequiredUserId(), page, pageSize, cancellationToken));
    }

    /// <summary>
    /// Retrieves interviews for a specific job (Employer only).
    /// </summary>
    /// <param name="jobId">The unique identifier of the job.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "EmployerOnly")]
    [HttpGet("job/{jobId}")]
    [ProducesResponseType(typeof(PagedResult<InterviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InterviewDto>>> GetJobInterviews(Guid jobId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(await _interviewService.GetJobInterviewsAsync(jobId, _currentUserService.GetRequiredUserId(), page, pageSize, cancellationToken));
    }

    /// <summary>
    /// Accepts a scheduled interview (Seeker only).
    /// </summary>
    /// <param name="id">The unique identifier of the interview.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "SeekerOnly")]
    [HttpPatch("{id}/accept")]
    [ProducesResponseType(typeof(InterviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InterviewDto>> AcceptInterview(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _interviewService.AcceptInterviewAsync(id, _currentUserService.GetRequiredUserId(), cancellationToken));
    }

    /// <summary>
    /// Rejects a scheduled interview (Seeker only).
    /// </summary>
    /// <param name="id">The unique identifier of the interview.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "SeekerOnly")]
    [HttpPatch("{id}/reject")]
    [ProducesResponseType(typeof(InterviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InterviewDto>> RejectInterview(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _interviewService.RejectInterviewAsync(id, _currentUserService.GetRequiredUserId(), cancellationToken));
    }

    /// <summary>
    /// Requests to reschedule an interview (Seeker only).
    /// </summary>
    /// <param name="id">The unique identifier of the interview.</param>
    /// <param name="request">Reschedule request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "SeekerOnly")]
    [HttpPost("{id}/reschedule")]
    [ProducesResponseType(typeof(InterviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InterviewDto>> RescheduleInterview(Guid id, RescheduleRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _interviewService.RescheduleInterviewAsync(id, _currentUserService.GetRequiredUserId(), request, cancellationToken));
    }

    /// <summary>
    /// Marks an interview as completed (Employer only).
    /// </summary>
    /// <param name="id">The unique identifier of the interview.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize(Policy = "EmployerOnly")]
    [HttpPatch("{id}/complete")]
    [ProducesResponseType(typeof(InterviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InterviewDto>> CompleteInterview(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _interviewService.CompleteInterviewAsync(id, _currentUserService.GetRequiredUserId(), cancellationToken));
    }
}

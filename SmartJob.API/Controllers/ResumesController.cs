using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartJob.API.DTOs.Resumes;
using SmartJob.API.Services;

namespace SmartJob.API.Controllers;

/// <summary>
/// Manages resume operations for job seekers.
/// </summary>
[ApiController]
[Authorize(Policy = "SeekerOnly")]
[Route("api/resumes")]
public class ResumesController : ControllerBase
{
    private readonly IResumeService _resumeService;
    private readonly ICurrentUserService _currentUserService;

    public ResumesController(IResumeService resumeService, ICurrentUserService currentUserService)
    {
        _resumeService = resumeService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves a list of resumes for the current job seeker.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of resume metadata.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ResumeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResumeDto>>> GetMyResumes(CancellationToken cancellationToken)
    {
        return Ok(await _resumeService.GetMyResumesAsync(_currentUserService.GetRequiredUserId(), cancellationToken));
    }

    /// <summary>
    /// Uploads a new resume file.
    /// </summary>
    /// <param name="request">The resume upload request containing the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The uploaded resume metadata.</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ResumeDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResumeDto>> Upload([FromForm] ResumeUploadRequest request, CancellationToken cancellationToken)
    {
        var result = await _resumeService.UploadAsync(_currentUserService.GetRequiredUserId(), request.File, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Deletes a specific resume by its ID.
    /// </summary>
    /// <param name="id">The ID of the resume to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _resumeService.DeleteAsync(_currentUserService.GetRequiredUserId(), id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets a specific resume as the default for the job seeker.
    /// </summary>
    /// <param name="id">The ID of the resume to set as default.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated resume metadata.</returns>
    [HttpPatch("{id:guid}/default")]
    [ProducesResponseType(typeof(ResumeDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumeDto>> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _resumeService.SetDefaultAsync(_currentUserService.GetRequiredUserId(), id, cancellationToken));
    }
}

using Microsoft.AspNetCore.Mvc;
using SmartJob.API.Services;

namespace SmartJob.API.AI.Controllers;

[Route("api/ai")]
[ApiController]
public class AiController : ControllerBase
{
    private readonly IAIMatchService _aiMatchService;

    public AiController(IAIMatchService aiMatchService)
    {
        _aiMatchService = aiMatchService;
    }

    /// <summary>
    /// Matches a candidate against a job posting using Gemini AI analysis.
    /// </summary>
    /// <param name="jobId">The ID of the job to match against.</param>
    /// <param name="seekerId">The ID of the job seeker.</param>
    /// <returns>Match score, matched/missing skills, and reasoning.</returns>
    [HttpPost("match")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Match([FromQuery] Guid jobId, [FromQuery] Guid seekerId)
    {
        if (jobId == Guid.Empty || seekerId == Guid.Empty)
        {
            return BadRequest("Both jobId and seekerId are required.");
        }

        try
        {
            var result = await _aiMatchService.AnalyzeAsync(jobId, seekerId);
            return Ok(result);
        }
        catch (ApplicationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartJob.API.AI.Services;
using SmartJob.API.Data;

namespace SmartJob.API.AI.Controllers;

[Route("api/ai")]
[ApiController]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly AppDbContext _context;

    public AiController(IAiService aiService, AppDbContext context)
    {
        _aiService = aiService;
        _context = context;
    }

    /// <summary>
    /// Matches a candidate's CV against a job description using the external AI service.
    /// </summary>
    /// <param name="cv">The candidate's CV in PDF format.</param>
    /// <param name="jobId">The ID of the job to match against.</param>
    /// <returns>Match score and detailed skills analysis.</returns>
    [HttpPost("match")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Match(IFormFile cv, [FromForm] Guid jobId)
    {
        if (cv == null || cv.Length == 0)
        {
            return BadRequest("CV file is required.");
        }

        if (cv.ContentType != "application/pdf")
        {
            return BadRequest("Only PDF files are supported.");
        }

        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job == null)
        {
            return NotFound("Job not found.");
        }

        if (string.IsNullOrEmpty(job.Description))
        {
            return BadRequest("Job description is empty.");
        }

        try
        {
            var matchResult = await _aiService.GetMatchScoreAsync(cv, job.Description);
            if (matchResult == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to parse AI service response.");
            }

            return Ok(matchResult);
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

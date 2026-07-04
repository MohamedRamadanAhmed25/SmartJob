using Microsoft.EntityFrameworkCore;
using SmartJob.API.AI.DTOs;
using SmartJob.API.AI.Services;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Jobs;
using SmartJob.API.Exceptions;
using SmartJob.API.Models;

namespace SmartJob.API.Services;

public class AIMatchService : IAIMatchService
{
    private readonly AppDbContext _db;
    private readonly IAiService _aiService;
    private readonly ILogger<AIMatchService> _logger;

    public AIMatchService(AppDbContext db, IAiService aiService, ILogger<AIMatchService> logger)
    {
        _db = db;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<AIAnalysisDto> AnalyzeAsync(Guid jobId, Guid seekerId, CancellationToken cancellationToken = default)
    {
        var job = await _db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Job not found.");

        var seekerProfile = await _db.SeekerProfiles.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == seekerId, cancellationToken);

        if (seekerProfile == null)
        {
            return new AIAnalysisDto();
        }

        var seekerSkills = string.Join(", ", seekerProfile.Skills);

        try
        {
            var aiResult = await _aiService.AnalyzeMatchAsync(
                seekerSkills,
                seekerProfile.ExperienceYears,
                job.Description,
                job.Requirements);

            if (aiResult != null)
            {
                return new AIAnalysisDto
                {
                    MatchScore = (int)Math.Round(aiResult.MatchScore),
                    MatchingSkills = aiResult.MatchedSkills,
                    MissingSkills = aiResult.MissingSkills,
                    WhyMatch = aiResult.WhyMatch
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini AI service call failed for Job={JobId}, Seeker={SeekerId}. Falling back to keyword matching.", jobId, seekerId);
        }

        // Fallback: simple keyword matching if Gemini fails
        return FallbackKeywordMatch(seekerProfile, job);
    }

    public async Task<int> ComputeScoreAsync(Guid jobId, Guid seekerId, CancellationToken cancellationToken = default)
    {
        var analysis = await AnalyzeAsync(jobId, seekerId, cancellationToken);
        return analysis.MatchScore;
    }

    /// <summary>
    /// Fallback keyword matching when Gemini API is unavailable.
    /// </summary>
    private static AIAnalysisDto FallbackKeywordMatch(SeekerProfile seeker, Job job)
    {
        var seekerSkills = seeker.Skills.Select(s => s.ToLower().Trim()).ToHashSet();
        var jobReqs = job.Requirements.Select(r => r.Trim()).ToList();
        var matching = jobReqs.Where(r => seekerSkills.Contains(r.ToLower())).ToList();
        var missing = jobReqs.Where(r => !seekerSkills.Contains(r.ToLower())).ToList();

        double skillScore = jobReqs.Count > 0 ? (double)matching.Count / jobReqs.Count * 100 : 50;

        return new AIAnalysisDto
        {
            MatchScore = (int)Math.Round(skillScore),
            MatchingSkills = matching,
            MissingSkills = missing,
            WhyMatch = new List<string> { "Score based on keyword matching (AI service unavailable)." }
        };
    }
}

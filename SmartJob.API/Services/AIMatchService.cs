using Microsoft.EntityFrameworkCore;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Jobs;
using SmartJob.API.Exceptions;
using SmartJob.API.Models;

namespace SmartJob.API.Services;

public class AIMatchService : IAIMatchService
{
    private readonly AppDbContext _db;

    public AIMatchService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AIAnalysisDto> AnalyzeAsync(Guid jobId, Guid seekerId, CancellationToken cancellationToken = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Job not found.");

        var seekerProfile = await _db.SeekerProfiles
            .FirstOrDefaultAsync(s => s.Id == seekerId, cancellationToken);

        if (seekerProfile == null)
        {
            return new AIAnalysisDto
            {
                MatchScore = 0,
                MatchingSkills = new(),
                MissingSkills = job.Requirements,
                WhyMatch = new List<string> { "Complete your seeker profile to get a match score." }
            };
        }

        // --- Skill Overlap (50%) ---
        var seekerSkills = seekerProfile.Skills.Select(s => s.ToLower().Trim()).ToHashSet();
        var jobReqs = job.Requirements.Select(r => r.Trim()).ToList();
        var matching = jobReqs.Where(r => seekerSkills.Contains(r.ToLower())).ToList();
        var missing = jobReqs.Where(r => !seekerSkills.Contains(r.ToLower())).ToList();

        double skillScore = jobReqs.Count > 0 ? (double)matching.Count / jobReqs.Count * 100 : 50;

        // --- Experience Score (20%) ---
        double expScore = seekerProfile.ExperienceYears switch
        {
            0 => 20,
            1 => 35,
            2 => 50,
            3 => 65,
            4 => 75,
            >= 5 and <= 7 => 90,
            _ => 100
        };

        // --- Location Score (15%) ---
        double locationScore = 50; // default if unknown
        if (!string.IsNullOrWhiteSpace(job.Location))
        {
            if (job.Type == JobType.Remote)
                locationScore = 100;
            else
                locationScore = 60; // partial
        }

        // --- Behavior Score (15%) ---
        var applicationCount = await _db.Applications.CountAsync(a => a.SeekerId == seekerId, cancellationToken);
        double behaviorScore = Math.Min(applicationCount * 10, 100);

        // --- Weighted Total ---
        double total = (skillScore * 0.50) + (expScore * 0.20) + (locationScore * 0.15) + (behaviorScore * 0.15);
        int matchScore = (int)Math.Round(total);

        // --- Why Match reasons ---
        var whyMatch = new List<string>();
        if (matching.Count > 0) whyMatch.Add($"Matches {matching.Count} of {jobReqs.Count} required skills");
        if (seekerProfile.ExperienceYears >= 3) whyMatch.Add($"{seekerProfile.ExperienceYears}+ years of experience");
        if (seekerProfile.EducationLevel >= EducationLevel.Bachelor) whyMatch.Add("Strong educational background");
        if (job.Type == JobType.Remote) whyMatch.Add("Remote-friendly position");
        if (whyMatch.Count == 0) whyMatch.Add("Build your profile and skills for a better match");

        return new AIAnalysisDto
        {
            MatchScore = matchScore,
            MatchingSkills = matching,
            MissingSkills = missing,
            WhyMatch = whyMatch
        };
    }

    public async Task<int> ComputeScoreAsync(Guid jobId, Guid seekerId, CancellationToken cancellationToken = default)
    {
        var analysis = await AnalyzeAsync(jobId, seekerId, cancellationToken);
        return analysis.MatchScore;
    }
}

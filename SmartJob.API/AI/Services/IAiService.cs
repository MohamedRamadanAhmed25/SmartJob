using SmartJob.API.AI.DTOs;

namespace SmartJob.API.AI.Services;

public interface IAiService
{
    /// <summary>
    /// Analyzes the match between a seeker's skills/experience and a job description using Gemini AI.
    /// </summary>
    Task<AiMatchResponseDto?> AnalyzeMatchAsync(string seekerSkills, int experienceYears, string jobDescription, List<string> jobRequirements);

    /// <summary>
    /// Analyzes a raw CV file (PDF) against a job description using Gemini AI.
    /// </summary>
    Task<AiMatchResponseDto?> AnalyzeCvFileMatchAsync(IFormFile cvFile, string jobDescription, List<string> jobRequirements);
}

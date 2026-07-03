using Microsoft.AspNetCore.Http;
using SmartJob.API.AI.DTOs;

namespace SmartJob.API.AI.Services;

public interface IAiService
{
    Task<AiMatchResponseDto?> GetMatchScoreAsync(IFormFile cvFile, string jobDescription);
}

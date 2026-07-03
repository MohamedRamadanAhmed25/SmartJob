using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SmartJob.API.AI.DTOs;

namespace SmartJob.API.AI.Services;

public class AiService : IAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AiService> _logger;

    public AiService(IHttpClientFactory httpClientFactory, ILogger<AiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<AiMatchResponseDto?> GetMatchScoreAsync(IFormFile cvFile, string jobDescription)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("AiServiceClient");
            
            using var content = new MultipartFormDataContent();
            
            // Add job_description
            content.Add(new StringContent(jobDescription), "job_description");

            // Add cv
            using var stream = cvFile.OpenReadStream();
            var streamContent = new StreamContent(stream);
            var contentType = string.IsNullOrEmpty(cvFile.ContentType) ? "application/pdf" : cvFile.ContentType;
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "cv", cvFile.FileName);

            var response = await httpClient.PostAsync("match", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI Service returned {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                throw new ApplicationException($"AI service returned an error status code: {response.StatusCode}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AiMatchResponseDto>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling AI service");
            throw new ApplicationException("Error connecting to the AI service.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout calling AI service");
            throw new ApplicationException("The AI service request timed out.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling AI service");
            throw new ApplicationException("An unexpected error occurred while communicating with the AI service.", ex);
        }
    }
}

using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using SmartJob.API.AI.DTOs;

namespace SmartJob.API.AI.Services;

public class AiService : IAiService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiService> _logger;

    public AiService(IConfiguration configuration, ILogger<AiService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AiMatchResponseDto?> AnalyzeMatchAsync(
        string seekerSkills,
        int experienceYears,
        string jobDescription,
        List<string> jobRequirements)
    {
        try
        {
            var apiKey = _configuration["AiService:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API key is not configured. Set 'AiService:ApiKey' in appsettings.json.");

            var modelName = _configuration["AiService:ModelName"] ?? "gemini-2.5-flash";

            var client = new Client(apiKey: apiKey);

            var prompt = BuildPrompt(seekerSkills, experienceYears, jobDescription, jobRequirements);

            _logger.LogInformation("Sending match analysis request to Gemini model: {Model}", modelName);

            var config = new GenerateContentConfig
            {
                Temperature = 0.3,
                ResponseMimeType = "application/json"
            };

            var response = await client.Models.GenerateContentAsync(
                model: modelName,
                contents: prompt,
                config: config);

            var jsonText = response.Text;

            if (string.IsNullOrWhiteSpace(jsonText))
            {
                _logger.LogWarning("Gemini returned an empty response");
                return null;
            }

            // Gemini often wraps JSON in markdown blocks
            jsonText = jsonText.Trim();
            if (jsonText.StartsWith("```json"))
            {
                jsonText = jsonText.Substring(7);
            }
            if (jsonText.StartsWith("```"))
            {
                jsonText = jsonText.Substring(3);
            }
            if (jsonText.EndsWith("```"))
            {
                jsonText = jsonText.Substring(0, jsonText.Length - 3);
            }

            _logger.LogDebug("Gemini raw response: {Response}", jsonText);

            var result = JsonSerializer.Deserialize<AiMatchResponseDto>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini AI service");
            throw new ApplicationException("An error occurred while communicating with the Gemini AI service.", ex);
        }
    }

    private static string BuildPrompt(
        string seekerSkills,
        int experienceYears,
        string jobDescription,
        List<string> jobRequirements)
    {
        var requirements = jobRequirements.Count > 0
            ? string.Join(", ", jobRequirements)
            : "Not specified";

        return $"""
            You are an expert HR AI assistant for a job matching platform.
            Analyze the match between a job seeker and a job posting.

            ## Job Seeker Profile:
            - Skills: {seekerSkills}
            - Experience: {experienceYears} years

            ## Job Posting:
            - Description: {jobDescription}
            - Requirements: {requirements}

            ## Instructions:
            Analyze how well the seeker matches this job and respond with a JSON object containing:

            1. "match_score": A number from 0 to 100 representing the overall match percentage.
            2. "result": A brief summary (1-2 sentences in English) of the match quality.
            3. "matched_skills": An array of skills the seeker has that match the job requirements.
            4. "missing_skills": An array of required skills that the seeker is missing.
            5. "why_match": An array of 2-4 short reasons explaining why this is or isn't a good match.

            Respond ONLY with valid JSON. No markdown, no explanation outside the JSON.
            """;
    }

    public async Task<AiMatchResponseDto?> AnalyzeCvFileMatchAsync(IFormFile cvFile, string jobDescription, List<string> jobRequirements)
    {
        try
        {
            var apiKey = _configuration["AiService:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API key is not configured. Set 'AiService:ApiKey' in appsettings.json.");

            var modelName = _configuration["AiService:ModelName"] ?? "gemini-2.5-flash";
            var client = new Client(apiKey: apiKey);

            using var memoryStream = new MemoryStream();
            await cvFile.CopyToAsync(memoryStream);
            var pdfBytes = memoryStream.ToArray();

            var prompt = BuildPromptForCv(jobDescription, jobRequirements);

            _logger.LogInformation("Sending CV match analysis request to Gemini model: {Model}", modelName);

            var config = new GenerateContentConfig
            {
                Temperature = 0.3,
                ResponseMimeType = "application/json"
            };

            var contents = new List<Content>
            {
                new Content
                {
                    Role = "user",
                    Parts = new List<Part>
                    {
                        Part.FromBytes(pdfBytes, "application/pdf"),
                        Part.FromText(prompt)
                    }
                }
            };

            var response = await client.Models.GenerateContentAsync(
                model: modelName,
                contents: contents,
                config: config);

            var jsonText = response.Text;

            if (string.IsNullOrWhiteSpace(jsonText))
            {
                _logger.LogWarning("Gemini returned an empty response for CV analysis");
                return null;
            }

            // Gemini often wraps JSON in markdown blocks
            jsonText = jsonText.Trim();
            if (jsonText.StartsWith("```json"))
            {
                jsonText = jsonText.Substring(7);
            }
            if (jsonText.StartsWith("```"))
            {
                jsonText = jsonText.Substring(3);
            }
            if (jsonText.EndsWith("```"))
            {
                jsonText = jsonText.Substring(0, jsonText.Length - 3);
            }

            _logger.LogDebug("Gemini raw response: {Response}", jsonText);

            var result = JsonSerializer.Deserialize<AiMatchResponseDto>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini AI service for CV file");
            throw new ApplicationException("An error occurred while communicating with the Gemini AI service.", ex);
        }
    }

    private static string BuildPromptForCv(string jobDescription, List<string> jobRequirements)
    {
        var requirements = jobRequirements.Count > 0
            ? string.Join(", ", jobRequirements)
            : "Not specified";

        return $"""
            You are an expert HR AI assistant for a job matching platform.
            I have attached a candidate's CV as a PDF file.
            Analyze the match between this CV and the following job posting.

            ## Job Posting:
            - Description: {jobDescription}
            - Requirements: {requirements}

            ## Instructions:
            Read the attached CV document carefully. Analyze how well the candidate matches this job and respond with a JSON object containing:

            1. "match_score": A number from 0 to 100 representing the overall match percentage.
            2. "result": A brief summary (1-2 sentences in English) of the match quality based on the CV.
            3. "matched_skills": An array of skills from the CV that match the job requirements.
            4. "missing_skills": An array of required skills that the candidate is missing based on the CV.
            5. "why_match": An array of 2-4 short reasons explaining why this is or isn't a good match based on the CV.

            Respond ONLY with valid JSON. No markdown, no explanation outside the JSON.
            """;
    }
}

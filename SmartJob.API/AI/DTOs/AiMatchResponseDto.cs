using System.Text.Json.Serialization;

namespace SmartJob.API.AI.DTOs;

public class AiMatchResponseDto
{
    [JsonPropertyName("match_score")]
    public double MatchScore { get; set; }

    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;

    [JsonPropertyName("matched_skills")]
    public List<string> MatchedSkills { get; set; } = new();

    [JsonPropertyName("missing_skills")]
    public List<string> MissingSkills { get; set; } = new();

    [JsonPropertyName("why_match")]
    public List<string> WhyMatch { get; set; } = new();
}

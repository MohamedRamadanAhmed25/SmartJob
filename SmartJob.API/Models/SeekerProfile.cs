using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartJob.API.Models;

public class SeekerProfile
{
    [Key]
    public Guid Id { get; set; }  // Same as User.Id (1-to-1)

    // Skills stored as JSON string in SQL Server
    public string SkillsJson { get; set; } = "[]";

    [NotMapped]
    public List<string> Skills
    {
        get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(SkillsJson) ?? new();
        set => SkillsJson = System.Text.Json.JsonSerializer.Serialize(value);
    }

    public int ExperienceYears { get; set; } = 0;

    public EducationLevel EducationLevel { get; set; } = EducationLevel.Bachelor;

    [MaxLength(255)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(255)]
    public string? GitHubUrl { get; set; }

    // Navigation
    [ForeignKey("Id")]
    public User User { get; set; } = null!;
}

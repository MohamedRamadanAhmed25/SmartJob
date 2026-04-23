using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartJob.API.Models;

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EmployerId { get; set; }

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    // Requirements stored as JSON string
    public string RequirementsJson { get; set; } = "[]";

    [NotMapped]
    public List<string> Requirements
    {
        get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(RequirementsJson) ?? new();
        set => RequirementsJson = System.Text.Json.JsonSerializer.Serialize(value);
    }

    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Salary { get; set; } = string.Empty;

    public JobType Type { get; set; } = JobType.FullTime;

    public JobStatus Status { get; set; } = JobStatus.Active;

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    // Navigation
    public User Employer { get; set; } = null!;
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<Chat> Chats { get; set; } = new List<Chat>();
}

namespace SmartJob.API.Models;

public class Application
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid JobId { get; set; }
    public Guid SeekerId { get; set; }
    public Guid ResumeId { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Sent;

    public int AIMatchScore { get; set; } = 0;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Job Job { get; set; } = null!;
    public User Seeker { get; set; } = null!;
    public Resume Resume { get; set; } = null!;
    public Interview? Interview { get; set; }
}

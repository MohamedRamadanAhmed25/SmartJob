namespace SmartJob.API.Models;

public class Interview
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ApplicationId { get; set; }

    public DateTime ScheduledAt { get; set; }

    public InterviewMode Mode { get; set; } = InterviewMode.Online;

    public string? InterviewLink { get; set; }

    public InterviewStatus Status { get; set; } = InterviewStatus.Pending;

    public DateTime? RescheduleRequestedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Application Application { get; set; } = null!;
}

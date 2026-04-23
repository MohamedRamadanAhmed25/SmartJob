namespace SmartJob.API.DTOs.Interviews;

public class InterviewDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string SeekerName { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Mode { get; set; } = string.Empty;
    public string? InterviewLink { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? RescheduleRequestedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ScheduleInterviewRequest
{
    public Guid ApplicationId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Mode { get; set; } = "Online";
    public string? InterviewLink { get; set; }
}

public class RescheduleRequest
{
    public DateTime ProposedAt { get; set; }
}

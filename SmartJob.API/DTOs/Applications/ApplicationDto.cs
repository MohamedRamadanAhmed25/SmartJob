namespace SmartJob.API.DTOs.Applications;

public class ApplicationDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public Guid SeekerId { get; set; }
    public string SeekerName { get; set; } = string.Empty;
    public string? SeekerAvatarUrl { get; set; }
    public Guid ResumeId { get; set; }
    public string ResumeFileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int AIMatchScore { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ApplyRequest
{
    public Guid JobId { get; set; }
    public Guid ResumeId { get; set; }
}

public class PatchApplicationStatusRequest
{
    public string Status { get; set; } = "Viewed";
}

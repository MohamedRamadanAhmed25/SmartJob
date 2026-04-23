namespace SmartJob.API.DTOs.Jobs;

public class JobDto
{
    public Guid Id { get; set; }
    public Guid EmployerId { get; set; }
    public string EmployerName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? CompanyLogoUrl { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Requirements { get; set; } = new();
    public string Location { get; set; } = string.Empty;
    public string Salary { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PostedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ApplicationCount { get; set; }
}

public class CreateJobRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Requirements { get; set; } = new();
    public string Location { get; set; } = string.Empty;
    public string Salary { get; set; } = string.Empty;
    public string Type { get; set; } = "FullTime";
    public DateTime? ExpiresAt { get; set; }
}

public class UpdateJobRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Requirements { get; set; }
    public string? Location { get; set; }
    public string? Salary { get; set; }
    public string? Type { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class PatchJobStatusRequest
{
    public string Status { get; set; } = "Active";
}

public class JobFilterRequest
{
    public string? Keyword { get; set; }
    public string? Location { get; set; }
    public string? Type { get; set; }
    public string? Salary { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AIAnalysisDto
{
    public int MatchScore { get; set; }
    public List<string> MatchingSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public List<string> WhyMatch { get; set; } = new();
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

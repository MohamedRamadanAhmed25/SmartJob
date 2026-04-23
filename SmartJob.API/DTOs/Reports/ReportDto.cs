namespace SmartJob.API.DTOs.Reports;

public class CreateReportRequest
{
    public string ReportType { get; set; } = "Other";
    public string Description { get; set; } = string.Empty;
}

public class ReportDto
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

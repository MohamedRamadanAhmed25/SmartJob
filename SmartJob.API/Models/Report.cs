using System.ComponentModel.DataAnnotations;

namespace SmartJob.API.Models;

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReporterId { get; set; }

    public ReportType ReportType { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public ReportStatus Status { get; set; } = ReportStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Reporter { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;

namespace SmartJob.API.Models;

public class Resume
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SeekerId { get; set; }

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FileUrl { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Seeker { get; set; } = null!;
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}

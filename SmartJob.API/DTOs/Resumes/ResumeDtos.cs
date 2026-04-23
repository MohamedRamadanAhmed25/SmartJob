using System.ComponentModel.DataAnnotations;

namespace SmartJob.API.DTOs.Resumes;

public class ResumeDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class ResumeUploadRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
}

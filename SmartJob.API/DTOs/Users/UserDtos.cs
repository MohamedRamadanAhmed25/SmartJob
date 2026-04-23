using System.ComponentModel.DataAnnotations;
using SmartJob.API.Models;

namespace SmartJob.API.DTOs.Users;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsVerified { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public SeekerProfileDto? SeekerProfile { get; set; }
    public EmployerProfileDto? EmployerProfile { get; set; }
}

public class UpdateUserRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }
}

public class SeekerProfileDto
{
    public List<string> Skills { get; set; } = new();
    public int ExperienceYears { get; set; }
    public EducationLevel EducationLevel { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
}

public class UpdateSeekerProfileRequest
{
    public List<string> Skills { get; set; } = new();
    [Range(0, 60)]
    public int ExperienceYears { get; set; }
    public EducationLevel EducationLevel { get; set; }
    [MaxLength(255)]
    public string? LinkedInUrl { get; set; }
    [MaxLength(255)]
    public string? GitHubUrl { get; set; }
}

public class EmployerProfileDto
{
    public string CompanyName { get; set; } = string.Empty;
    public CompanySize CompanySize { get; set; }
    public string? Industry { get; set; }
    public string? CompanyLogoUrl { get; set; }
    public string? Website { get; set; }
}

public class UpdateEmployerProfileRequest
{
    [Required, MaxLength(150)]
    public string CompanyName { get; set; } = string.Empty;
    public CompanySize CompanySize { get; set; }
    [MaxLength(100)]
    public string? Industry { get; set; }
    [MaxLength(255)]
    public string? Website { get; set; }
}

public class AvatarUploadRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
}

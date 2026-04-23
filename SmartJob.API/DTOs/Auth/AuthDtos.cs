using System.ComponentModel.DataAnnotations;
using SmartJob.API.Models;

namespace SmartJob.API.DTOs.Auth;

public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public int? ExperienceYears { get; set; }
    public EducationLevel? EducationLevel { get; set; }
    public List<string>? Skills { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }

    public string? CompanyName { get; set; }
    public CompanySize? CompanySize { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;
}

public class AuthUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsVerified { get; set; }
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public AuthUserDto User { get; set; } = new();
}

public class MessageResponse
{
    public string Message { get; set; } = string.Empty;
}

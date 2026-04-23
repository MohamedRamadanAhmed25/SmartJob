using System.ComponentModel.DataAnnotations;

namespace SmartJob.API.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public bool IsVerified { get; set; } = false;

    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public SeekerProfile? SeekerProfile { get; set; }
    public EmployerProfile? EmployerProfile { get; set; }
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

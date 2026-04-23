using Microsoft.EntityFrameworkCore;
using SmartJob.API.Models;

namespace SmartJob.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<SeekerProfile> SeekerProfiles => Set<SeekerProfile>();
    public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- User ---
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.EmailVerificationToken).HasMaxLength(128);
            e.Property(u => u.RefreshToken).HasMaxLength(128);
            e.Property(u => u.PasswordResetToken).HasMaxLength(128);
            e.HasOne(u => u.SeekerProfile)
                .WithOne(s => s.User)
                .HasForeignKey<SeekerProfile>(s => s.Id)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(u => u.EmployerProfile)
                .WithOne(ep => ep.User)
                .HasForeignKey<EmployerProfile>(ep => ep.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Resume ---
        modelBuilder.Entity<Resume>(e =>
        {
            e.HasOne(r => r.Seeker)
                .WithMany(u => u.Resumes)
                .HasForeignKey(r => r.SeekerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Job ---
        modelBuilder.Entity<Job>(e =>
        {
            e.HasOne(j => j.Employer)
                .WithMany(u => u.Jobs)
                .HasForeignKey(j => j.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Application ---
        modelBuilder.Entity<Application>(e =>
        {
            e.HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Seeker)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.SeekerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Resume)
                .WithMany(r => r.Applications)
                .HasForeignKey(a => a.ResumeId)
                .OnDelete(DeleteBehavior.Restrict);
            // Unique: one application per seeker per job
            e.HasIndex(a => new { a.JobId, a.SeekerId }).IsUnique();
        });

        // --- Interview ---
        modelBuilder.Entity<Interview>(e =>
        {
            e.HasOne(i => i.Application)
                .WithOne(a => a.Interview)
                .HasForeignKey<Interview>(i => i.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Chat ---
        modelBuilder.Entity<Chat>(e =>
        {
            e.HasOne(c => c.Job)
                .WithMany(j => j.Chats)
                .HasForeignKey(c => c.JobId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Seeker)
                .WithMany()
                .HasForeignKey(c => c.SeekerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Employer)
                .WithMany()
                .HasForeignKey(c => c.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);
            // Unique: one chat per seeker+employer per job
            e.HasIndex(c => new { c.JobId, c.SeekerId, c.EmployerId }).IsUnique();
        });

        // --- Message ---
        modelBuilder.Entity<Message>(e =>
        {
            e.HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Notification ---
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Report ---
        modelBuilder.Entity<Report>(e =>
        {
            e.HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

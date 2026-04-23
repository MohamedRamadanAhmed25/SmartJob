using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartJob.API.Data;
using SmartJob.API.Models;
using SmartJob.API.Services;
using Xunit;

namespace SmartJob.Tests.Services;

public class AIMatchServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly AIMatchService _aiMatchService;

    public AIMatchServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _aiMatchService = new AIMatchService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task AnalyzeAsync_CalculatesCorrectScore_WhenSkillsMatch()
    {
        // Arrange
        var seekerId = Guid.NewGuid();
        var jobId = Guid.NewGuid();

        _dbContext.Users.Add(new User 
        { 
            Id = seekerId, 
            Email = "seeker@test.com", 
            Name = "John", 
            Role = UserRole.Seeker,
            SeekerProfile = new SeekerProfile
            {
                Id = seekerId,
                Skills = new List<string> { "C#", "SQL", "Azure" },
                ExperienceYears = 3, // ExperienceScore = 65
                EducationLevel = EducationLevel.Bachelor
            }
        });

        _dbContext.Jobs.Add(new Job
        {
            Id = jobId,
            EmployerId = Guid.NewGuid(),
            Title = "Backend Dev",
            Description = "Test",
            Location = "Remote", // LocationScore = 100
            Type = JobType.Remote,
            Requirements = new List<string> { "C#", "SQL", "React" } // 2/3 overlap = 66.6%
        });

        await _dbContext.SaveChangesAsync();

        // Skill Score: (2 / 3) * 100 = 66.6
        // Experience Score: 65
        // Location Score: 100 (Remote)
        // Behavior Score: 0 (No applications)
        // Total = (66.6 * 0.50) + (65 * 0.20) + (100 * 0.15) + (0 * 0.15)
        // Total = 33.3 + 13 + 15 + 0 = 61.3 -> Round to 61

        // Act
        var result = await _aiMatchService.AnalyzeAsync(jobId, seekerId);

        // Assert
        result.Should().NotBeNull();
        result.MatchScore.Should().BeInRange(60, 62);
        result.MatchingSkills.Should().Contain(new[] { "C#", "SQL" });
        result.MissingSkills.Should().Contain("React");
        result.WhyMatch.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AnalyzeAsync_CalculatesCorrectScore_WhenSeekerProfileIsMissing()
    {
        // Arrange
        var seekerId = Guid.NewGuid();
        var jobId = Guid.NewGuid();

        _dbContext.Jobs.Add(new Job
        {
            Id = jobId,
            EmployerId = Guid.NewGuid(),
            Title = "Backend Dev",
            Description = "Test",
            Requirements = new List<string> { "C#" }
        });

        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _aiMatchService.AnalyzeAsync(jobId, seekerId);

        // Assert
        result.MatchScore.Should().Be(0);
        result.MatchingSkills.Should().BeEmpty();
        result.MissingSkills.Should().Contain("C#");
    }
}

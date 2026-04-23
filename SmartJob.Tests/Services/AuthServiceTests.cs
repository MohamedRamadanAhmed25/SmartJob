using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Auth;
using SmartJob.API.Exceptions;
using SmartJob.API.Mappings;
using SmartJob.API.Models;
using SmartJob.API.Options;
using SmartJob.API.Services;
using Xunit;

namespace SmartJob.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new AuthMappingProfile());
        });
        _mapper = mapperConfig.CreateMapper();

        _jwtOptions = Options.Create(new JwtOptions
        {
            Key = "a-very-secret-and-long-key-for-testing-purposes",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        });

        _authService = new AuthService(_dbContext, _mapper, _jwtOptions);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_ValidSeeker_ReturnsTokenResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "seeker@test.com",
            Password = "Password123!",
            Role = UserRole.Seeker,
            Name = "John Doe",
            Skills = new List<string> { "C#", "ASP.NET" },
            ExperienceYears = 3
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be("seeker@test.com");
        result.User.Role.Should().Be(UserRole.Seeker);

        var dbUser = await _dbContext.Users.Include(u => u.SeekerProfile).SingleOrDefaultAsync(u => u.Email == "seeker@test.com");
        dbUser.Should().NotBeNull();
        dbUser!.SeekerProfile.Should().NotBeNull();
        dbUser.SeekerProfile!.Skills.Should().Contain("C#");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsApiException()
    {
        // Arrange
        await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "duplicate@test.com",
            Password = "Password123!",
            Role = UserRole.Seeker,
            Name = "John Doe"
        });

        var request = new RegisterRequest
        {
            Email = "duplicate@test.com",
            Password = "NewPassword123!",
            Role = UserRole.Employer,
            Name = "Jane Doe",
            CompanyName = "Test Corp"
        };

        // Act
        var act = async () => await _authService.RegisterAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenResponse()
    {
        // Arrange
        var password = "StrongPassword123!";
        await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "login@test.com",
            Password = password,
            Role = UserRole.Seeker,
            Name = "John Doe"
        });

        var request = new LoginRequest
        {
            Email = "login@test.com",
            Password = password
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsApiException()
    {
        // Arrange
        await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "invalid@test.com",
            Password = "StrongPassword123!",
            Role = UserRole.Seeker,
            Name = "John Doe"
        });

        var request = new LoginRequest
        {
            Email = "invalid@test.com",
            Password = "WrongPassword!"
        };

        // Act
        var act = async () => await _authService.LoginAsync(request);

        // Assert
        var exception = await act.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(401);
    }
}

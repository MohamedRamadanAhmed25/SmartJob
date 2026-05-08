using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Auth;
using SmartJob.API.DTOs.Jobs;
using SmartJob.API.Models;
using Xunit;

namespace SmartJob.Tests.Integration;

public class IntegrationFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private readonly System.Text.Json.JsonSerializerOptions _jsonOptions;

    public IntegrationFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"UseInMemoryDatabase", "true"},
                    {"FileStorage:RootPath", "wwwroot/test_uploads"}
                }!);
            });
        });

        _jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    [Fact]
    public async Task Employer_CanRegister_And_PostJob()
    {
        // Arrange
        var client = _factory.CreateClient();

        // 1. Register Employer
        var registerRequest = new RegisterRequest
        {
            Email = "employer_integration@test.com",
            Password = "Password123!",
            Role = UserRole.Employer,
            Name = "Tech Corp",
            CompanyName = "Tech Corp"
        };

        // Act - Register
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert - Register
        registerResponse.IsSuccessStatusCode.Should().BeTrue();
        var tokenResponse = await registerResponse.Content.ReadFromJsonAsync<TokenResponse>(_jsonOptions);
        tokenResponse.Should().NotBeNull();
        tokenResponse!.AccessToken.Should().NotBeNullOrEmpty();

        // 2. Post a Job (Uses Authorization Header)
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var createJobRequest = new CreateJobRequest
        {
            Title = "Senior Dev",
            Description = "We need a senior dev.",
            Requirements = new List<string> { "C#", ".NET" },
            Location = "Remote",
            Salary = "120k",
            Type = "FullTime"
        };

        // Act - Post Job
        var postJobResponse = await client.PostAsJsonAsync("/api/jobs", createJobRequest);

        // Assert - Post Job
        if (!postJobResponse.IsSuccessStatusCode)
        {
            var body = await postJobResponse.Content.ReadAsStringAsync();
            throw new Exception($"Post Job failed with {postJobResponse.StatusCode}. Body: {body}");
        }
        
        var createdJob = await postJobResponse.Content.ReadFromJsonAsync<JobDto>(_jsonOptions);
        createdJob.Should().NotBeNull();
        createdJob!.Title.Should().Be("Senior Dev");
        createdJob.EmployerName.Should().Be("Tech Corp");
    }
}

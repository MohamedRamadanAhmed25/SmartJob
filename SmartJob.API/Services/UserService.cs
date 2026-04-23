using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Users;
using SmartJob.API.Exceptions;
using SmartJob.API.Models;

namespace SmartJob.API.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILocalFileStorageService _fileStorageService;

    public UserService(AppDbContext dbContext, IMapper mapper, ILocalFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    public async Task<UserProfileDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await QueryUsers().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "User not found.");

        return MapUser(user);
    }

    public async Task<UserProfileDto> UpdateCurrentUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "User not found.");

        user.Name = request.Name.Trim();
        user.Phone = request.Phone?.Trim();
        user.Bio = request.Bio?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetCurrentUserAsync(userId, cancellationToken);
    }

    public async Task<UserProfileDto> UpdateAvatarAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "User not found.");

        _fileStorageService.DeleteRelativeFile(user.AvatarUrl);
        user.AvatarUrl = await _fileStorageService.SaveAvatarAsync(file, cancellationToken);
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetCurrentUserAsync(userId, cancellationToken);
    }

    public async Task<UserProfileDto> UpdateSeekerProfileAsync(Guid userId, UpdateSeekerProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await QueryUsers().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "User not found.");

        if (user.Role != UserRole.Seeker)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Only seekers can update seeker profiles.");
        }

        user.SeekerProfile ??= new SeekerProfile { Id = user.Id };
        user.SeekerProfile.Skills = request.Skills
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(skill => skill.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        user.SeekerProfile.ExperienceYears = request.ExperienceYears;
        user.SeekerProfile.EducationLevel = request.EducationLevel;
        user.SeekerProfile.LinkedInUrl = request.LinkedInUrl?.Trim();
        user.SeekerProfile.GitHubUrl = request.GitHubUrl?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapUser(user);
    }

    public async Task<UserProfileDto> UpdateEmployerProfileAsync(Guid userId, UpdateEmployerProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await QueryUsers().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "User not found.");

        if (user.Role != UserRole.Employer)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Only employers can update employer profiles.");
        }

        user.EmployerProfile ??= new EmployerProfile { Id = user.Id };
        user.EmployerProfile.CompanyName = request.CompanyName.Trim();
        user.EmployerProfile.CompanySize = request.CompanySize;
        user.EmployerProfile.Industry = request.Industry?.Trim();
        user.EmployerProfile.Website = request.Website?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapUser(user);
    }

    private IQueryable<User> QueryUsers()
    {
        return _dbContext.Users
            .Include(u => u.SeekerProfile)
            .Include(u => u.EmployerProfile);
    }

    private UserProfileDto MapUser(User user)
    {
        var dto = _mapper.Map<UserProfileDto>(user);
        dto.SeekerProfile = user.SeekerProfile is null ? null : _mapper.Map<SeekerProfileDto>(user.SeekerProfile);
        dto.EmployerProfile = user.EmployerProfile is null ? null : _mapper.Map<EmployerProfileDto>(user.EmployerProfile);
        return dto;
    }
}

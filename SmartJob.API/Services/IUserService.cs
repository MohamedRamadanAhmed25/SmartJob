using SmartJob.API.DTOs.Users;

namespace SmartJob.API.Services;

public interface IUserService
{
    Task<UserProfileDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileDto> UpdateCurrentUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileDto> UpdateAvatarAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default);
    Task<UserProfileDto> UpdateSeekerProfileAsync(Guid userId, UpdateSeekerProfileRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileDto> UpdateEmployerProfileAsync(Guid userId, UpdateEmployerProfileRequest request, CancellationToken cancellationToken = default);
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartJob.API.DTOs.Users;
using SmartJob.API.Services;

namespace SmartJob.API.Controllers;

/// <summary>
/// Manages user information and profile updates.
/// </summary>
[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves the current authenticated user's details.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current user's profile.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> GetMe(CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetCurrentUserAsync(_currentUserService.GetRequiredUserId(), cancellationToken));
    }

    /// <summary>
    /// Updates the current authenticated user's profile information.
    /// </summary>
    /// <param name="request">The update request data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user profile.</returns>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> UpdateMe(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userService.UpdateCurrentUserAsync(_currentUserService.GetRequiredUserId(), request, cancellationToken));
    }

    /// <summary>
    /// Updates the current authenticated user's avatar image.
    /// </summary>
    /// <param name="request">The avatar upload request (multipart/form-data).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user profile with the new avatar URL.</returns>
    [HttpPut("me/avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> UpdateAvatar([FromForm] AvatarUploadRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userService.UpdateAvatarAsync(_currentUserService.GetRequiredUserId(), request.File, cancellationToken));
    }

    /// <summary>
    /// Updates the current user's seeker-specific profile (Seekers only).
    /// </summary>
    /// <param name="request">The seeker profile update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user profile.</returns>
    [Authorize(Policy = "SeekerOnly")]
    [HttpPut("me/seeker-profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> UpdateSeekerProfile(UpdateSeekerProfileRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userService.UpdateSeekerProfileAsync(_currentUserService.GetRequiredUserId(), request, cancellationToken));
    }

    /// <summary>
    /// Updates the current user's employer-specific company profile (Employers only).
    /// </summary>
    /// <param name="request">The employer profile update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user profile.</returns>
    [Authorize(Policy = "EmployerOnly")]
    [HttpPut("me/employer-profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> UpdateEmployerProfile(UpdateEmployerProfileRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _userService.UpdateEmployerProfileAsync(_currentUserService.GetRequiredUserId(), request, cancellationToken));
    }
}

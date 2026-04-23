using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartJob.API.DTOs.Auth;
using SmartJob.API.Services;

namespace SmartJob.API.Controllers;

/// <summary>
/// Handles user authentication and registration.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">Registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT token and user profile.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _authService.RegisterAsync(request, cancellationToken));
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Email and password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT token and user profile.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _authService.LoginAsync(request, cancellationToken));
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <param name="request">The refresh token request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new set of JWT and refresh tokens.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenResponse>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _authService.RefreshAsync(request, cancellationToken));
    }

    /// <summary>
    /// Logs out the current user by invalidating their refresh token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(_currentUserService.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Initiates the password reset process by sending an email (mocked).
    /// </summary>
    /// <param name="request">The forgot password request containing the user email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A message response.</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageResponse>> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _authService.ForgotPasswordAsync(request, cancellationToken));
    }

    /// <summary>
    /// Resets the user password using a valid reset token.
    /// </summary>
    /// <param name="request">The reset password request containing the token and new password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A message response.</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageResponse>> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _authService.ResetPasswordAsync(request, cancellationToken));
    }

    /// <summary>
    /// Verifies a user's email using a verification token.
    /// </summary>
    /// <param name="token">The email verification token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A message response.</returns>
    [HttpGet("verify-email/{token}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageResponse>> VerifyEmail(string token, CancellationToken cancellationToken)
    {
        return Ok(await _authService.VerifyEmailAsync(token, cancellationToken));
    }
}

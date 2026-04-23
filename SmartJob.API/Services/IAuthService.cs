using SmartJob.API.DTOs.Auth;

namespace SmartJob.API.Services;

public interface IAuthService
{
    Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<TokenResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<MessageResponse> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);
}

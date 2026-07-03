using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Auth;
using SmartJob.API.Exceptions;
using SmartJob.API.Models;
using SmartJob.API.Options;

namespace SmartJob.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly JwtOptions _jwtOptions;

    public AuthService(AppDbContext dbContext, IMapper mapper, IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var userExists = await _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (userExists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "An account with this email already exists.");
        }

        ValidateRoleSpecificRegistration(request);

        // Generate ID manually to fix the Profile-Link issue
        var newUserId = Guid.NewGuid();

        var user = new User
        {
            Id = newUserId,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            Name = request.Name.Trim(),
            Phone = request.Phone?.Trim(),
            EmailVerificationToken = GenerateToken(),
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(2)
        };

        if (request.Role == UserRole.Seeker)
        {
            user.SeekerProfile = new SeekerProfile
            {
                Id = newUserId, // Matches User.Id
                Skills = request.Skills?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new(),
                ExperienceYears = request.ExperienceYears ?? 0,
                EducationLevel = request.EducationLevel ?? EducationLevel.Bachelor,
                LinkedInUrl = request.LinkedInUrl?.Trim(),
                GitHubUrl = request.GitHubUrl?.Trim()
            };
        }
        else
        {
            user.EmployerProfile = new EmployerProfile
            {
                Id = newUserId, // Matches User.Id
                CompanyName = request.CompanyName!.Trim(),
                CompanySize = request.CompanySize ?? CompanySize.Small,
                Industry = request.Industry?.Trim(),
                Website = request.Website?.Trim()
            };
        }

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GenerateTokenResponseAsync(user, cancellationToken);
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status401Unauthorized, "Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Invalid email or password.");
        }

        return await GenerateTokenResponseAsync(user, cancellationToken);
    }

    public async Task<TokenResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            u => u.RefreshToken == request.RefreshToken,
            cancellationToken) ?? throw new ApiException(StatusCodes.Status401Unauthorized, "Invalid refresh token.");

        if (user.RefreshTokenExpiry is null || user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Refresh token expired.");
        }

        return await GenerateTokenResponseAsync(user, cancellationToken);
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "User not found.");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is not null)
        {
            user.PasswordResetToken = GenerateToken();
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            user.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new MessageResponse
        {
            Message = "If the email exists, a password reset token has been generated."
        };
    }

    public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            u => u.PasswordResetToken == request.Token,
            cancellationToken) ?? throw new ApiException(StatusCodes.Status400BadRequest, "Invalid reset token.");

        if (user.PasswordResetTokenExpiry is null || user.PasswordResetTokenExpiry <= DateTime.UtcNow)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Reset token expired.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MessageResponse
        {
            Message = "Password has been reset successfully."
        };
    }

    public async Task<MessageResponse> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            u => u.EmailVerificationToken == token,
            cancellationToken) ?? throw new ApiException(StatusCodes.Status400BadRequest, "Invalid verification token.");

        if (user.EmailVerificationTokenExpiry is null || user.EmailVerificationTokenExpiry <= DateTime.UtcNow)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Verification token expired.");
        }

        user.IsVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MessageResponse
        {
            Message = "Email verified successfully."
        };
    }

    private async Task<TokenResponse> GenerateTokenResponseAsync(User user, CancellationToken cancellationToken)
    {
        user.RefreshToken = GenerateToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TokenResponse
        {
            AccessToken = GenerateJwt(user),
            RefreshToken = user.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes),
            User = _mapper.Map<AuthUserDto>(user)
        };
    }

    private string GenerateJwt(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)),
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private static string GenerateToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }

    private static void ValidateRoleSpecificRegistration(RegisterRequest request)
    {
        if (request.Role == UserRole.Seeker)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(request.CompanyName))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Company name is required for employer registration.");
        }
    }
}

using System.Security.Claims;
using SmartJob.API.Exceptions;

namespace SmartJob.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetRequiredUserId()
    {
        var rawValue = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(rawValue, out var userId))
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Authenticated user id is missing.");
        }

        return userId;
    }

    public string GetRequiredUserRole()
    {
        var role = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Authenticated user role is missing.");
        }

        return role;
    }
}

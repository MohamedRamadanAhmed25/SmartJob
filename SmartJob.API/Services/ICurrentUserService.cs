namespace SmartJob.API.Services;

public interface ICurrentUserService
{
    Guid GetRequiredUserId();
    string GetRequiredUserRole();
}

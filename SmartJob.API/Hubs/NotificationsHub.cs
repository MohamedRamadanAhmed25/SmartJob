using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartJob.API.Hubs;

[Authorize]
public class NotificationsHub : Hub
{
}

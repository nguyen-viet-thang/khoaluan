using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PopPop.Hubs;

/// <summary>
/// Push-only hub: the server sends "NotificationReceived" to a signed-in user via Clients.User(userId)
/// (the user id comes from the NameIdentifier claim). Clients never call server methods here.
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub
{
    public const string ReceivedEvent = "NotificationReceived";
}

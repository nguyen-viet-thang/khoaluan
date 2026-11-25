using PopPop.Services.Implementations;

namespace PopPop.Middleware;

/// <summary>
/// Sends a signed-in user whose password was set by an administrator to the change-password page
/// until they pick their own. Static files, SignalR hubs, logout and the page itself stay reachable.
/// </summary>
public sealed class ForcePasswordChangeMiddleware
{
    public const string ChangePasswordPath = "/Identity/Account/Manage/ChangePassword";

    private readonly RequestDelegate _next;

    public ForcePasswordChangeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (context.User.HasClaim(claim => claim.Type == PopPopClaimsPrincipalFactory.MustChangePasswordClaim)
            && !path.StartsWithSegments(ChangePasswordPath, StringComparison.OrdinalIgnoreCase)
            && !path.StartsWithSegments("/Identity/Account/Logout", StringComparison.OrdinalIgnoreCase)
            && !path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase)
            && !Path.HasExtension(path.Value))
        {
            context.Response.Redirect(ChangePasswordPath);
            return Task.CompletedTask;
        }

        return _next(context);
    }
}

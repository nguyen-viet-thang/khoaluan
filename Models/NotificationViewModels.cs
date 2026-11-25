namespace PopPop.Models;

public sealed class NotificationItemViewModel
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public string? Message { get; init; }
    public string? Url { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class NotificationListViewModel
{
    public IReadOnlyList<NotificationItemViewModel> Items { get; init; } = [];
    public int UnreadCount { get; init; }
    public int PageNumber { get; init; }
    public int TotalPages { get; init; }
}

/// <summary>Counts and latest notifications shown in the storefront header.</summary>
public sealed class HeaderBadges
{
    public int CartQuantity { get; init; }
    public int UnreadNotificationCount { get; init; }
    public IReadOnlyList<NotificationItemViewModel> RecentNotifications { get; init; } = [];
}

/// <summary>Bell data for the Admin/Staff layouts.</summary>
public sealed class StaffBadges
{
    public int UnreadNotificationCount { get; init; }
    public IReadOnlyList<NotificationItemViewModel> RecentNotifications { get; init; } = [];
    /// <summary>Claims waiting for a staff decision (status Submitted).</summary>
    public int PendingClaimCount { get; init; }
    /// <summary>Orders waiting for confirmation (status Pending).</summary>
    public int PendingOrderCount { get; init; }
}

/// <summary>Payload pushed to the browser over SignalR.</summary>
public sealed record NotificationPush(int Id, string Title, string? Message, string OpenUrl, string CreatedAt);

namespace PopPop.Data.Entities;

/// <summary>
/// In-site notification for a customer (e.g. a warranty claim changed status). Shown under the header bell.
/// </summary>
public class Notification : IBaseEntity<int>
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string Title { get; set; }
    public string? Message { get; set; }

    /// <summary>Site-relative link opened when the customer clicks the notification.</summary>
    public string? Url { get; set; }

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public virtual User? User { get; set; }
}

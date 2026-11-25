using PopPop.Data.Entities;

namespace PopPop.Services.Interfaces;

/// <summary>
/// Locks/unlocks any account (customer or staff). A locked account cannot sign in,
/// and its existing sessions end at the next security-stamp check (about one minute).
/// </summary>
public interface IAccountLockService
{
    Task LockAsync(User user, string reason);
    Task UnlockAsync(User user);
    bool IsLocked(User user);
}

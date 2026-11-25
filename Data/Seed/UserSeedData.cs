using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PopPop.Data.Entities;

namespace PopPop.Data.Seed;

/// <summary>
/// Demo accounts for testing and presenting the system: customers and two staff members.
/// They sign in with their phone number and the password <c>PopPop@123</c>. Values are constants so migrations stay stable.
/// </summary>
public static class UserSeedData
{
    /// <summary>Identity v3 hash of "PopPop@123".</summary>
    private const string DemoPasswordHash = "AQAAAAIAAYagAAAAEOEUdp9KRxpQhrsF0xcWtmWJpjBiTeczdlnUVx+AAjLzZ89rzp0ciJBzX5wC1bwvUQ==";

    private const string StaffRoleId = "2";
    private const string CustomerRoleId = "3";

    private sealed record Demo(
        string Id,
        string FullName,
        string Phone,
        DateTime CreatedAt,
        string RoleId,
        string? LockReason = null);

    private static readonly Demo[] Accounts =
    [
        // Customers (sign in at /login)
        new("customer-001", "Nguyễn Văn An", "0901234501", Utc(2026, 2, 3, 9, 15), CustomerRoleId),
        new("customer-002", "Trần Thị Bích Ngọc", "0912345602", Utc(2026, 3, 11, 20, 40), CustomerRoleId),
        new("customer-003", "Lê Hoàng Minh", "0987654303", Utc(2026, 4, 2, 13, 5), CustomerRoleId),
        new("customer-004", "Phạm Thu Hà", "0934567804", Utc(2026, 5, 19, 8, 30), CustomerRoleId),
        new("customer-005", "Hoàng Đức Huy", "0978123405", Utc(2026, 6, 7, 18, 55), CustomerRoleId),
        new("customer-006", "Vũ Thị Mai Anh", "0965432106", Utc(2026, 7, 23, 10, 20), CustomerRoleId),
        new("customer-007", "Đặng Quốc Bảo", "0923456707", Utc(2026, 8, 30, 21, 10), CustomerRoleId),
        new("customer-008", "Bùi Thanh Tâm", "0945678908", Utc(2026, 10, 14, 7, 45), CustomerRoleId),
        new("customer-009", "Đỗ Minh Khoa", "0956789009", Utc(2026, 11, 5, 16, 0), CustomerRoleId),
        new("customer-010", "Ngô Gia Hân", "0909876510", Utc(2026, 9, 9, 12, 25), CustomerRoleId,
            LockReason: "Nhiều lần từ chối nhận hàng COD (dữ liệu mẫu)."),

        // Staff (sign in at /admin/login)
        new("staff-001", "Trần Văn Long", "0911222301", Utc(2026, 1, 15, 8, 0), StaffRoleId),
        new("staff-002", "Nguyễn Thị Lan", "0911222302", Utc(2026, 3, 1, 8, 0), StaffRoleId)
    ];

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(Accounts.Select((account, index) => new User
        {
            Id = account.Id,
            UserName = account.Phone,
            NormalizedUserName = account.Phone,
            PasswordHash = DemoPasswordHash,
            SecurityStamp = $"DEMO-SECURITY-STAMP-{index + 1:000}",
            ConcurrencyStamp = $"demo-concurrency-{index + 1:000}",
            PhoneNumber = account.Phone,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            LockoutEnd = account.LockReason is null ? null : DateTimeOffset.MaxValue,
            LockReason = account.LockReason,
            AccessFailedCount = 0,
            FullName = account.FullName,
            CreatedAt = account.CreatedAt
        }).ToArray());

        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            Accounts.Select(account => new IdentityUserRole<string> { UserId = account.Id, RoleId = account.RoleId }).ToArray());
    }

    private static DateTime Utc(int year, int month, int day, int hour, int minute) =>
        new(year, month, day, hour, minute, 0, DateTimeKind.Utc);
}

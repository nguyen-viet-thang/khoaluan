using Microsoft.EntityFrameworkCore;
using PopPop.Data;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Repositories.Interfaces;

namespace PopPop.Repositories.Implementations;

public sealed class CustomerRepository : Repository<User, string>, ICustomerRepository
{
    private static readonly string[] BackOfficeRoles = ["Admin", "Staff"];

    public CustomerRepository(PopPopDbContext context)
        : base(context)
    {
    }

    public async Task<(IReadOnlyList<CustomerSummary> Customers, int TotalCount)> GetPageAsync(
        string? search,
        bool? isLocked,
        string sort,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var query = CustomersQuery();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(user => (user.FullName != null && user.FullName.Contains(term))
                || (user.UserName != null && user.UserName.Contains(term))
                || (user.PhoneNumber != null && user.PhoneNumber.Contains(term))
                || Context.Orders.Any(order => order.UserId == user.Id
                    && order.RecipientPhone != null && order.RecipientPhone.Contains(term)));
        }

        if (isLocked.HasValue)
        {
            query = isLocked.Value
                ? query.Where(user => user.LockoutEnd != null && user.LockoutEnd > now)
                : query.Where(user => user.LockoutEnd == null || user.LockoutEnd <= now);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var projected = Project(query, now);
        projected = sort switch
        {
            "spent" => projected.OrderByDescending(row => row.TotalSpent).ThenByDescending(row => row.OrderCount),
            "orders" => projected.OrderByDescending(row => row.OrderCount).ThenByDescending(row => row.TotalSpent),
            "recent-order" => projected.OrderByDescending(row => row.LastOrderAt),
            "name" => projected.OrderBy(row => row.FullName ?? row.Email ?? row.PhoneNumber),
            _ => projected.OrderByDescending(row => row.CreatedAt)
        };

        var rows = await projected
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (rows.Select(ToSummary).ToList(), totalCount);
    }

    public async Task<CustomerSummary?> GetSummaryAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var row = await Project(CustomersQuery().Where(user => user.Id == customerId), DateTimeOffset.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);
        return row is null ? null : ToSummary(row);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return await Context.Orders
            .AsNoTracking()
            .Include(order => order.OrderItems)
            .Where(order => order.UserId == customerId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsCustomerAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await CustomersQuery().AnyAsync(user => user.Id == userId, cancellationToken);
    }

    public Task<int> CountClaimsAsync(string customerId, CancellationToken cancellationToken = default) =>
        Context.WarrantyClaims.CountAsync(claim => claim.UserId == customerId, cancellationToken);

    /// <summary>Every account that does not hold a back-office role.</summary>
    private IQueryable<User> CustomersQuery()
    {
        var backOfficeUserIds = Context.UserRoles
            .Where(userRole => Context.Roles.Any(role => role.Id == userRole.RoleId && BackOfficeRoles.Contains(role.Name!)))
            .Select(userRole => userRole.UserId);

        return DbSet.AsNoTracking().Where(user => !backOfficeUserIds.Contains(user.Id));
    }

    private IQueryable<CustomerRow> Project(IQueryable<User> users, DateTimeOffset now)
    {
        return users.Select(user => new CustomerRow
        {
            Id = user.Id,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? (user.Email == null ? user.UserName : null),
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            IsLocked = user.LockoutEnd != null && user.LockoutEnd > now,
            OrderCount = Context.Orders.Count(order => order.UserId == user.Id),
            // Money actually received: only delivered (completed) orders count.
            TotalSpent = Context.Orders
                .Where(order => order.UserId == user.Id && order.Status == OrderStatus.Completed)
                .Sum(order => (decimal?)order.TotalAmount) ?? 0m,
            LastOrderAt = Context.Orders
                .Where(order => order.UserId == user.Id)
                .Max(order => (DateTime?)order.CreatedAt)
        });
    }

    private static CustomerSummary ToSummary(CustomerRow row) => new(
        row.Id, row.FullName, row.PhoneNumber, row.Email, row.EmailConfirmed, row.CreatedAt,
        row.IsLocked, row.OrderCount, row.TotalSpent, row.LastOrderAt);

    private sealed class CustomerRow
    {
        public required string Id { get; init; }
        public string? FullName { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public bool EmailConfirmed { get; init; }
        public DateTime CreatedAt { get; init; }
        public bool IsLocked { get; init; }
        public int OrderCount { get; init; }
        public decimal TotalSpent { get; init; }
        public DateTime? LastOrderAt { get; init; }
    }
}

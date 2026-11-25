using Microsoft.AspNetCore.Identity;
using PopPop.Data.Entities;
using PopPop.Data.Entities.Enums;
using PopPop.Models;
using PopPop.Repositories.Interfaces;
using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

public sealed class CustomerManagementService : ICustomerManagementService
{
    private const int PageSize = 20;

    private readonly ICustomerRepository _customerRepository;
    private readonly UserManager<User> _userManager;
    private readonly IAccountLockService _accountLockService;
    private readonly IAccountPasswordService _accountPasswordService;

    public CustomerManagementService(
        ICustomerRepository customerRepository,
        UserManager<User> userManager,
        IAccountLockService accountLockService,
        IAccountPasswordService accountPasswordService)
    {
        _customerRepository = customerRepository;
        _userManager = userManager;
        _accountLockService = accountLockService;
        _accountPasswordService = accountPasswordService;
    }

    public async Task<CustomerListViewModel> GetListAsync(
        string? search,
        string? status,
        string? sort,
        int page,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = status is "active" or "locked" ? status : null;
        bool? isLocked = normalizedStatus switch
        {
            "locked" => true,
            "active" => false,
            _ => null
        };
        var normalizedSort = sort is "spent" or "orders" or "recent-order" or "name" ? sort : "newest";
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        page = Math.Max(1, page);

        var (customers, totalCount) = await _customerRepository.GetPageAsync(
            normalizedSearch, isLocked, normalizedSort, page, PageSize, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
            (customers, totalCount) = await _customerRepository.GetPageAsync(
                normalizedSearch, isLocked, normalizedSort, page, PageSize, cancellationToken);
        }

        return new CustomerListViewModel
        {
            Customers = customers,
            Search = normalizedSearch,
            Status = normalizedStatus,
            Sort = normalizedSort,
            PageNumber = page,
            PageSize = PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CustomerDetailsViewModel?> GetDetailsAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var summary = await _customerRepository.GetSummaryAsync(customerId, cancellationToken);
        if (summary is null)
        {
            return null;
        }

        var user = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        var orders = await _customerRepository.GetOrdersAsync(customerId, cancellationToken);
        var completed = orders.Where(order => order.Status == OrderStatus.Completed).ToList();
        var lastOrder = orders.FirstOrDefault();

        return new CustomerDetailsViewModel
        {
            Customer = summary,
            LockReason = summary.IsLocked ? user?.LockReason : null,
            LockoutEnd = summary.IsLocked ? user?.LockoutEnd : null,
            LastShippingAddress = lastOrder?.ShippingAddress,
            LastRecipientPhone = lastOrder?.RecipientPhone,
            CompletedOrders = completed.Count,
            CancelledOrders = orders.Count(order => order.Status == OrderStatus.Cancelled),
            OpenOrders = orders.Count(order => order.Status is not (OrderStatus.Completed or OrderStatus.Cancelled)),
            AverageOrderValue = completed.Count == 0 ? 0m : Math.Round(completed.Average(order => order.TotalAmount)),
            ClaimCount = await _customerRepository.CountClaimsAsync(customerId, cancellationToken),
            Orders = orders.Select(order => new CustomerOrderRow
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                StatusText = GetStatusText(order.Status),
                ItemCount = order.OrderItems.Sum(item => item.Quantity),
                TotalAmount = order.TotalAmount
            }).ToList()
        };
    }

    public async Task LockAsync(string customerId, string reason, CancellationToken cancellationToken = default)
    {
        var user = await GetCustomerAsync(customerId, cancellationToken);
        await _accountLockService.LockAsync(user, reason);
    }

    public async Task UnlockAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var user = await GetCustomerAsync(customerId, cancellationToken);
        await _accountLockService.UnlockAsync(user);
    }

    public async Task<string> ResetPasswordAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var user = await GetCustomerAsync(customerId, cancellationToken);
        return await _accountPasswordService.ResetToTemporaryPasswordAsync(user);
    }

    public async Task<string> DeleteAsync(string customerId, string confirmPhone, CancellationToken cancellationToken = default)
    {
        var user = await GetCustomerAsync(customerId, cancellationToken);
        // Email accounts confirm with their email, phone accounts with their phone number.
        var isEmailAccount = !string.IsNullOrEmpty(user.Email);
        var login = isEmailAccount ? user.Email : user.PhoneNumber ?? user.UserName;
        var typed = isEmailAccount ? confirmPhone.Trim() : PhoneNumberFormatter.Normalize(confirmPhone);
        if (!string.Equals(typed, login, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(isEmailAccount
                ? "Email xác nhận không khớp. Vui lòng nhập đúng email của tài khoản cần xóa."
                : "Số điện thoại xác nhận không khớp. Vui lòng nhập đúng số điện thoại của tài khoản cần xóa.");
        }

        var orderCount = (await _customerRepository.GetOrdersAsync(customerId, cancellationToken)).Count;
        var claimCount = await _customerRepository.CountClaimsAsync(customerId, cancellationToken);
        if (orderCount > 0 || claimCount > 0)
        {
            throw new InvalidOperationException(
                $"Không thể xóa: tài khoản có {orderCount} đơn hàng và {claimCount} yêu cầu bảo hành cần lưu lại để đối soát. "
                + "Hãy khóa tài khoản thay vì xóa.");
        }

        // Cart, notifications, roles and external logins are removed by cascade.
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Không thể xóa tài khoản: " + string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        return login ?? customerId;
    }

    private async Task<User> GetCustomerAsync(string customerId, CancellationToken cancellationToken)
    {
        // Only customer accounts can be managed here; admin/staff accounts are out of scope.
        if (!await _customerRepository.IsCustomerAsync(customerId, cancellationToken))
        {
            throw new KeyNotFoundException("Không tìm thấy khách hàng.");
        }

        return await _userManager.FindByIdAsync(customerId)
            ?? throw new KeyNotFoundException("Không tìm thấy khách hàng.");
    }

    private static string GetStatusText(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Chờ xác nhận",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Processing => "Đang xử lý",
        OrderStatus.Shipping => "Đang giao",
        OrderStatus.Completed => "Đã giao thành công",
        OrderStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };
}

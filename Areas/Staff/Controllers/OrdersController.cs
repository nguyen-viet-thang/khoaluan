using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Controllers.Management;
using PopPop.Services.Interfaces;

namespace PopPop.Areas.Staff.Controllers;

[Area("Staff")]
[Authorize(Roles = "Admin,Staff")]
public sealed class OrdersController(IOrderService orderService) : OrderManagementController(orderService);

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PopPop.Areas.Staff.Controllers;

[Area("Staff")]
[Authorize(Roles = "Admin,Staff")]
public sealed class DashboardController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Orders", new { area = "Staff" });
    }

    /// <summary>What a staff account can and cannot do in the system.</summary>
    [HttpGet]
    public IActionResult Permissions()
    {
        return View();
    }
}

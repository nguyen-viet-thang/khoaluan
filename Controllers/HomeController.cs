using Microsoft.AspNetCore.Mvc;
using PopPop.Models;
using PopPop.Services.Interfaces;
using System.Diagnostics;

namespace PopPop.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = await _productService.GetHomeAsync(cancellationToken);
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

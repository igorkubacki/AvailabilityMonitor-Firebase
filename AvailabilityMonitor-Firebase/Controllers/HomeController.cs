using AvailabilityMonitor_Firebase.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AvailabilityMonitor_Firebase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:/Users/Igor/Dropbox/Komputer/Downloads/availability-monitor-7231f-ad6a7d6a6f03.json");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
using AvailabilityMonitor_Firebase.Models;
using Microsoft.AspNetCore.Mvc;

namespace AvailabilityMonitor_Firebase.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly BusinessLogic businessLogic;
        public ConfigurationController()
        {
            businessLogic = new BusinessLogic();
        }

        public async Task<IActionResult> Index()
        {
            if (!await businessLogic.ConfigExists())
                return View("Create");
            var config = new List<Config>() { await businessLogic.GetConfig() ?? new Config() };

            return View(config.AsEnumerable());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("supplierFileUrl,prestaShopUrl,prestaApiKey,currency")] Config config)
        {
            if (ModelState.IsValid)
            {
                await businessLogic.CreateConfig(config);
                return RedirectToAction(nameof(Index));
            }
            return View(config);
        }

        public async Task<IActionResult> Edit()
        {
            if (!await businessLogic.ConfigExists())
            {
                return Create();
            }

            var config = await businessLogic.GetConfig();
            if (config == null)
            {
                return NotFound();
            }

            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("id,supplierFileUrl,prestaShopUrl,prestaApiKey,currency")] Config config)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await businessLogic.CreateConfig(config);
                }
                catch (Exception)
                {
                    if (!await businessLogic.ConfigExists())
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(config);
        }
    }
}

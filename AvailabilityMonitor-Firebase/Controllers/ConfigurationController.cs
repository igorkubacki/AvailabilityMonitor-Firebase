using AvailabilityMonitor_Firebase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AvailabilityMonitor_Firebase.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly BusinessLogic businessLogic;
        public ConfigurationController()
        {
            businessLogic = new BusinessLogic();
        }

        // GET: Configuration
        public async Task<IActionResult> Index()
        {
            if (!await businessLogic.ConfigExists())
                return View("Create");
            var config = new List<Config>() { await businessLogic.GetConfig() };
            
            return View(config.AsEnumerable());
        }

        // GET: Configuration/Details/5
        public async Task<IActionResult> Details()
        {
            if (!await businessLogic.ConfigExists())
            {
                return NotFound();
            }

            var config = await businessLogic.GetConfig();

            return View(config);
        }

        // GET: Configuration/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Configuration/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Configuration/Edit/5
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

        // POST: Configuration/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,supplierFileUrl,prestaShopUrl,prestaApiKey,currency")] Config config)
        {
            if (id != config.id)
            {
                return NotFound();
            }

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

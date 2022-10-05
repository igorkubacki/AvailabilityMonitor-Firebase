using AvailabilityMonitor_Firebase.Models;
using Microsoft.AspNetCore.Mvc;

namespace AvailabilityMonitor_Firebase.Controllers
{
    public class Notifications : Controller
    {
        private readonly BusinessLogic _businessLogic;

        public Notifications()
        {
            _businessLogic = new BusinessLogic();
        }

        public async Task<ActionResult> Index()
        {
            var priceChanges = await _businessLogic.GetAllPriceChanges();
            var quantityChanges = await _businessLogic.GetAllQuantityChanges();

            if (priceChanges != null)
                ViewData["PriceChanges"] = priceChanges.Where(p => p.IsNotificationRead == false);

            if (quantityChanges != null)
                ViewData["QuantityChanges"] = quantityChanges.Where(p => p.IsNotificationRead == false);

            return View();
        }

        public async Task MarkPriceChangeAsRead(string id, int productId)
        {
            await _businessLogic.MarkPriceChangeAsRead(id, productId);
        }

        public async Task MarkQuantityChangeAsRead(string id, int productId)
        {
            await _businessLogic.MarkQuantityChangeAsRead(id, productId);
        }

        public async Task MarkAllChangesAsRead()
        {
            await _businessLogic.MarkAllPriceChangesAsRead();
            await _businessLogic.MarkAllQuantityChangesAsRead();
        }
    }
}

using AvailabilityMonitor_Firebase.Models;
using Microsoft.AspNetCore.Mvc;

namespace AvailabilityMonitor_Firebase.Controllers
{
    public class Notifications : Controller
    {
        private BusinessLogic _businessLogic;
        public Notifications()
        {
            this._businessLogic = new BusinessLogic();
        }

        // GET: Notifications
        public async Task<ActionResult> Index()
        {
            var priceChanges = await _businessLogic.GetAllPriceChanges();
            var quantityChanges= await _businessLogic.GetAllQuantityChanges();

            if (priceChanges != null)
                ViewData["PriceChanges"] = priceChanges.Where(p => p.IsNotificationRead == false);

            if(quantityChanges != null)
                ViewData["QuantityChanges"] = quantityChanges.Where(p => p.IsNotificationRead == false);

            return View();
        }


        public void MarkPriceChangeAsRead(string id, int productId)
        {
            _businessLogic.MarkPriceChangeAsRead(id, productId);
        }

        public void MarkQuantityChangeAsRead(string id, int productId)
        {
            _businessLogic.MarkQuantityChangeAsRead(id, productId);
        }

        public async void MarkAllChangesAsRead()
        {
            IEnumerable<PriceChange>? priceChanges = await _businessLogic.GetAllPriceChanges();
            IEnumerable<QuantityChange>? quantityChanges = await _businessLogic.GetAllQuantityChanges();

            foreach (PriceChange change in priceChanges)
            {
                change.IsNotificationRead = true;
            }
            foreach (QuantityChange change in quantityChanges)
            {
                change.IsNotificationRead = true;
            }
        }
    }
}

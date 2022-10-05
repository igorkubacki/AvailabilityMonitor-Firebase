using AvailabilityMonitor_Firebase.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;

namespace AvailabilityMonitor_Firebase.Controllers
{
    public class ProductsController : Controller
    {
        private readonly BusinessLogic _businessLogic;
        private static readonly string[] months = {
            "Jan",
            "Feb",
            "Mar",
            "Apr",
            "May",
            "Jun",
            "Jul",
            "Aug",
            "Sept",
            "Oct",
            "Nov",
            "Dec"
        };
        public ProductsController()
        {
            _businessLogic = new BusinessLogic();
        }

        public async Task<ActionResult> Index(string? sortOrder, string name, string index, int? prestashopId, float? priceFrom, float? priceTo,
            int? quantityFrom, int? quantityTo, int? page, int? pageSize)
        {
            ProductSearch searchModel = new()
            {
                Name = name ?? "",
                Index = index ?? "",
                PrestashopId = prestashopId,
                PriceFrom = priceFrom,
                PriceTo = priceTo,
                QuantityFrom = quantityFrom,
                QuantityTo = quantityTo
            };

            IQueryable<Product>? products = await _businessLogic.GetProducts(searchModel);

            products = sortOrder switch
            {
                "id_desc" => products.OrderByDescending(p => p.PrestashopId),
                "name" => products.OrderBy(p => p.Name),
                "name_desc" => products.OrderByDescending(p => p.Name),
                "price" => products.OrderBy(p => p.RetailPrice),
                "price_desc" => products.OrderByDescending(p => p.RetailPrice),
                "quantity" => products.OrderBy(p => p.Quantity),
                "quantity_desc" => products.OrderByDescending(p => p.Quantity),
                _ => products.OrderBy(p => p.PrestashopId),
            };
            int productsPerPage = pageSize ?? 10;
            int pageNumber = page ?? 1;

            Config? config = await _businessLogic.GetConfig();
            ViewData["currency"] = config?.Currency;

            return View(products.ToPagedList(pageNumber, productsPerPage));
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product? product = await _businessLogic.GetProductById((int)id);

            if (product == null)
            {
                return NotFound();
            }

            // Price chart data handling
            List<float> prices = new List<float>();
            string priceLabels = "";

            if (await _businessLogic.AnyPriceChangesForProduct((int)id))
            {
                IEnumerable<PriceChange> priceChanges = await _businessLogic.GetPriceChangesForProduct((int)id);

                priceChanges = priceChanges.OrderBy(p => p.DateTime);

                foreach (PriceChange change in priceChanges)
                {
                    prices.Add(change.NewPrice);
                    priceLabels += change.DateTime.Day.ToString() + " " + months[change.DateTime.Month - 1] + " " + change.DateTime.Year.ToString() + ",";
                }
                priceLabels = priceLabels.Substring(0, priceLabels.Length - 1);
            }

            string pricesJson = JsonConvert.SerializeObject(prices);
            ViewData["pricesJson"] = pricesJson;
            ViewData["priceLabelsJson"] = priceLabels;


            // Quantity chart data handling
            List<int> quantities = new List<int>();
            string quantityLabels = "";

            if (await _businessLogic.AnyQuantityChangesForProduct((int)id))
            {
                IEnumerable<QuantityChange> quantityChanges = await _businessLogic.GetQuantityChangesForProduct((int)id);

                quantityChanges = quantityChanges.OrderBy(p => p.DateTime);

                foreach (QuantityChange change in quantityChanges)
                {
                    quantities.Add(change.NewQuantity);
                    quantityLabels += change.DateTime.Day.ToString() + " " + months[change.DateTime.Month - 1] + " " + change.DateTime.Year.ToString() + ",";
                }
                quantityLabels = quantityLabels.Substring(0, quantityLabels.Length - 1);
            }

            string quantitiesJson = JsonConvert.SerializeObject(quantities);
            ViewData["quantitiesJson"] = quantitiesJson;
            ViewData["quantityLabelsJson"] = quantityLabels;

            return View(product);
        }

        public async Task DeleteProduct(int? id)
        {
            await _businessLogic.DeleteProduct(id);
        }

        public async Task UpdateAllProductsFromPresta()
        {
            await _businessLogic.ImportProductsFromPresta();
        }

        public async Task UpdateProductFromPresta(int id)
        {
            await _businessLogic.UpdateProductFromPresta(id);
        }

        public async Task UpdateAllProductsSupplierInfo()
        {
            await _businessLogic.UpdateSupplierInfo();
        }

        public async Task UpdateProductSupplierInfo(int id)
        {
            await _businessLogic.UpdateSupplierInfo(id);
        }
    }
}

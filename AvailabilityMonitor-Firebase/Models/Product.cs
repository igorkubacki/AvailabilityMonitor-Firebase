using System.ComponentModel.DataAnnotations;

namespace AvailabilityMonitor_Firebase.Models
{
    public class Product
    {
        public int PrestashopId { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public string PhotoURL { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public float? SupplierWholesalePrice { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public float RetailPrice { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public float? SupplierRetailPrice { get; set; }
        public int Quantity { get; set; }
        public int? SupplierQuantity { get; set; }
        public int StockavailableId { get; set; }
        public string? AvailabilityLabel { get; set; }
        public bool? IsVisible { get; set; }

        public Product(int prestashopId, string index, string name, string photoURL, int stockavailableId,
            int quantity, float retailPrice, string availabilityLabel)
        {
            PrestashopId = prestashopId;
            Index = index;
            Name = name;
            PhotoURL = photoURL;
            StockavailableId = stockavailableId;
            Quantity = quantity;
            RetailPrice = retailPrice;
            AvailabilityLabel = availabilityLabel;
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace AvailabilityMonitor_Firebase.Models
{
    public class Config
    {
        [DataType(DataType.Url)]
        [Display(Name = "Adress of the XML file")]
        public string SupplierFileUrl { get; set; }
        [Display(Name = "PrestaShop adress")]
        [DataType(DataType.Url)]
        public string PrestaShopUrl { get; set; }
        [StringLength(60)]
        [Display(Name = "PrestaShop API key")]
        public string PrestaApiKey { get; set; }
        [Display(Name = "Currency")]
        public string? Currency { get; set; }
    }
}
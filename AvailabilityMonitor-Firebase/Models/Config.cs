using System.ComponentModel.DataAnnotations;

namespace AvailabilityMonitor_Firebase.Models
{
    public class Config
    {
        [Key]
        public int id { get; set; }
        [DataType(DataType.Url)]
        [Display(Name = "Adress of the XML file")]
        public string supplierFileUrl { get; set; }
        [Display(Name = "PrestaShop adress")]
        [DataType(DataType.Url)]
        public string prestaShopUrl { get; set; }
        [StringLength(60)]
        [Display(Name = "PrestaShop API key")]
        public string prestaApiKey { get; set; }
        [Display(Name = "Currency")]
        public string? currency { get; set; }
    }
}
namespace AvailabilityMonitor_Firebase.Models
{
    public class ProductSearch
    {
        public int? PrestashopId { get; set; }
        public string? Name { get; set; }
        public string? Index { get; set; }
        public float? PriceFrom { get; set; }
        public float? PriceTo { get; set; }
        public int? QuantityFrom { get; set; }
        public int? QuantityTo { get; set; }
    }
}

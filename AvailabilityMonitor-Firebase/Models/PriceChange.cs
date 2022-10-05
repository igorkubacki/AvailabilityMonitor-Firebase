namespace AvailabilityMonitor_Firebase.Models
{
    public class PriceChange
    {
        public float PreviousPrice { get; set; }
        public float NewPrice { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsNotificationRead { get; set; } = false;
        public int ProductId { get; set; }
        public string? productName { get; set; }

        public PriceChange(int productId, float previousPrice, float newPrice, DateTime dateTime, bool isNotificationRead)
        {
            ProductId = productId;
            PreviousPrice = previousPrice;
            NewPrice = newPrice;
            DateTime = dateTime;
            IsNotificationRead = isNotificationRead;
        }
    }
}

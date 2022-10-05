namespace AvailabilityMonitor_Firebase.Models
{
    public class QuantityChange
    {
        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsNotificationRead { get; set; } = false;
        public int ProductId { get; set; }
        public string? productName { get; set; }

        public QuantityChange(int productId, int previousQuantity, int newQuantity, DateTime dateTime, bool isNotificationRead)
        {
            ProductId = productId;
            PreviousQuantity = previousQuantity;
            NewQuantity = newQuantity;
            DateTime = dateTime;
            IsNotificationRead = isNotificationRead;
        }
    }
}

using Google.Cloud.Firestore;

namespace AvailabilityMonitor_Firebase.Models
{
    public partial class BusinessLogic
    {
        private static QuantityChange SnapshotToQuantityChange(DocumentSnapshot snapshot)
        {
            return new QuantityChange(
                int.Parse(snapshot.Reference.Parent.Parent.Id),
                snapshot.GetValue<int>("previousQuantity"),
                snapshot.GetValue<int>("newQuantity"),
                DateTime.ParseExact(snapshot.Id, formats, System.Globalization.CultureInfo.InvariantCulture),
                snapshot.GetValue<bool>("isNotificationRead")
            );
        }

        public async Task<IEnumerable<QuantityChange>> GetAllQuantityChanges()
        {
            QuerySnapshot? collectionSnapshot = await db.Collection("products").Select("quantityChanges", "name").GetSnapshotAsync();
            var quantityChanges = new List<QuantityChange>();

            foreach (DocumentSnapshot snapshot in collectionSnapshot.Documents)
            {
                QuerySnapshot? quantityChangesSnapshot = await snapshot.Reference.Collection("quantityChanges").GetSnapshotAsync();

                foreach (DocumentSnapshot quantityChangeSnapshot in quantityChangesSnapshot.Documents)
                {
                    QuantityChange quantityChange = SnapshotToQuantityChange(quantityChangeSnapshot);
                    quantityChange.productName = snapshot.GetValue<string>("name");
                    quantityChanges.Add(quantityChange);
                }
            }
            return quantityChanges.AsEnumerable();
        }

        public async Task<QuantityChange> GetQuantityChangeById(string id, int productId)
        {
            IEnumerable<QuantityChange>? quantityChanges = await GetQuantityChangesForProduct(productId);

            return quantityChanges.Where(c => c.DateTime.ToString() == id).First();
        }

        public async Task<IEnumerable<QuantityChange>> GetQuantityChangesForProduct(int productId)
        {
            QuerySnapshot? collectionSnapshot = await db.Collection("products").Document(productId.ToString()).Collection("quantityChanges").GetSnapshotAsync();

            List<QuantityChange> quantityChanges = new List<QuantityChange>();

            foreach (DocumentSnapshot snapshot in collectionSnapshot.Documents)
            {
                quantityChanges.Add(SnapshotToQuantityChange(snapshot));
            }

            return quantityChanges.AsEnumerable();
        }

        public async void InsertQuantityChange(QuantityChange quantityChange)
        {
            DocumentReference docRef = db.Collection("products").Document(quantityChange.ProductId.ToString())
                .Collection("quantityChanges").Document(quantityChange.DateTime.ToString());
            Dictionary<string, object> entry = new Dictionary<string, object>
            {
                {"previousQuantity", quantityChange.PreviousQuantity},
                {"newQuantity", quantityChange.NewQuantity},
                {"isNotificationRead", quantityChange.IsNotificationRead}
            };

            await docRef.SetAsync(entry);
        }

        public async Task<bool> AnyQuantityChangesForProduct(int productId)
        {
            QuerySnapshot? collectionReference = await db.Collection("products").Document(productId.ToString())
                .Collection("quantityChanges").GetSnapshotAsync();

            return collectionReference.Documents.Any();
        }

        public async Task MarkQuantityChangeAsRead(string id, int productId)
        {
            DocumentReference docRef = db.Collection("products").Document(productId.ToString()).Collection("quantityChanges").Document(id);
            var entry = new Dictionary<string, object>
            {
                {"isNotificationRead", true}
            };
            await docRef.UpdateAsync(entry);
        }

        public async Task MarkAllQuantityChangesAsRead()
        {
            IEnumerable<QuantityChange>? quantityChanges = await GetAllQuantityChanges();

            // Get only unread notifications.
            quantityChanges = quantityChanges.Where(p => p.IsNotificationRead == false);

            foreach (QuantityChange change in quantityChanges)
            {
                change.IsNotificationRead = true;
                InsertQuantityChange(change);
            }
        }
    }
}

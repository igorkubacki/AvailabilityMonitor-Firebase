using Google.Cloud.Firestore;

namespace AvailabilityMonitor_Firebase.Models
{
    public partial class BusinessLogic
    {
        public async Task<IEnumerable<PriceChange>> GetAllPriceChanges()
        {
            QuerySnapshot? collectionSnapshot = await db.Collection("products").Select("priceChanges", "name").GetSnapshotAsync();
            var priceChanges = new List<PriceChange>();

            foreach(DocumentSnapshot snapshot in collectionSnapshot.Documents)
            {
                QuerySnapshot? priceChangesSnapshot = await snapshot.Reference.Collection("priceChanges").GetSnapshotAsync();

                foreach(DocumentSnapshot priceChangeSnapshot in priceChangesSnapshot.Documents)
                {
                    PriceChange priceChange = SnapshotToPriceChange(priceChangeSnapshot);
                    priceChange.productName = snapshot.GetValue<string>("name");
                    priceChanges.Add(priceChange);
                }
            }
            return priceChanges.AsEnumerable();
        }

        private PriceChange SnapshotToPriceChange(DocumentSnapshot snapshot)
        {
            return new PriceChange(
                int.Parse(snapshot.Reference.Parent.Parent.Id),
                snapshot.GetValue<float>("previousPrice"),
                snapshot.GetValue<float>("newPrice"),
                DateTime.Parse(snapshot.Id),
                snapshot.GetValue<bool>("isNotificationRead")
            );
        }

        public async Task<PriceChange> GetPriceChangeById(string id, int productId)
        {
            IEnumerable<PriceChange>? priceChanges = await GetPriceChangesForProduct(productId);

            return priceChanges.Where(c => c.DateTime.ToString() == id).First();
        }
        public async Task<IEnumerable<PriceChange>> GetPriceChangesForProduct(int productId)
        {
            QuerySnapshot? collectionSnapshot = await db.Collection("products").Document(productId.ToString()).Collection("priceChanges").GetSnapshotAsync();

            List<PriceChange> priceChanges = new List<PriceChange>();

            foreach(DocumentSnapshot snapshot in collectionSnapshot.Documents)
            {
                priceChanges.Add(SnapshotToPriceChange(snapshot));
            }

            return priceChanges.AsEnumerable();
        }
        public async void InsertPriceChange(PriceChange priceChange)
        {
            DocumentReference docRef = db.Collection("products").Document(priceChange.ProductId.ToString())
                .Collection("priceChanges").Document(priceChange.DateTime.ToString());
            Dictionary<string, object> entry = new Dictionary<string, object>
            {
                {"previousPrice", priceChange.PreviousPrice},
                {"newPrice", priceChange.NewPrice},
                {"isNotificationRead", priceChange.IsNotificationRead}
            };

            await docRef.SetAsync(entry);
        }
        public async Task<bool> AnyPriceChangesForProduct(int productId)
        {
            QuerySnapshot? collectionReference = await db.Collection("products").Document(productId.ToString())
                .Collection("priceChanges").GetSnapshotAsync();

            return collectionReference.Documents.Any();
        }

        public async void MarkPriceChangeAsRead(string id, int productId)
        {
            DocumentReference docRef = db.Collection("products").Document(productId.ToString()).Collection("priceChanges").Document(id);
            var entry = new Dictionary<string, object>
            {
                {"isNotificationRead", true}
            };
            await docRef.UpdateAsync(entry);
        }
    }
}

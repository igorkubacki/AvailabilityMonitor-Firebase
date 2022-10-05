using Google.Cloud.Firestore;
using System.Globalization;
using System.Net.Http.Headers;
using System.Web.Mvc;
using System.Xml;

namespace AvailabilityMonitor_Firebase.Models
{
    public partial class BusinessLogic : IDisposable
    {
        FirestoreDb db;
        public BusinessLogic()
        {
            db = FirestoreDb.Create("availability-monitor-7231f");
        }
        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            QuerySnapshot? collectionSnapshot = await db.Collection("products").GetSnapshotAsync();
            var products = new List<Product>();

            foreach(DocumentSnapshot snapshot in collectionSnapshot.Documents)
            {
                products.Add(SnapshotToProduct(snapshot));
            }

            return products.AsEnumerable();
        }

        public async Task<bool> ConfigExists()
        {
            var collectionSnapshot = await db.Collection("config").GetSnapshotAsync();
            return collectionSnapshot.Documents.Count == 0 ? false : true;
        }

        public async Task<Product?> GetProductById(int id)
        {
            var snapshot = await db.Collection("products").Document(id.ToString()).GetSnapshotAsync();

            return SnapshotToProduct(snapshot);
        }
        private Product SnapshotToProduct(DocumentSnapshot snapshot)
        {
            Product product = new Product(
                int.Parse(snapshot.Id), 
                snapshot.GetValue<string>("index"), 
                snapshot.GetValue<string>("name"), 
                snapshot.GetValue<string>("photoURL"), 
                snapshot.GetValue<int>("stockavailableId"),
                snapshot.GetValue<int>("quantity"),
                snapshot.GetValue<float>("retailPrice"),
                snapshot.GetValue<string>("availabilityLabel")
            );

            snapshot.TryGetValue<int?>("supplierQuantity", out int? check);
            if (check != null)
            {
                product.SupplierQuantity = snapshot.GetValue<int>("supplierQuantity");
                product.SupplierRetailPrice = snapshot.GetValue<float>("supplierRetailPrice");
                product.SupplierWholesalePrice = snapshot.GetValue<float>("supplierWholesalePrice");
                product.IsVisible = snapshot.GetValue<bool>("isVisible");
            }

            return product;
        }
        public async Task<IQueryable<Product>> GetProducts(ProductSearch? searchModel)
        {
            CollectionReference? collectionRef = db.Collection("products");
            QuerySnapshot? snapshot = await collectionRef.GetSnapshotAsync();
            var productsList = new List<Product>();

            foreach(DocumentSnapshot docSnap in snapshot)
            {
                productsList.Add(SnapshotToProduct(docSnap));
            }

            var products = productsList.AsQueryable();

            if (searchModel != null)
            {
                if (searchModel.PrestashopId.HasValue)
                    products = products.Where(p => p.PrestashopId == searchModel.PrestashopId);
                if (!string.IsNullOrEmpty(searchModel.Name))
                    products = products.Where(p => p.Name.ToLower().Contains(searchModel.Name.ToLower()));
                if (!string.IsNullOrEmpty(searchModel.Index))
                    products = products.Where(p => p.Index.ToLower().Contains(searchModel.Index.ToLower()));
                if (searchModel.PriceFrom.HasValue)
                    products = products.Where(p => p.RetailPrice >= searchModel.PriceFrom);
                if (searchModel.PriceTo.HasValue)
                    products = products.Where(p => p.RetailPrice <= searchModel.PriceTo);
                if (searchModel.QuantityFrom.HasValue)
                    products = products.Where(p => p.Quantity >= searchModel.QuantityFrom);
                if (searchModel.QuantityTo.HasValue)
                    products = products.Where(p => p.Quantity <= searchModel.QuantityTo);
            }
            
            return products;
        }

        public async Task CreateConfig(Config config)
        {
            DocumentReference docRef = db.Collection("config").Document("config");
            Dictionary<string, object> entry = new Dictionary<string, object>
            {
                {"prestaShopUrl", config.prestaShopUrl },
                {"supplierFileUrl", config.supplierFileUrl},
                {"prestaApiKey", config.prestaApiKey },
                {"currency", config.currency ?? ""}
            };
            await docRef.SetAsync(entry);
        }

        public async void InsertOrUpdateProduct(Product product)
        {
            DocumentReference docRef = db.Collection("products").Document(product.PrestashopId.ToString());
            Dictionary<string, object> entry = new Dictionary<string, object>
            {
                {"index", product.Index },
                {"name", product.Name},
                {"photoURL", product.PhotoURL },
                {"retailPrice", product.RetailPrice },
                {"quantity", product.Quantity },
                {"stockavailableId", product.StockavailableId },
                {"availabilityLabel", product.AvailabilityLabel },
                {"supplierQuantity", product.SupplierQuantity },
                {"supplierRetailPrice", product.SupplierRetailPrice },
                {"supplierWholesalePrice", product.SupplierWholesalePrice },
                {"isVisible", product.IsVisible }
            };

            if(docRef.GetSnapshotAsync().Result.Exists)
            {
                await docRef.UpdateAsync(entry);
            }
            else
            {
                await docRef.SetAsync(entry);
            }
        }
        public async Task DeleteProduct(int? productId)
        {
            if (productId != null)
            {
                // Deleting all price and quantity changes for product.
                var priceChangesSnapshot = await db.Collection("products").Document(productId.ToString()).Collection("priceChanges").GetSnapshotAsync();
                
                foreach(DocumentSnapshot priceChange in priceChangesSnapshot.Documents)
                {
                    await priceChange.Reference.DeleteAsync();
                }
                
                var quantityChangesSnapshot = await db.Collection("products").Document(productId.ToString()).Collection("quantityChanges").GetSnapshotAsync();
                
                foreach(DocumentSnapshot quantityChange in quantityChangesSnapshot.Documents)
                {
                    await quantityChange.Reference.DeleteAsync();
                }


                // Deleting product.
                var docRef = db.Collection("products").Document(productId.ToString());
                await docRef.DeleteAsync();
            }
        }

        public async Task<bool> AnyProducts()
        {
            QuerySnapshot? productsSnapshot = await db.Collection("products").GetSnapshotAsync();

            return productsSnapshot.Documents.Count == 0;
        }
        public async Task<Config?> GetConfig()
        {
            DocumentReference docRef = db.Collection("config").Document("config");
            DocumentSnapshot? snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.ContainsField("prestaApiKey")) return new Config();

            return new Config()
            {
                prestaApiKey = snapshot.GetValue<string>("prestaApiKey"),
                prestaShopUrl = snapshot.GetValue<string>("prestaShopUrl"),
                supplierFileUrl = snapshot.GetValue<string>("supplierFileUrl"),
                currency = snapshot.GetValue<string>("currency")
            };
        }

        private async Task<XmlDocument> ResponseIntoXml(HttpResponseMessage result)
        {
            string? resultContent = await result.Content.ReadAsStringAsync();

            XmlDocument XmlFile = new XmlDocument();
            XmlFile.LoadXml(resultContent);

            return XmlFile;
        }
        private async Task<int> GetPrestaQuantity(HttpClient client, Config config, int stockavailableId)
        {
            Task<HttpResponseMessage>? request = client.GetAsync("stock_availables/" + stockavailableId + "?ws_key=" + config.prestaApiKey);
            request.Wait();
            HttpResponseMessage result = request.Result;

            if (result.IsSuccessStatusCode)
            {
                XmlDocument xmlDocument = await ResponseIntoXml(result);

                return int.Parse(xmlDocument.GetElementsByTagName("quantity").Item(0).InnerText);
            }
            else
            {
                throw new BadHttpRequestException("Status code " + result.StatusCode.ToString() + " - " + result.ReasonPhrase);
            }
        }
        public async Task ImportProductsFromPresta()
        {
            Config? config = await GetConfig();

            if (config is null)
            {
                return;
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(config.prestaShopUrl + "/api/");

            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/xml"));

            // getting all products id's

            Task<HttpResponseMessage>? request = client.GetAsync("products/?ws_key=" + config.prestaApiKey);
            request.Wait();
            HttpResponseMessage? result = request.Result;

            if (result.IsSuccessStatusCode)
            {
                XmlDocument AllProductsXML = await ResponseIntoXml(result);

                foreach (XmlNode node in AllProductsXML.ChildNodes.Item(1).ChildNodes.Item(0).ChildNodes)
                {

                    int productId = int.Parse(node.Attributes[0].Value);

                    Task<HttpResponseMessage>? productRequest = client.GetAsync("products/" + productId + "?ws_key=" + config.prestaApiKey);
                    productRequest.Wait();
                    HttpResponseMessage? productResult = productRequest.Result;

                    if (productResult.IsSuccessStatusCode)
                    {
                        XmlDocument productInfoXML = await ResponseIntoXml(productResult);

                        // Skip product if it's not displayed in PrestaShop.
                        if (productInfoXML.GetElementsByTagName("active").Item(0).InnerText == "0")
                        {
                            continue;
                        }

                        string index = productInfoXML.GetElementsByTagName("reference").Item(0).InnerText;
                        string name = productInfoXML.GetElementsByTagName("name").Item(0).ChildNodes.Item(0).InnerText;
                        string photoURL = config.prestaShopUrl + "/" + productInfoXML.GetElementsByTagName("id_default_image").Item(0).InnerText
                            + "-home_default/p.jpg";
                        string availabilityLabel = productInfoXML.GetElementsByTagName("available_now").Item(0).ChildNodes.Item(0).InnerText;
                        int stockavailableId = int.Parse(productInfoXML.GetElementsByTagName("stock_available").Item(0).ChildNodes.Item(0).InnerText);
                        int quantity = await GetPrestaQuantity(client, config, stockavailableId);
                        float retailPrice = (float)1.23 * float.Parse(productInfoXML.GetElementsByTagName("price").Item(0).InnerText, CultureInfo.InvariantCulture);
                        retailPrice = Convert.ToSingle(retailPrice);

                        
                        
                        InsertOrUpdateProduct(new Product(productId, index, name, photoURL, stockavailableId, quantity, retailPrice, availabilityLabel));
                        
                    }
                    else
                    {
                        throw new BadHttpRequestException("Status code " + productResult.StatusCode.ToString() + " - " + productResult.ReasonPhrase);
                    }
                }
            }

            else
            {
                throw new BadHttpRequestException("Status code " + result.StatusCode.ToString() + " - " + result.ReasonPhrase);
            }
            client.Dispose();
            return;
        }

        public async Task UpdateProductFromPresta(int id)
        {
            Config? config = await GetConfig();

            if (config is null)
            {
                return;
            }

            Product? product = await GetProductById(id);

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(config.prestaShopUrl + "/api/");

            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/xml"));

            Task<HttpResponseMessage>? request = client.GetAsync("products/" + product.PrestashopId + "?ws_key=" + config.prestaApiKey);
            request.Wait();
            HttpResponseMessage? result = request.Result;

            if (result.IsSuccessStatusCode)
            {
                XmlDocument productInfoXML = await ResponseIntoXml(result);

                string index = productInfoXML.GetElementsByTagName("reference").Item(0).InnerText;
                string name = productInfoXML.GetElementsByTagName("name").Item(0).ChildNodes.Item(0).InnerText;
                string photoURL = config.prestaShopUrl + "/" + productInfoXML.GetElementsByTagName("id_default_image").Item(0).InnerText
                    + "-home_default/p.jpg";
                string availabilityLabel = productInfoXML.GetElementsByTagName("available_now").Item(0).ChildNodes.Item(0).InnerText;
                int stockavailableId = int.Parse(productInfoXML.GetElementsByTagName("stock_available").Item(0).ChildNodes.Item(0).InnerText);
                int quantity = await GetPrestaQuantity(client, config, stockavailableId);
                float retailPrice = (float)1.23 * float.Parse(productInfoXML.GetElementsByTagName("price").Item(0).InnerText, CultureInfo.InvariantCulture);
                retailPrice = Convert.ToSingle(retailPrice);

                product.Index = index;
                product.Name = name;
                product.PhotoURL = photoURL;
                product.AvailabilityLabel = availabilityLabel;
                product.StockavailableId = stockavailableId;
                product.Quantity = quantity;
                product.RetailPrice = retailPrice;

                InsertOrUpdateProduct(product);
            }
            else
            {
                throw new BadHttpRequestException("Status code " + result.StatusCode.ToString() + " - " + result.ReasonPhrase);
            }
        }

        public async Task<bool> UpdateSupplierInfo()
        {
            Config? config = await GetConfig();

            if (config is null)
            {
                return false;
            }

            HttpClient client = new HttpClient();

            Task<HttpResponseMessage>? request = client.GetAsync(config.supplierFileUrl);
            request.Wait();
            HttpResponseMessage? result = request.Result;

            if (result.IsSuccessStatusCode)
            {
                XmlDocument supplierProducts = await ResponseIntoXml(result);
                XmlNodeList? indexNodes = supplierProducts.GetElementsByTagName("Kod");

                IEnumerable<Product>? products = await GetAllProducts();


                foreach (Product product in products)
                {
                    foreach (XmlNode node in indexNodes)
                    {
                        var index = node.InnerText.Trim();
                        if (product.Index == index)
                        {
                            XmlNode? supplierProduct = node.ParentNode;

                            Product updatedProduct = UpdateProductSupplierInfo(product, supplierProduct);
                            InsertOrUpdateProduct(updatedProduct);
                            break;
                        }
                    }
                }
            }
            else
            {
                throw new BadHttpRequestException("Status code " + result.StatusCode.ToString() + " - " + result.ReasonPhrase);
            }
            return true;
        }
        public async Task<bool> UpdateInfoFromXmlFile(int productId)
        {
            Config? config = await GetConfig();

            if (config is null)
            {
                return false;
            }

            HttpClient client = new HttpClient();

            Task<HttpResponseMessage>? request = client.GetAsync(config.supplierFileUrl);
            request.Wait();
            HttpResponseMessage? result = request.Result;

            if (result.IsSuccessStatusCode)
            {
                XmlDocument supplierProducts = await ResponseIntoXml(result);
                XmlNodeList? indexes = supplierProducts.GetElementsByTagName("Kod");

                Product? product = await GetProductById(productId);

                if (product == null) return false;

                foreach (XmlNode node in indexes)
                {
                    var index = node.InnerText.Trim();
                    if (product.Index == index)
                    {
                        XmlNode supplierProduct = node.ParentNode;

                        Product updatedProduct = UpdateProductSupplierInfo(product, supplierProduct);
                        InsertOrUpdateProduct(updatedProduct);
                    }
                }
            }
            else
            {
                throw new BadHttpRequestException("Status code " + result.StatusCode.ToString() + " - " + result.ReasonPhrase);
            }

            return true;
        }
        private Product UpdateProductSupplierInfo(Product product, XmlNode node)
        {
            int supplierQuantity = int.Parse(node.ChildNodes.Item(8).InnerText.Trim());

            // If value is different than the previous one, create a change object.
            if (product.SupplierQuantity != supplierQuantity && product.SupplierQuantity != null)
            {
                InsertQuantityChange(new QuantityChange(product.ProductId, (int)product.SupplierQuantity, supplierQuantity, DateTime.Now, false));
            }
            // If it's first import, then create a change.
            if (product.SupplierQuantity == null)
            {
                InsertQuantityChange(new QuantityChange(product.ProductId, 0, supplierQuantity, DateTime.Now, true));

            }
            product.SupplierQuantity = supplierQuantity;


            float supplierRetailPrice = float.Parse(node.ChildNodes.Item(7).InnerText.Trim(), CultureInfo.InvariantCulture);
            // If value is different than the previous one, create a change object.
            if (product.SupplierRetailPrice != supplierRetailPrice && product.SupplierRetailPrice != null)
            {
                InsertPriceChange(new PriceChange(product.ProductId, (float)product.SupplierRetailPrice, supplierRetailPrice, DateTime.Now, false));
            }
            // If it's first import, then create a change.
            if (product.SupplierRetailPrice == null)
            {
                InsertPriceChange(new PriceChange(product.PrestashopId, 0, supplierRetailPrice, DateTime.Now, true));
            }
            product.SupplierRetailPrice = supplierRetailPrice;


            product.SupplierWholesalePrice = float.Parse(node.ChildNodes.Item(5).InnerText.Trim(), CultureInfo.InvariantCulture);


            product.IsVisible = node.ChildNodes.Item(10).InnerText.Trim() == "1" ? true : false;

            return product;
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    throw new NotImplementedException();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}

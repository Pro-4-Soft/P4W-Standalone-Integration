//using Microsoft.EntityFrameworkCore;
//using Pro4Soft.BackgroundWorker.Business.Database.Entities;
//using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
//using Pro4Soft.BackgroundWorker.Execution;
//using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

//namespace Tester;

//public class ProductToDb(ScheduleSetting settings) : BaseWorker(settings)
//{
//    public override async Task ExecuteAsync()
//    {
//        await LogAsync($"Starting Product sync from SAP to Database (Mode: {Settings.SyncMode})...");

//        try
//        {
//            // Create SAP Service Layer client
//            var sapClient = new SapServiceLayerClient(Config);

//            await LogAsync($"Connecting to SAP Service Layer at {Config.SapServiceLayerUrl}");

//            // Initialize pagination variables
//            const int pageSize = 20; // SAP Service Layer default and max page size
//            int skip = 0;
//            bool hasMoreRecords = true;
//            int totalFetched = 0;

//            // Build query with delta sync filter if in Delta mode
//            string baseQuery = "Items";
//            if (Settings.SyncMode == "Delta" && Settings.LastRun != null)
//            {
//                // Format date for SAP OData filter (ISO 8601 format)
//                var lastSyncDate = Settings.LastRun.Value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
//                baseQuery = $"Items?$filter=UpdateDate ge '{lastSyncDate}' or CreateDate ge '{lastSyncDate}'";
//                await LogAsync($"Delta sync: Fetching products modified after {lastSyncDate}");
//            }
//            else
//            {
//                await LogAsync($"Full sync: Fetching all products from SAP");
//            }

//            var allProducts = new List<Newtonsoft.Json.Linq.JToken>();

//            while (hasMoreRecords)
//            {
//                try
//                {
//                    // Use OData endpoint with pagination
//                    // SAP Service Layer returns max 20 records per request
//                    var separator = baseQuery.Contains("?") ? "&" : "?";
//                    var endpoint = $"{baseQuery}{separator}$skip={skip}&$top={pageSize}";
//                    await LogAsync($"Fetching: {endpoint}");

//                    var result = await sapClient.GetAsync<Newtonsoft.Json.Linq.JObject>(endpoint);

//                    if (result == null || !result.ContainsKey("value"))
//                    {
//                        hasMoreRecords = false;
//                        break;
//                    }

//                    var pageResults = result["value"] as Newtonsoft.Json.Linq.JArray;
//                    if (pageResults == null || pageResults.Count == 0)
//                    {
//                        hasMoreRecords = false;
//                        break;
//                    }

//                    totalFetched += pageResults.Count;
//                    await LogAsync($"Retrieved page {(skip / pageSize) + 1}: {pageResults.Count} products (Total: {totalFetched})");
//                    allProducts.AddRange(pageResults);

//                    // SAP returns exactly 20 records if more are available
//                    // Less than 20 means we've reached the end
//                    hasMoreRecords = pageResults.Count == pageSize;
//                    skip += pageResults.Count; // Use actual count in case it's less than pageSize

//                    // Add a small delay to avoid overwhelming the server
//                    if (hasMoreRecords)
//                    {
//                        await Task.Delay(200); // Slightly longer delay for SAP
//                    }
//                }
//                catch (Exception ex)
//                {
//                    await LogErrorAsync($"Error fetching page at skip={skip}: {ex.Message}");
//                    // Log more details about the error
//                    if (ex.InnerException != null)
//                    {
//                        await LogErrorAsync($"Inner exception: {ex.InnerException.Message}");
//                    }
//                    hasMoreRecords = false;
//                }
//            }

//            if (allProducts.Count == 0)
//            {
//                await LogAsync("No products found in SAP");
//                return;
//            }

//            await LogAsync($"Found total of {allProducts.Count} products in SAP");

//            // Count actual inventory items (after filtering)
//            var inventoryItemsCount = allProducts.Count(p => p["ItemType"]?.ToString() == "itItems");
//            await LogAsync($"Filtering to {inventoryItemsCount} inventory items (excluding services)");

//            // Get or create client from configuration
//            await using var context = CreateContext();

//            var clientCode = Config.ClientCode ?? "DEFAULT";

//            var defaultClient = await context.Clients
//                .FirstOrDefaultAsync(c => c.Name == clientCode);

//            if (defaultClient == null)
//            {
//                await LogAsync($"Creating client: {clientCode}...");
//                defaultClient = new Client
//                {
//                    Name = clientCode,
//                    Description = $"Client for {Config.SapCompanyDb}"
//                };
//                await context.Clients.AddAsync(defaultClient);
//                await context.SaveChangesAsync();
//            }
//            else
//            {
//                await LogAsync($"Using existing client: {defaultClient.Name}");
//            }

//            var processedCount = 0;
//            var errorCount = 0;
//            var skippedFrozenCount = 0;

//            foreach (var sapProduct in allProducts)
//            {
//                try
//                {
//                    // Filter for inventory items only (skip services)
//                    var itemType = sapProduct["ItemType"]?.ToString();
//                    if (itemType != "itItems")
//                        continue;

//                    // Skip frozen/inactive products
//                    var frozen = sapProduct["Frozen"]?.ToString();
//                    if (frozen == "tYES")
//                    {
//                        skippedFrozenCount++;
//                        continue;
//                    }

//                    var itemCode = sapProduct["ItemCode"]?.ToString();
//                    if (string.IsNullOrEmpty(itemCode))
//                        continue;

//                    // Check if product exists (without tracking to avoid issues)
//                    var existingId = await context.Products
//                        .Where(p => p.Sku == itemCode && p.ClientId == defaultClient.Id)
//                        .Select(p => p.Id)
//                        .SingleOrDefaultAsync();

//                    Product existing = null;
//                    if (existingId != Guid.Empty)
//                    {
//                        existing = await context.Products
//                            .Include(p => p.Packsizes)
//                            .Where(p => p.Id == existingId)
//                            .SingleAsync();
//                    }

//                    if (existing == null)
//                    {
//                        existing = new Product
//                        {
//                            Sku = itemCode,
//                            ClientId = defaultClient.Id,
//                            State = DownloadState.New
//                        };
//                        await context.Products.AddAsync(existing);
//                    }

//                    // Update product properties - using actual SAP B1 field names
//                    existing.Description = sapProduct["ItemName"]?.ToString() ?? "";
//                    existing.Info1 = sapProduct["ForeignName"]?.ToString() ?? "";
//                    existing.Info2 = sapProduct["BarCode"]?.ToString() ?? "";
//                    existing.Info3 = sapProduct["DefaultWarehouse"]?.ToString() ?? "";
//                    existing.Info4 = sapProduct["PurchaseUnit"]?.ToString() ?? "";
//                    existing.Info6 = sapProduct["SalesUnit"]?.ToString() ?? "";
//                    existing.Info7 = sapProduct["Valid"]?.ToString() ?? "tYES";

//                    // Map ItemsGroupCode
//                    if (int.TryParse(sapProduct["ItemsGroupCode"]?.ToString(), out var groupCode))
//                    {
//                        existing.ItemsGroupCode = groupCode;
//                    }

//                    // Map serial and lot control fields
//                    var manageSerialNumbers = sapProduct["ManageSerialNumbers"]?.ToString();
//                    existing.IsSerialControlled = manageSerialNumbers == "tYES";

//                    var manageBatchNumbers = sapProduct["ManageBatchNumbers"]?.ToString();
//                    existing.IsLotControlled = manageBatchNumbers == "tYES";

//                    existing.State = DownloadState.ReadyForDownload; // Set state for P4W sync

//                    // Handle pack sizes if applicable
//                    var itemsPerPurchase = sapProduct["PurchaseItemsPerUnit"];
//                    if (itemsPerPurchase != null && decimal.TryParse(itemsPerPurchase.ToString(), out var packQty) && packQty > 1)
//                    {
//                        existing.IsPacksizeController = true;

//                        // Check if packsize exists
//                        var packsize = existing.Packsizes.FirstOrDefault(p => p.EachCount == (int)packQty);
//                        if (packsize == null)
//                        {
//                            packsize = new Packsize
//                            {
//                                EachCount = (int)packQty,
//                                Name = sapProduct["PurchaseUnit"]?.ToString() ?? $"PACK{packQty}"
//                            };
//                            existing.Packsizes.Add(packsize);
//                        }
//                    }

//                    processedCount++;

//                    if (processedCount % 100 == 0)
//                    {
//                        await LogAsync($"Processed {processedCount} products...");
//                        try
//                        {
//                            await context.SaveChangesAsync();
//                        }
//                        catch (Exception saveEx)
//                        {
//                            await LogErrorAsync($"Error saving batch at {processedCount}: {saveEx.Message}");
//                            if (saveEx.InnerException != null)
//                            {
//                                await LogErrorAsync($"Save inner exception: {saveEx.InnerException.Message}");
//                            }
//                            // Continue processing but don't save this batch
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    errorCount++;
//                    var itemCode = sapProduct["ItemCode"]?.ToString() ?? "Unknown";
//                    await LogErrorAsync($"Error processing product {itemCode}: {ex.Message}");
//                    if (ex.InnerException != null)
//                    {
//                        await LogErrorAsync($"Inner exception: {ex.InnerException.Message}");
//                        if (ex.InnerException.InnerException != null)
//                        {
//                            await LogErrorAsync($"Inner-inner exception: {ex.InnerException.InnerException.Message}");
//                        }
//                    }
//                }
//            }

//            // Save any remaining changes
//            try
//            {
//                await context.SaveChangesAsync();
//            }
//            catch (Exception finalSaveEx)
//            {
//                await LogErrorAsync($"Error in final save: {finalSaveEx.Message}");
//                if (finalSaveEx.InnerException != null)
//                {
//                    await LogErrorAsync($"Final save inner exception: {finalSaveEx.InnerException.Message}");
//                }
//            }

//            await LogAsync($"Product sync completed ({Settings.SyncMode} mode). Processed: {processedCount}, Skipped (frozen): {skippedFrozenCount}, Errors: {errorCount}");
//        }
//        catch (Exception ex)
//        {
//            await LogErrorAsync($"Fatal error in ProductToDb: {ex}");
//        }
//    }
//}
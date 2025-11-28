using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.SAP;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using System.Dynamic;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;

public class InventoryFromDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        var now = DateTime.Now;
        foreach (var company in Config.Companies)
        {
            try
            {
                var sapService = SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);

                var packsizeGroups = await sapService.Get<UnitOfMeasurementGroup>("UnitOfMeasurementGroups");
                var uoms = await sapService.Get<UnitOfMeasureSap>("UnitOfMeasurements");

                var context = await company.CreateContext(Config.SqlConnection);
                var products = await context.Products.Where(c => c.IsInventoryItem).Include(c => c.Inventory).ThenInclude(c => c.Details).ToListAsync();

                const int batchSize = 5000;
                for (var start = 0; start < products.Count; start += batchSize)
                {
                    foreach (var wh in company.SyncWarehouses)
                    {
                        dynamic countDoc = new ExpandoObject();
                        countDoc.CountDate = now.Date.ToString("yyyy-MM-dd");
                        countDoc.Remarks = $"P4W Inventory sync [{wh}]";
                        countDoc.InventoryCountingLines = new List<ExpandoObject>();
                        var productsToSync = products.Skip(start).Take(batchSize).ToList();
                        foreach (var product in productsToSync)
                        {
                            var prodInv = product.Inventory.SingleOrDefault(c => c.WarehouseCode == wh);

                            if (prodInv == null)
                            {
                                dynamic line = new ExpandoObject();
                                countDoc.InventoryCountingLines.Add(line);

                                line.ItemCode = product.Sku;
                                line.WarehouseCode = wh;
                                line.CountedQuantity = 0;
                            }
                            else
                            {
                                dynamic line = new ExpandoObject();
                                countDoc.InventoryCountingLines.Add(line);

                                line.ItemCode = prodInv.Product.Sku;
                                line.Counted = "tYES";
                                line.WarehouseCode = prodInv.WarehouseCode;
                                line.CountedQuantity = prodInv.Quantity;

                                if (product.IsPacksizeControlled)
                                {
                                    var group = packsizeGroups.SingleOrDefault(c => c.Code == product.Reference1);
                                    var altUom = group.UoMGroupDefinitionCollection.Single(c => c.BaseQuantity == 1);
                                    var uom = uoms.SingleOrDefault(c => c.AbsEntry == altUom.AlternateUoM);
                                    line.UoMCode = uom.Code;
                                    //line.CountedQuantity = prodInv.Details.Sum(c => c.Quantity * (c.PacksizeEachCount ?? 1));
                                }

                                if (prodInv.Product.IsLotControlled)
                                    line.InventoryCountingBatchNumbers = prodInv.Details.GroupBy(c => c.LotNumber).Select(detl =>
                                        new
                                        {
                                            BatchNumber = detl.Key,
                                            Quantity = detl.Sum(c => c.Quantity * (c.PacksizeEachCount ?? 1)),
                                        });

                                if (prodInv.Product.IsSerialControlled)
                                    line.InventoryCountingSerialNumbers = prodInv.Details.Select(detl =>
                                        new
                                        {
                                            InternalSerialNumber = detl.SerialNumber
                                        });
                            }
                        }

                        var resp = await sapService.Post<DocumentCountingSap>("InventoryCountings", (object)countDoc);
                        await LogAsync($"Inventory count document [{resp.DocumentNumber}] created");
                    }
                }
            }
            catch (Exception e)
            {
                await LogErrorAsync(e);
            }
        }
    }
}
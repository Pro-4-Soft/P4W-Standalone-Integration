using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromP4W;

public class InventoryFromP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        var added = 0;
        var changed = 0;
        var removed = 0;
        var warehouses = await P4WClient.GetInvokeAsync<List<WarehouseP4>>("/warehouses");
        foreach (var company in Config.Companies)
        {
            var clientId = await GetClientId(company) ?? throw new BusinessWebException($"Client id does not exist");

            foreach (var wh in company.Warehouses.Select(c => warehouses.SingleOrDefault(c1 => c1.Code == c)).Where(c => c != null))
            {
                var inventoryP4 = await P4WClient.GetInvokeAsync<List<ProductInventoryP4>>($"/inventory?warehouseId={wh.Id}&clientId={clientId}");

                foreach (var invItem in inventoryP4)
                {
                    try
                    {
                        var context = await company.CreateContext(Config.SqlConnection);

                        var existingInv = await context.ProductInventory
                            .Include(c => c.Product)
                            .Include(c => c.Details)
                            .Where(c => c.Product.P4WId == invItem.ProductId)
                            .Where(c => c.WarehouseCode == wh.Code)
                            .SingleOrDefaultAsync();
                        Product product = null;
                        if (existingInv == null)
                        {
                            product = await context.Products.SingleOrDefaultAsync(c => c.P4WId == invItem.ProductId)
                                              ?? throw new BusinessWebException($"Product [{invItem.Sku}] with P4WId [{invItem.ProductId}] does not exist in Database");
                            existingInv = new()
                            {
                                ProductId = product.Id,
                                WarehouseCode = wh.Code,
                                Quantity = invItem.Quantity
                            };
                            await context.ProductInventory.AddAsync(existingInv);
                            added++;
                        }

                        product ??= existingInv.Product;

                        if (existingInv.Quantity != invItem.Quantity)
                            changed++;

                        existingInv.Quantity = invItem.Quantity;

                        if (product.IsLotControlled || product.IsSerialControlled || product.IsPacksizeControlled || product.IsExpiryControlled)
                        {
                            var remainingDetls = existingInv.Details.ToList();
                            
                            foreach (var detl in invItem.Details)
                            {
                                var existingDetl = remainingDetls.SingleOrDefault(c =>
                                    c.PacksizeEachCount == detl.PacksizeEachCount &&
                                    c.LotNumber == detl.LotNumber &&
                                    c.SerialNumber == detl.SerialNumber &&
                                    c.ExpiryDate == detl.ExpiryDate);
                                if (existingDetl == null)
                                {
                                    existingDetl = new()
                                    {
                                        ProductInventoryId = existingInv.Id,
                                        ExpiryDate = detl.ExpiryDate,
                                        SerialNumber = detl.SerialNumber,
                                        LotNumber = detl.LotNumber,
                                        PacksizeEachCount = detl.PacksizeEachCount,
                                    };
                                    context.ProductInventoryDetails.Add(existingDetl);
                                }
                                else
                                    remainingDetls.Remove(existingDetl);

                                existingDetl.Quantity = detl.Quantity;
                            }

                            if (remainingDetls.Count > 0)
                            {
                                removed += remainingDetls.Count;
                                context.ProductInventoryDetails.RemoveRange(remainingDetls);
                            }
                        }

                        await context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        await LogErrorAsync(e);
                    }
                }
            }
        }

        if (added > 0 || changed > 0 || removed > 0)
            await LogAsync($"Added [{added}], Changed [{changed}], Removed [{removed}] inventory records");

        await new InventoryFromDb(Settings).ExecuteAsync();
    }
}
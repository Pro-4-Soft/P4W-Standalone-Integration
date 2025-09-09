using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using System.Dynamic;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;

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
                var context = await company.CreateContext(Config.SqlConnection);
                var inventory = await context.ProductInventory.Include(c => c.Details).Include(c => c.Product).ToListAsync();

                dynamic countDoc = new ExpandoObject();
                countDoc.CountDate = now.Date.ToString("yyyy-MM-dd");
                countDoc.Remarks = "P4W Inventory sync";
                countDoc.InventoryCountingLines = new List<ExpandoObject>();

                foreach (var inv in inventory)
                {
                    dynamic line = new ExpandoObject();
                    countDoc.InventoryCountingLines.Add(line);

                    line.ItemCode = inv.Product.Sku;
                    line.WarehouseCode = inv.WarehouseCode;
                    line.CountedQuantity = inv.Quantity;

                    if (inv.Product.IsLotControlled)
                        line.BatchNumbers = inv.Details.Select(detl =>
                            new
                            {
                                BatchNumber = detl.LotNumber,
                                inv.Quantity,
                            });


                    if (inv.Product.IsSerialControlled)
                        line.SerialNumbers = inv.Details.Select(detl =>
                            new
                            {
                                InternalSerialNumber = detl.SerialNumber
                            });
                    
                    inv.State = DownloadState.Uploaded;
                }

                var sapPo = await SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync)
                    .Post("InventoryCountings", countDoc);
            }
            catch (Exception e)
            {
                await LogErrorAsync(e);
            }
        }
    }
}
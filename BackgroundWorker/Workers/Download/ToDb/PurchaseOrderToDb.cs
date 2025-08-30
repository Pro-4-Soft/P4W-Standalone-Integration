using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

public class PurchaseOrderToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        await using var context = CreateContext();

        var defaultClient = await context.Clients.FirstOrDefaultAsync();
        if (defaultClient == null)
            return;

        var defaultWh = await context.Warehouses.FirstOrDefaultAsync();
        if (defaultWh == null)
            return;

        var now = DateTime.Now;

        var ticks = DateTime.Now.Ticks;
        var rnd = new Random((int)ticks);

        var poCount = rnd.Next(5);

        var vendors = await context.Vendors.Where(c => c.P4WId != null).ToListAsync();
        var products = await context.Products
            .Where(c => c.IsPacksizeController == false)
            .Where(c => c.P4WId != null).ToListAsync();

        for (var i = 0; i < poCount; i++)
        {
            var poLines = new List<PurchaseOrderLine>();

            var poLineCount = rnd.Next(10) + 5;

            for (var j = 0; j < poLineCount; j++)
            {
                var poLine = new PurchaseOrderLine
                {
                    ProductId = products[rnd.Next(products.Count)].Id,
                    Quantity = rnd.Next(100) + 10,
                    LineNumber = poLines.Count + 1
                };

                if (poLines.All(c => c.ProductId != poLine.ProductId))
                    poLines.Add(poLine);
            }

            var newPo = new PurchaseOrder()
            {
                PurchaseOrderNumber = $"PO-{now.ToString("yyyy-MM-dd-HH-mm-ss") + i}",

                VendorId = vendors[rnd.Next(vendors.Count)].Id,
                WarehouseId = defaultWh.Id,

                Lines = poLines,

                State = DownloadState.ReadyForDownload,
            };

            if (!newPo.Lines.Any())
                continue;

            await context.PurchaseOrders.AddAsync(newPo);
            await context.SaveChangesAsync();

            await LogAsync($"PO [{newPo.PurchaseOrderNumber}] saved to db");
        }
    }
}
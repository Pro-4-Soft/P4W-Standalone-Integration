using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToP4W;

public class PurchaseOrderToP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            await using var context = await company.CreateContext(Config.SqlConnection);

            var pos = await context.PurchaseOrders
                .Where(c => c.Lines.Count > 0 && c.Lines.All(c1 => c1.Product.P4WId != null))
                .Where(c => c.State == DownloadState.ReadyForDownload)
                .Include(c => c.Lines).ThenInclude(c => c.Product)
                .ToListAsync();

            foreach (var po in pos)
            {
                var payload = new PurchaseOrderP4()
                {
                    Id = po.P4WId,
                    VendorId = po.Vendor.P4WId ?? throw new BusinessWebException($"Vendor [{po.Vendor.Code}] has not been synced"),
                    //WarehouseId = po.WarehouseId,
                    PurchaseOrderNumber = po.PurchaseOrderNumber,
                    Comments = po.Comments,
                    Lines = po.Lines.Select(c => new PurchaseOrderLineP4()
                    {
                        ProductId = c.Product.P4WId ?? throw new BusinessWebException($"Product [{c.Product.Sku}] has not been synced"),
                        LineNumber = c.LineNumber,
                        OrderedQuantity = c.Quantity,
                        Reference1 = c.Id.ToString()
                    }).ToList()
                };

                try
                {
                    PurchaseOrderP4 p4Prod;
                    if (payload.Id != null)
                        p4Prod = await P4WClient.PutInvokeAsync<PurchaseOrderP4>("/purchase-orders", payload);
                    else
                        p4Prod = await P4WClient.PostInvokeAsync<PurchaseOrderP4>("/purchase-orders", payload);

                    po.P4WId = p4Prod.Id;
                    po.State = DownloadState.Downloaded;

                    await LogAsync($"PO [{po.PurchaseOrderNumber}] sent to P4W");
                }
                catch (Exception e)
                {
                    po.DownloadError = e.ToString();
                    po.State = DownloadState.Failed;

                    await LogAsync($"PO [{po.PurchaseOrderNumber}] failed to be sent to P4W\n{e}");
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
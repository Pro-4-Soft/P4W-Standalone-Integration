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
        var warehouses = await P4WClient.GetInvokeAsync<List<WarehouseP4>>("/warehouses");
        foreach (var company in Config.Companies)
        {
            try
            {
                await using var context = await company.CreateContext(Config.SqlConnection);

                var pos = await context.PurchaseOrders
                    .Where(c => c.Lines.Count > 0 && c.Lines.All(c1 => c1.Product.P4WId != null))
                    .Where(c => c.State == DownloadState.ReadyForDownload)
                    .Include(c => c.Vendor)
                    .Include(c => c.Lines).ThenInclude(c => c.Product)
                    .ToListAsync();

                foreach (var po in pos)
                {
                    var count = 0;
                    var payload = new PurchaseOrderP4
                    {
                        //Id = po.P4WId,
                        VendorId = po.Vendor.P4WId ?? throw new BusinessWebException($"Vendor [{po.Vendor.Code}] has not been synced"),
                        WarehouseId = warehouses.SingleOrDefault(c => c.Code == po.WarehouseCode)?.Id ?? throw new BusinessWebException($"Warehouse [{po.WarehouseCode}] is not setup in P4W"),
                        PurchaseOrderNumber = po.PurchaseOrderNumber,
                        Comments = po.Comments,
                        Lines = po.Lines.OrderBy(c => c.LineNumber).Select(c => new PurchaseOrderLineP4()
                        {
                            LineNumber = ++count,
                            ProductId = c.Product.P4WId ?? throw new BusinessWebException($"Product [{c.Product.Sku}] has not been synced"),
                            OrderedQuantity = c.NumberOfPacks* c.Packsize?? c.Quantity,
                            NumberOfPacks = c.NumberOfPacks,
                            Packsize = c.Packsize,
                            Reference1 = c.Id.ToString()
                        }).ToList()
                    };

                    try
                    {
                        var existing = await P4WClient.GetInvokeAsync<List<PurchaseOrderP4>>($"/purchase-orders/PurchaseOrderNumber/{payload.PurchaseOrderNumber}");
                        if (existing.Count > 0)
                            payload.Id = existing.First().Id;

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
                        po.ErrorMessage = e.ToString();
                        po.State = DownloadState.DownloadFailed;

                        await LogAsync($"PO [{po.PurchaseOrderNumber}] failed to be sent to P4W\n{e}");
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                await LogErrorAsync(e);
            }
        }
    }
}
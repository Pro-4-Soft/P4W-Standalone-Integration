using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToP4W;

public class CustomerReturnsToP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        List<WarehouseP4> warehouses = null;
        foreach (var company in Config.Companies)
        {
            try
            {
                await using var context = await company.CreateContext(Config.SqlConnection);

                var pos = await context.CustomerReturns
                    .Where(c => c.Lines.Count > 0)
                    .Where(c => c.State == DownloadState.ReadyForDownload)
                    .Include(c => c.Lines).ThenInclude(c => c.Product)
                    .ToListAsync();
                if (pos.Count == 0)
                    continue;

                warehouses ??= await P4WClient.GetInvokeAsync<List<WarehouseP4>>("/warehouses");

                foreach (var po in pos)
                {
                    if (po.IsmanualCancelledClosed)
                    {
                        if (po.P4WId != null)
                        {
                            await P4WClient.WebInvokeAsync($"/customer-returns/{po.P4WId}", HttpMethod.Delete);
                            await LogAsync($"CustomerReturnNumber [{po.CustomerReturnNumber}] delete from P4W");
                        }

                        po.State = DownloadState.Downloaded;
                        await context.SaveChangesAsync();
                        continue;
                    }

                    var count = 0;
                    var payload = new CustomerReturnP4()
                    {
                        // Id = po.P4WId,
                        CustomerId = po.CustomerId,
                        WarehouseId = warehouses.SingleOrDefault(c => c.Code == po.WarehouseCode)?.Id ?? throw new BusinessWebException($"Warehouse [{po.WarehouseCode}] is not setup in P4W"),
                        CustomerReturnNumber = po.CustomerReturnNumber,
                        Comments = po.Comments,
                        Lines = po.Lines
                            .Where(c => (c.Packsize * c.Packsize ?? c.Quantity) > 0)
                            .Where(c => c.Product.IsInventoryItem)
                            .OrderBy(c => c.LineNumber).Select(c => new CustomerReturnLineP4()
                            {
                                LineNumber = ++count,
                                ProductId = c.Product.P4WId ?? throw new BusinessWebException($"Product [{c.Product.Sku}] has not been synced"),
                                OrderedQuantity = c.NumberOfPacks * c.Packsize ?? c.Quantity,
                                NumberOfPacks = c.NumberOfPacks,
                                Packsize = c.Packsize,
                                Reference1 = c.Id.ToString()
                            }).ToList()
                    };

                    if (payload.Lines.Count == 0)
                    {
                        po.State = DownloadState.External;
                        await context.SaveChangesAsync();
                        await LogAsync($"PO [{po.CustomerReturnNumber}] not downloaded. No lines on an order");
                        continue;
                    }

                    try
                    {
                        var existing = await P4WClient.GetInvokeAsync<List<CustomerReturnP4>>($"/customer-returns/CustomerReturnNumber/{payload.CustomerReturnNumber}");
                        if (existing.Count > 0)
                            payload.Id = existing.First().Id;

                        CustomerReturnP4 p4Po;
                        if (payload.Id != null)
                            p4Po = await P4WClient.PutInvokeAsync<CustomerReturnP4>("/customer-returns", payload);
                        else
                            p4Po = await P4WClient.PostInvokeAsync<CustomerReturnP4>("/customer-returns", payload);

                        po.P4WId = p4Po.Id;
                        po.State = DownloadState.Downloaded;

                        await LogAsync($"PO [{po.CustomerReturnNumber}] sent to P4W");
                    }
                    catch (Exception e)
                    {
                        po.ErrorMessage = e.ToString();
                        po.State = DownloadState.DownloadFailed;

                        await LogAsync($"PO [{po.CustomerReturnNumber}] failed to be sent to P4W\n{e}");
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
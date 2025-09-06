using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities.Base;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;
using Pro4Soft.P4E.Common.Utilities;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromP4W;

public class PurchaseOrdersFromP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        var posForUpload = await P4WClient.GetInvokeAsync<List<PurchaseOrderP4>>("purchase-orders");
        foreach (var poHeader in posForUpload)
        {
            var company = Config.Companies.SingleOrDefault(c => c.P4WClientName == poHeader.Vendor.Client.Name);
            if (company == null)
                continue;//Ignore orders for clients that we don't know about

            var p4WPo = await P4WClient.GetInvokeAsync<PurchaseOrderP4>($"purchase-orders/{poHeader.Id}");

            PurchaseOrder po = null;
            var context = await company.CreateContext(Config.SqlConnection);
            try
            {
                po = await context.PurchaseOrders
                    .Include(c => c.Lines).ThenInclude(c => c.Product)
                    .Include(c => c.Lines).ThenInclude(c => c.Details)
                    .SingleOrDefaultAsync(c => c.P4WId == p4WPo.Id) 
                     ?? throw new BusinessWebException($"PO [{p4WPo.PurchaseOrderNumber}] with P4WId [{p4WPo.Id}] does not exist in Database");

                foreach (var line in p4WPo.Lines)
                {
                    var dbLine = po.Lines.SingleOrDefault(c => c.Id == line.Reference1.ParseGuid()) ??
                                 throw new BusinessWebException($"Line [{line.LineNumber}] with Sku [{line.Product.Sku}] does not existing on original PO. Line Id [{line.Reference1}] not found");

                    dbLine.ReceivedQuantity = line.ReceivedQuantity;

                    if (dbLine.Product.IsLotControlled || dbLine.Product.IsSerialControlled || dbLine.Product.IsPacksizeControlled || dbLine.Product.IsExpiryControlled)
                    {
                        foreach (var detl in line.Details)
                        {
                            var existing = dbLine.Details.SingleOrDefault(c =>
                                c.PacksizeEachCount == detl.PacksizeEachCount &&
                                c.LotNumber == detl.LotNumber &&
                                c.SerialNumber == detl.SerialNumber &&
                                c.ExpiryDate == detl.ExpiryDate);
                            if (existing == null)
                            {
                                existing = new()
                                {
                                    PurchaseOrderLineId = dbLine.Id,
                                    ExpiryDate = detl.ExpiryDate,
                                    SerialNumber = detl.SerialNumber,
                                    LotNumber = detl.LotNumber,
                                    PacksizeEachCount = detl.PacksizeEachCount,
                                    ReceivedQuantity = detl.ReceivedQuantity,
                                };
                                context.PurchaseOrderLineDetails.Add(existing);
                            }

                            existing.ReceivedQuantity = detl.ReceivedQuantity;
                        }
                    }
                }

                if (po.Lines.Any(c => c.ReceivedQuantity > 0))
                    po.State = DownloadState.ReadyForUpload;

                await context.SaveChangesAsync();
                await P4WClient.PostInvokeAsync("/purchase-orders/upload", new UploadConfirmationP4
                {
                    Ids = [poHeader.Id.Value],
                    UploadSucceeded = true,
                });

                await LogAsync($"PO [{po.PurchaseOrderNumber}] uploaded from P4W");
            }
            catch (Exception e)
            {
                if (po != null)
                {
                    po.State = DownloadState.UploadFailed;
                    po.ErrorMessage = e.ToString();
                    await context.SaveChangesAsync();
                }

                await P4WClient.PostInvokeAsync("/purchase-orders/upload", new UploadConfirmationP4
                {
                    Ids = [poHeader.Id.Value],
                    UploadSucceeded = false,
                    UploadMessage = e.Message,
                    ResetUploadCount = false
                });
                await LogErrorAsync(e);
            }
        }

        await new PurchaseOrdersFromDb(Settings).ExecuteAsync();
    }
}
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities.Base;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromP4W;

public class CustomerReturnsFromP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        var rmasForUpload = await P4WClient.GetInvokeAsync<List<CustomerReturnP4>>("customer-returns");
        foreach (var rmaHeader in rmasForUpload)
        {
            var company = Config.Companies.SingleOrDefault(c => c.P4WClientName == rmaHeader.Customer.Client.Name);
            if (company == null)
                continue;//Ignore orders for clients that we don't know about

            var p4WRma = await P4WClient.GetInvokeAsync<CustomerReturnP4>($"customer-returns/{rmaHeader.Id}");

            CustomerReturn rma = null;
            var context = await company.CreateContext(Config.SqlConnection);
            try
            {
                rma = await context.CustomerReturns
                    .Include(c => c.Lines).ThenInclude(c => c.Product)
                    .Include(c => c.Lines).ThenInclude(c => c.Details)
                    .SingleOrDefaultAsync(c => c.P4WId == p4WRma.Id) 
                     ?? throw new BusinessWebException($"RMA [{p4WRma.CustomerReturnNumber}] with P4WId [{p4WRma.Id}] does not exist in Database");

                foreach (var line in p4WRma.Lines)
                {
                    var dbLine = rma.Lines.SingleOrDefault(c => c.Id == line.Reference1.ParseGuid()) ??
                                 throw new BusinessWebException($"Line [{line.LineNumber}] with Sku [{line.Product.Sku}] does not existing on original RMA. Line Id [{line.Reference1}] not found");

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
                                    CustomerReturnLineId = dbLine.Id,
                                    ExpiryDate = detl.ExpiryDate,
                                    SerialNumber = detl.SerialNumber,
                                    LotNumber = detl.LotNumber,
                                    PacksizeEachCount = detl.PacksizeEachCount,
                                    ReceivedQuantity = detl.ReceivedQuantity,
                                };
                                context.CustomerReturnLineDetails.Add(existing);
                            }

                            existing.ReceivedQuantity = detl.ReceivedQuantity;
                        }
                    }
                }

                if (rma.Lines.Any(c => c.ReceivedQuantity > 0))
                    rma.State = DownloadState.ReadyForUpload;

                await context.SaveChangesAsync();
                await P4WClient.PostInvokeAsync("/customer-returns/upload", new UploadConfirmationP4
                {
                    Ids = [rmaHeader.Id.Value],
                    UploadSucceeded = true,
                });

                await LogAsync($"RMA [{rma.CustomerReturnNumber}] uploaded from P4W");
            }
            catch (Exception e)
            {
                if (rma != null)
                {
                    rma.State = DownloadState.UploadFailed;
                    rma.ErrorMessage = e.ToString();
                    await context.SaveChangesAsync();
                }

                await P4WClient.PostInvokeAsync("/customer-returns/upload", new UploadConfirmationP4
                {
                    Ids = [rmaHeader.Id.Value],
                    UploadSucceeded = false,
                    UploadMessage = e.Message,
                    ResetUploadCount = false
                });
                await LogErrorAsync(e);
            }
        }

        await new CustomerReturnsFromDb(Settings).ExecuteAsync();
    }
}
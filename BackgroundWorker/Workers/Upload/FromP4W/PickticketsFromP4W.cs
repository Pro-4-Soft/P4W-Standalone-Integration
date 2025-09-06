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

public class PickticketsFromP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        var pickTicketsForUpload = await P4WClient.GetInvokeAsync<List<PickTicketP4>>("/pick-tickets");
        foreach (var pckTicketHeader in pickTicketsForUpload)
        {
            var company = Config.Companies.SingleOrDefault(c => c.P4WClientName == pckTicketHeader.Customer.Client.Name);
            if (company == null)
                continue;//Ignore orders for clients that we don't know about
            
            var pickTicketP4W = await P4WClient.GetInvokeAsync<PickTicketP4>($"pick-tickets/{pckTicketHeader.Id}");

            Pickticket pickTicketDb = null;
            var context = await company.CreateContext(Config.SqlConnection);
            try
            {
                pickTicketDb = await context.PickTickets
                    .Include(c => c.Lines).ThenInclude(c => c.Product)
                    .Include(c => c.Totes).ThenInclude(c => c.Lines).ThenInclude(c => c.Details)
                    .SingleOrDefaultAsync(c => c.P4WId == pickTicketP4W.Id) 
                               ?? throw new BusinessWebException($"Pickticket [{pickTicketP4W.PickTicketNumber}] with P4WId [{pickTicketP4W.Id}] does not exist in Database");

                foreach (var toteP4W in pickTicketP4W.Totes)
                {
                    var existingTote = pickTicketDb.Totes.SingleOrDefault(c => c.P4WId == toteP4W.Id);
                    if (existingTote == null)
                    {
                        existingTote = new()
                        {
                            P4WId = toteP4W.Id.Value,
                            PickTicketId = pickTicketDb.Id,
                        };
                        await context.Totes.AddAsync(existingTote);
                        await context.SaveChangesAsync();
                    }
                    existingTote.Sscc18Code = toteP4W.Sscc18Code;
                    existingTote.TrackTraceNumber = toteP4W.TrackTraceNumber;
                    existingTote.BolNumber = toteP4W.BolNumber;
                    existingTote.Carrier = toteP4W.Carrier;
                    existingTote.CartonNumber = toteP4W.CartonNumber;
                    existingTote.ShippingCost = toteP4W.ShippingCost;
                    existingTote.Carrier = toteP4W.Carrier;
                    existingTote.ShippingService = toteP4W.ShippingService;

                    foreach (var toteLineP4W in toteP4W.Lines)
                    {
                        var pickTicketLineP4W = pickTicketP4W.Lines.Single(c => c.Id == toteLineP4W.PickTicketLineId);
                        var pickTicketLineDb = pickTicketDb.Lines.SingleOrDefault(c => c.Id == pickTicketLineP4W.Reference1.ParseGuid()) ??
                                     throw new BusinessWebException($"Line [{pickTicketLineP4W.LineNumber}] with Sku [{toteLineP4W.Product.Sku}] could does not existing on original PO. Line Id [{pickTicketLineP4W.Reference1}] not found");

                        var existingToteLine = existingTote.Lines.SingleOrDefault(c => c.P4WId == toteLineP4W.Id && c.PickTicketLineId == pickTicketLineDb.Id);
                        if (existingToteLine == null)
                        {
                            existingToteLine = new()
                            {
                                P4WId = toteLineP4W.Id.Value,
                                ToteId = existingTote.Id,
                                PickTicketLineId = pickTicketLineDb.Id
                            };
                            await context.ToteLines.AddAsync(existingToteLine);
                            await context.SaveChangesAsync();
                        }

                        existingToteLine.ShippedQuantity = toteLineP4W.ShippedQuantity;

                        if (pickTicketLineDb.Product.IsLotControlled ||
                            pickTicketLineDb.Product.IsSerialControlled ||
                            pickTicketLineDb.Product.IsPacksizeControlled ||
                            pickTicketLineDb.Product.IsExpiryControlled)
                        {
                            foreach (var detl in toteLineP4W.Details)
                            {
                                var existing = existingToteLine.Details.SingleOrDefault(c =>
                                    c.PacksizeEachCount == detl.PacksizeEachCount &&
                                    c.LotNumber == detl.LotNumber &&
                                    c.SerialNumber == detl.SerialNumber &&
                                    c.ExpiryDate == detl.ExpiryDate);
                                if (existing == null)
                                {
                                    existing = new()
                                    {
                                        ToteLineId = existingToteLine.Id,
                                        ExpiryDate = detl.ExpiryDate,
                                        SerialNumber = detl.SerialNumber,
                                        LotNumber = detl.LotNumber,
                                        PacksizeEachCount = detl.PacksizeEachCount,
                                    };
                                    await context.ToteLineDetails.AddAsync(existing);
                                    await context.SaveChangesAsync();
                                }
                                existing.ShippedQuantity = detl.ShippedQuantity;
                            }
                        }
                    }
                }

                if (pickTicketDb.Totes.SelectMany(c => c.Lines).Sum(c => c.ShippedQuantity) > 0)
                    pickTicketDb.State = DownloadState.ReadyForUpload;

                await context.SaveChangesAsync();
                await P4WClient.PostInvokeAsync("/pick-tickets/upload", new UploadConfirmationP4()
                {
                    Ids = [pckTicketHeader.Id.Value],
                    UploadSucceeded = true,
                });

                await LogAsync($"Pickticket [{pickTicketDb.PickTicketNumber}] uploaded from P4W");
            }
            catch (Exception e)
            {
                if (pickTicketDb != null)
                {
                    pickTicketDb.State = DownloadState.UploadFailed;
                    pickTicketDb.ErrorMessage = e.ToString();
                    await context.SaveChangesAsync();
                }

                await P4WClient.PostInvokeAsync("/pick-tickets/upload", new UploadConfirmationP4()
                {
                    Ids = [pckTicketHeader.Id.Value],
                    UploadSucceeded = false,
                    UploadMessage = e.Message,
                    ResetUploadCount = false
                });
                await LogErrorAsync(e);
            }
        }

        await new PickticketsFromDb(Settings).ExecuteAsync();
    }
}
using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Business.SAP;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.P4E.Common.Utilities;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;

public class PurchaseOrderFromDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            try
            {
                var context = await company.CreateContext(Config.SqlConnection);
                var sapService = new SapServiceClient(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);

                var pos = await context.PurchaseOrders
                    .Include(c => c.Vendor)
                    .Include(c => c.Lines).ThenInclude(c => c.Details)
                    .Include(c => c.Lines).ThenInclude(c => c.Product)
                    .Where(c => c.State == DownloadState.ReadyForUpload)
                    .ToListAsync();

                foreach (var po in pos)
                {
                    try
                    {
                        dynamic delivery = new ExpandoObject();
                        delivery.CardCode = po.Vendor.Code;
                        delivery.DocumentLines = new List<ExpandoObject>();

                        foreach (var poLine in po.Lines)
                        {
                            dynamic line = new ExpandoObject();

                            line.BaseEntry = po.Reference1.ParseInt();
                            line.BaseLine = poLine.LineNumber;
                            line.BaseType = (int)BoObjectTypes.oPurchaseOrders;
                            line.Quantity = poLine.ReceivedQuantity;

                            if (poLine.Product.IsSerialControlled)
                            {
                                var count = 0;
                                line.SerialNumbers = poLine.Details.Where(c => !c.SerialNumber.IsNullOrEmpty()).Select(c => new
                                {
                                    //BaseLineNumber = count++,
                                    InternalSerialNumber = c.SerialNumber
                                });
                            }

                            if (poLine.Product.IsLotControlled)
                            {
                                var count = 0;
                                line.BatchNumbers = poLine.Details.Where(c => !c.LotNumber.IsNullOrEmpty())
                                    .GroupBy(c => c.LotNumber)
                                    .Select(lot => new
                                    {
                                        //BaseLineNumber = count++,
                                        BatchNumber = lot.Key,
                                        Quantity = lot.Sum(c => (c.PacksizeEachCount ?? 1) * c.ReceivedQuantity)
                                    });
                            }

                            delivery.DocumentLines.Add(line);
                        }

                        await sapService.Post("PurchaseDeliveryNotes", delivery);

                        po.State = DownloadState.Uploaded;
                        po.ErrorMessage = null;
                    }
                    catch (Exception e)
                    {
                        po.State = DownloadState.UploadFailed;
                        po.ErrorMessage = e.Message;
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
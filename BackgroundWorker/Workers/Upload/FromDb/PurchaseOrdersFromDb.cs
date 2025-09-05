using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Business.SAP;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers.Download.ToDb;
using Pro4Soft.P4E.Common.Utilities;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;

public class PurchaseOrdersFromDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            try
            {
                var context = await company.CreateContext(Config.SqlConnection);
                var sapService = SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);

                var now = DateTime.Now;

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
                        delivery.DocDate = now.Date.ToString("yyyy-MM-dd");
                        delivery.TaxDate = now.Date.ToString("yyyy-MM-dd");
                        delivery.DocDueDate = now.Date.ToString("yyyy-MM-dd");
                        delivery.DocumentLines = new List<ExpandoObject>();

                        foreach (var poLine in po.Lines)
                        {
                            dynamic line = new ExpandoObject();

                            line.BaseEntry = po.Reference1.ParseInt();
                            line.BaseLine = poLine.LineNumber;
                            line.BaseType = (int)BoObjectTypes.oPurchaseOrders;
                            line.Quantity = poLine.ReceivedQuantity / (poLine.Packsize??1);
                            
                            if (poLine.Product.IsSerialControlled)
                            {
                                line.SerialNumbers = poLine.Details.Where(c => !c.SerialNumber.IsNullOrEmpty()).Select(c => new
                                {
                                    InternalSerialNumber = c.SerialNumber
                                });
                            }

                            if (poLine.Product.IsLotControlled)
                            {
                                line.BatchNumbers = poLine.Details.Where(c => !c.LotNumber.IsNullOrEmpty())
                                    .GroupBy(c => c.LotNumber)
                                    .Select(lot => new
                                    {
                                        BatchNumber = lot.Key,
                                        Quantity = lot.Sum(c => (c.PacksizeEachCount ?? 1) * c.ReceivedQuantity),
                                    });
                            }

                            delivery.DocumentLines.Add(line);
                        }

                        var goodsReceiptPo = await sapService.Post<BaseDocumentSap>("PurchaseDeliveryNotes", (object)delivery, LogAsync);

                        po.Uploaded = true;
                        po.State = DownloadState.Uploaded;
                        po.ErrorMessage = null;
                        
                        await context.SaveChangesAsync();

                        await LogAsync($"PO [{po.PurchaseOrderNumber}] written to SAP as Goods Receipt PO [{goodsReceiptPo.DocNum}]");

                        //Download a backorder
                        var sapPo = await sapService.Get<PurchaseOrderSap>("PurchaseOrders",
                            new(ConditionType.And, [
                                new FilterRule("DocEntry", Operator.Eq, po.Reference1),
                                new FilterRule("DocumentStatus", Operator.Eq, "'bost_Open'")
                            ]));
                        await new PurchaseOrdersToDb(Settings).DownloadPos(company, sapPo);
                    }
                    catch (Exception e)
                    {
                        po.State = DownloadState.UploadFailed;
                        po.ErrorMessage = e.Message;
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception e)
            {
                await LogErrorAsync(e);
            }
        }
    }
}
using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Business.SAP;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers.Download.ToDb;
using Pro4Soft.P4E.Common.Utilities;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;

public class PickticketsFromDb(ScheduleSetting settings) : BaseWorker(settings)
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

                var pickTickets = await context.PickTickets
                    .Include(c => c.Customer)
                    .Include(c => c.Lines).ThenInclude(c => c.Product)
                    .Include(c => c.Totes).ThenInclude(c => c.Lines).ThenInclude(c => c.Details)
                    .Where(c => c.State == DownloadState.ReadyForUpload)
                    .ToListAsync();

                foreach (var pickTicket in pickTickets)
                {
                    try
                    {
                        dynamic delivery = new ExpandoObject();
                        delivery.CardCode = pickTicket.Customer.Code;
                        delivery.DocDate = now.Date.ToString("yyyy-MM-dd");
                        delivery.TaxDate = now.Date.ToString("yyyy-MM-dd");
                        delivery.DocDueDate = now.Date.ToString("yyyy-MM-dd");
                        delivery.DocumentLines = new List<ExpandoObject>();

                        foreach (var pickTicketLine in pickTicket.Lines)
                        {
                            dynamic line = new ExpandoObject();

                            line.BaseEntry = pickTicket.Reference1.ParseInt();
                            line.BaseLine = pickTicketLine.LineNumber;
                            line.BaseType = (int)BoObjectTypes.oOrders;

                            var shipped = pickTicketLine.ToteLines.Sum(c => c.ShippedQuantity);

                            line.Quantity = shipped / (pickTicketLine.Packsize??1);

                            var detls = pickTicketLine.ToteLines.SelectMany(c => c.Details).ToList();

                            if (pickTicketLine.Product.IsSerialControlled)
                            {
                                line.SerialNumbers = detls.Where(c => !c.SerialNumber.IsNullOrEmpty()).Select(c => new
                                {
                                    InternalSerialNumber = c.SerialNumber
                                });
                            }

                            if (pickTicketLine.Product.IsLotControlled)
                            {
                                line.BatchNumbers = detls.Where(c => !c.LotNumber.IsNullOrEmpty())
                                    .GroupBy(c => c.LotNumber)
                                    .Select(lot => new
                                    {
                                        BatchNumber = lot.Key,
                                        Quantity = lot.Sum(c => (c.PacksizeEachCount ?? 1) * c.ShippedQuantity),
                                    });
                            }

                            delivery.DocumentLines.Add(line);
                        }

                        var goodsReceiptPo = await sapService.Post<BaseDocumentSap>("DeliveryNotes", (object)delivery, LogAsync);

                        pickTicket.Uploaded = true;
                        pickTicket.State = DownloadState.Uploaded;
                        pickTicket.ErrorMessage = null;
                        
                        await context.SaveChangesAsync();

                        await LogAsync($"Pickticket [{pickTicket.PickTicketNumber}] written to SAP as Delivery note [{goodsReceiptPo.DocNum}]");

                        //Download a backorder
                        var sapPo = await sapService.Get<SalesOrderSap>("Orders",
                            new(ConditionType.And, [
                                new FilterRule("DocEntry", Operator.Eq, pickTicket.Reference1),
                                new FilterRule("DocumentStatus", Operator.Eq, "'bost_Open'")
                            ]));
                        await new PickticketsToDb(Settings).DownloadSos(company, sapPo);
                    }
                    catch (Exception e)
                    {
                        pickTicket.State = DownloadState.UploadFailed;
                        pickTicket.ErrorMessage = e.Message;
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
using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Dto.SAP;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;

public class CustomerReturnsFromDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            try
            {
                var context = await company.CreateContext(Config.SqlConnection);
                var now = DateTime.Now;

                var rmas = await context.CustomerReturns
                    .Include(c => c.Customer)
                    .Include(c => c.Lines)
                    .ThenInclude(c => c.Details)
                    .Include(c => c.Lines).ThenInclude(c => c.Product)
                    .Where(c => c.State == DownloadState.ReadyForUpload)
                    .ToListAsync();

                foreach (var rma in rmas)
                {
                    try
                    {
                        dynamic returnDoc = new ExpandoObject();
                        returnDoc.CardCode = rma.Customer.Code;
                        returnDoc.DocDate = now.Date.ToString("yyyy-MM-dd");
                        returnDoc.TaxDate = now.Date.ToString("yyyy-MM-dd");
                        returnDoc.DocDueDate = now.Date.ToString("yyyy-MM-dd");
                        returnDoc.DocumentLines = new List<ExpandoObject>();

                        foreach (var rmaLine in rma.Lines)
                        {
                            dynamic line = new ExpandoObject();

                            //TODO: Figure out how to properly link it to base document

                            //line.BaseEntry = rma.Reference1.ParseInt();
                            //line.BaseLine = rmaLine.LineNumber;
                            //line.BaseType = (int)BoObjectTypes.oReturnRequest;

                            line.ItemCode = rmaLine.Product.Sku;
                            line.Quantity = rmaLine.ReceivedQuantity / (rmaLine.Packsize??1);
                            
                            if (rmaLine.Product.IsSerialControlled)
                            {
                                line.SerialNumbers = rmaLine.Details.Where(c => !c.SerialNumber.IsNullOrEmpty()).Select(c => new
                                {
                                    InternalSerialNumber = c.SerialNumber
                                });
                            }

                            if (rmaLine.Product.IsLotControlled)
                            {
                                line.BatchNumbers = rmaLine.Details.Where(c => !c.LotNumber.IsNullOrEmpty())
                                    .GroupBy(c => c.LotNumber)
                                    .Select(lot => new
                                    {
                                        BatchNumber = lot.Key,
                                        Quantity = lot.Sum(c => (c.PacksizeEachCount ?? 1) * c.ReceivedQuantity),
                                    });
                            }

                            returnDoc.DocumentLines.Add(line);
                        }

                        var sapService = SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);
                        var ret = await sapService.Post<BaseDocumentSap>("Returns", returnDoc);

                        rma.Uploaded = true;
                        rma.State = DownloadState.Uploaded;
                        rma.ErrorMessage = null;
                        
                        await context.SaveChangesAsync();

                        await LogAsync($"RMA [{rma.CustomerReturnNumber}] written to SAP as Return [{ret.DocNum}]");

                        //TODO: Download a backorder - enable only when RMA created has a proper base document
                        //var sapRma = await sapService.Get<CustomerReturnSap>("ReturnRequest",
                        //    new(ConditionType.And, [
                        //        new FilterRule("DocEntry", Operator.Eq, rma.Reference1),
                        //        new FilterRule("DocumentStatus", Operator.Eq, "'bost_Open'")
                        //    ]));
                        //await new CustomerReturnsToDb(Settings).Download(company, sapRma);
                    }
                    catch (Exception e)
                    {
                        rma.State = DownloadState.UploadFailed;
                        rma.ErrorMessage = e.Message;
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
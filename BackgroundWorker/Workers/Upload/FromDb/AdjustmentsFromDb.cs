using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities;
using Pro4Soft.BackgroundWorker.Business.SAP;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using System.Dynamic;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;

public class AdjustmentsFromDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            try
            {
                await PostAdjustment(AuditType.ProductAdd, "InventoryGenEntries", company);
                await PostAdjustment(AuditType.ProductRemove, "InventoryGenExits", company);
            }
            catch (Exception e)
            {
                await LogErrorAsync(e);
            }
        }
    }

    private async Task PostAdjustment(string type, string endpoint, CompanySettings company)
    {
        var context = await company.CreateContext(Config.SqlConnection);
        var now = DateTime.Now;
        var adjusts = await context.Adjustments
            .Include(c => c.Product)
            .Where(c => c.State == DownloadState.ReadyForUpload)
            .Where(c => c.Type == type)
            .ToListAsync();
        if (adjusts.Count == 0)
            return;

        try
        {
            dynamic adjDoc = new ExpandoObject();
            adjDoc.DocDate = now.Date.ToString("yyyy-MM-dd");
            adjDoc.TaxDate = now.Date.ToString("yyyy-MM-dd");
            adjDoc.DocumentLines = new List<ExpandoObject>();

            foreach (var adj in adjusts)
            {
                dynamic line = new ExpandoObject();
                adjDoc.DocumentLines.Add(line);

                line.ItemCode = adj.Product.Sku;
                line.WarehouseCode = adj.Type == AuditType.ProductAdd? adj.ToWarehouse: adj.FromWarehouse;
                line.Quantity = adj.Quantity;

                if (adj.Product.IsLotControlled)
                {
                    line.BatchNumbers = new List<dynamic>
                    {
                        new
                        {
                            BatchNumber = adj.LotNumber,
                            adj.Quantity,
                        }
                    };
                }

                if (adj.Product.IsSerialControlled)
                {
                    line.SerialNumbers = new List<dynamic>
                    {
                        new
                        {
                            InternalSerialNumber = adj.SerialNumber
                        }
                    };
                }
                adj.State = DownloadState.Uploaded;
            }

            await SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync)
                .Post<BaseDocumentSap>(endpoint, (object)adjDoc);

            await LogAsync($"{(type == AuditType.ProductAdd?"Positive":"Negative")} adjustment with [{adjusts.Count}] lines written to SAP");
        }
        catch (Exception e)
        {
            foreach (var adj in adjusts)
            {
                adj.State = DownloadState.UploadFailed;
                adj.ErrorMessage = e.Message;
            }

            await LogErrorAsync(e);
        }
        finally
        {
            await context.SaveChangesAsync();
        }
    }
}
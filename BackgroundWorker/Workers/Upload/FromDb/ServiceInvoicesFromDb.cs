using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities;
using Pro4Soft.BackgroundWorker.Dto.SAP;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;

public class ServiceInvoicesFromDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            try
            {
                var context = await company.CreateContext(Config.SqlConnection);

                var invoices = await context.ServiceInvoices
                    .Include(c => c.Lines)
                    .Where(c => c.State == DownloadState.ReadyForUpload || c.State == DownloadState.UploadFailed)
                    .ToListAsync();

                if (invoices.Count == 0)
                    continue;

                var sapService = SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);

                foreach (var inv in invoices)
                {
                    try
                    {
                        // Clear previous error before retry attempt
                        inv.ErrorMessage = null;
                        await context.SaveChangesAsync();

                        dynamic doc = new ExpandoObject();

                        doc.DocDate = inv.PostingDate?.ToString("yyyy-MM-dd");
                        doc.DocDueDate = inv.PostingDate?.ToString("yyyy-MM-dd");
                        doc.TaxDate = inv.PostingDate?.ToString("yyyy-MM-dd");

                        // Use stored CustomerCode as CardCode in SAP
                        doc.CardCode = inv.CustomerCode;
                        doc.NumAtCard = inv.InvoiceNumber;
                        doc.Comments = $"P4W Invoice {inv.InvoiceNumber}";

                        doc.DocumentLines = new List<ExpandoObject>();

                        if (inv.Lines != null && inv.Lines.Count > 0)
                        {
                            foreach (var ln in inv.Lines.OrderBy(c => c.Id))
                            {
                                dynamic line = new ExpandoObject();
                                if (!string.IsNullOrWhiteSpace(ln.Item))
                                    line.ItemCode = ln.Item;
                                else
                                    line.ItemDescription = ln.Description;

                                line.Quantity = ln.Quantity;
                                line.UnitPrice = ln.UnitPrice;
                                //line.TaxCode = "S1"; // adjust if needed
                                //line.AccountCode = "";

                                doc.DocumentLines.Add(line);
                            }
                        }
                        else
                        {
                            dynamic line = new ExpandoObject();
                            line.ItemDescription = $"P4W Invoice {inv.InvoiceNumber}";
                            line.Quantity = 1;
                            line.UnitPrice = inv.Total ?? 0m;
                            //line.TaxCode = "S1";
                            //line.AccountCode = "";
                            doc.DocumentLines.Add(line);
                        }

                        var resp = await sapService.Post<BaseDocumentSap>("Invoices", (object)doc);

                        inv.Uploaded = true;
                        inv.State = DownloadState.Uploaded;
                        inv.ErrorMessage = null;

                        await context.SaveChangesAsync();

                        await LogAsync($"Invoice [{inv.InvoiceNumber}] written to SAP as Invoice [{resp.DocNum}]");
                    }
                    catch (Exception e)
                    {
                        inv.State = DownloadState.UploadFailed;
                        inv.ErrorMessage = e.Message;
                        await context.SaveChangesAsync();

                        await LogErrorAsync(e);
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

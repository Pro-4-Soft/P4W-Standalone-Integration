using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities;
using Pro4Soft.BackgroundWorker.Dto.SAP;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Dto.Database;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;

public class ServiceInvoicesFromDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        try
        {
            // Use master DB where invoices were imported
            await using var masterContext = new DatabaseContext(Config.SqlConnection);

            var invoices = await masterContext.ServiceInvoices
                .Include(c => c.Lines)
                .Where(c => c.State == DownloadState.ReadyForUpload || c.State == DownloadState.UploadFailed)
                .ToListAsync();

            if (invoices.Count == 0)
            {
                await LogAsync("No service invoices to upload");
                return;
            }

            await LogAsync($"Preparing to upload {invoices.Count} invoices");

            foreach (var inv in invoices)
            {
                try
                {
                    await LogAsync($"Processing invoice {inv.InvoiceNumber} (ClientId={inv.ClientId}, CustomerCode={inv.CustomerCode})");

                    // Find matching company settings by CustomerCode or CompanyName only. SAP uses CustomerCode.
                    var mappedCompany = Config.Companies.FirstOrDefault(c =>
                        (!string.IsNullOrWhiteSpace(c.CustomerCode) && string.Equals(c.CustomerCode, inv.CustomerCode, StringComparison.OrdinalIgnoreCase))
                        || (!string.IsNullOrWhiteSpace(c.CompanyName) && string.Equals(c.CompanyName, inv.CustomerCode, StringComparison.OrdinalIgnoreCase))
                    );

                    CompanySettings targetCompany = mappedCompany;

                    if (mappedCompany == null)
                    {
                        // No explicit mapping found — fall back to first configured company if available
                        targetCompany = Config.Companies.FirstOrDefault();
                        if (targetCompany == null)
                        {
                            inv.State = DownloadState.UploadFailed;
                            inv.ErrorMessage = $"No company configured to upload invoice '{inv.InvoiceNumber}' (CustomerCode={inv.CustomerCode})";
                            await masterContext.SaveChangesAsync();
                            await LogErrorAsync(inv.ErrorMessage);
                            continue;
                        }

                        await LogAsync($"No mapping found for invoice {inv.InvoiceNumber}; falling back to default company {targetCompany.CompanyName ?? targetCompany.SapCompanyDb}");
                    }
                    else
                    {
                        await LogAsync($"Mapped invoice {inv.InvoiceNumber} to company {mappedCompany.CompanyName} (SapCompanyDb={mappedCompany.SapCompanyDb})");
                    }

                    var sapService = SapServiceClient.GetInstance(targetCompany.SapUrl, targetCompany.SapCompanyDb, targetCompany.SapUsername, targetCompany.SapPassword, LogAsync, LogErrorAsync);

                    // Clear previous error before retry attempt
                    inv.ErrorMessage = null;

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

                            doc.DocumentLines.Add(line);
                        }
                    }
                    else
                    {
                        dynamic line = new ExpandoObject();
                        line.ItemDescription = $"P4W Invoice {inv.InvoiceNumber}";
                        line.Quantity = 1;
                        line.UnitPrice = inv.Total ?? 0m;
                        doc.DocumentLines.Add(line);
                    }

                    // Log payload for debugging
                    try
                    {
                        var payloadJson = Utils.SerializeToStringJson(doc, Newtonsoft.Json.Formatting.Indented, false);
                        await LogAsync($"SAP payload for invoice {inv.InvoiceNumber}: {payloadJson}");
                    }
                    catch { }

                    try
                    {
                        var resp = await sapService.Post<BaseDocumentSap>("Invoices", (object)doc);

                        inv.Uploaded = true;
                        inv.State = DownloadState.Uploaded;
                        inv.ErrorMessage = null;

                        await masterContext.SaveChangesAsync();

                        await LogAsync($"Invoice [{inv.InvoiceNumber}] written to SAP as Invoice [{resp?.DocNum}] using company {targetCompany.CompanyName}");
                    }
                    catch (BusinessWebException bwe)
                    {
                        // SAP client returned a business error; capture and log details
                        inv.State = DownloadState.UploadFailed;
                        inv.ErrorMessage = bwe.Message;
                        await masterContext.SaveChangesAsync();

                        await LogErrorAsync(new Exception($"SAP upload failed for invoice {inv.InvoiceNumber}: {bwe.Message}", bwe));
                    }
                }
                catch (Exception e)
                {
                    inv.State = DownloadState.UploadFailed;
                    inv.ErrorMessage = e.Message;
                    await masterContext.SaveChangesAsync();

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

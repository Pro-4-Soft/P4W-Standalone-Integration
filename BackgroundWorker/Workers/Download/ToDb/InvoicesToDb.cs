using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

public class InvoicesToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            try
            {
                var summaries = await P4WClient.GetInvokeAsync<List<ClientInvoiceP4>>("client-invoices");
                if (summaries == null || summaries.Count == 0)
                    continue;

                var clientId = Guid.Parse(company.ClientId);
                var companySummaries = summaries.Where(s => s.ClientId == clientId).ToList();
                if (companySummaries.Count == 0)
                    continue;

                var context = await company.CreateContext(Config.SqlConnection);

                foreach (var summary in companySummaries)
                {
                    try
                    {
                        var existing = await context.ServiceInvoices.SingleOrDefaultAsync(c => c.InvoiceNumber == summary.InvoiceNumber && c.ClientId == clientId);
                        if (existing != null)
                            continue;

                        // Try to fetch detailed invoice (lines) using correct endpoint
                        ClientInvoiceP4 detail = null;
                        try
                        {
                            detail = await P4WClient.GetInvokeAsync<ClientInvoiceP4>($"client-invoices/{summary.Id}");
                        }
                        catch (Exception ex)
                        {
                            await LogErrorAsync($"Failed to fetch invoice details for {summary.InvoiceNumber}: {ex.Message}");
                            // fallback to summary
                            detail = null;
                        }

                        // prefer detailed source if available
                        var src = detail ?? summary;

                        var customerCode = src?.Client?.Name ?? src?.Name ?? summary.InvoiceNumber;

                        var entity = new ServiceInvoice
                        {
                            InvoiceNumber = src.InvoiceNumber,
                            ClientId = clientId,
                            // capture the client name from P4W; this will be used as SAP CardCode
                            CustomerCode = customerCode,
                            StartPeriod = src.StartPeriod,
                            EndPeriod = src.EndPeriod,
                            PostingDate = src.PostingDate,
                            SubTotal = src.SubTotal,
                            Total = src.Total,
                            State = DownloadState.ReadyForUpload
                        };

                        if (src?.Lines != null)
                        {
                            foreach (var ln in src.Lines)
                            {
                                var unitPrice = ln.Rate;
                                var lineTotal = unitPrice * ln.Quantity;

                                entity.Lines.Add(new ServiceInvoiceLine
                                {
                                    Item = ln.Item,
                                    Description = ln.Description,
                                    Quantity = ln.Quantity,
                                    UnitPrice = unitPrice,
                                    Total = lineTotal
                                });
                            }
                        }

                        await context.ServiceInvoices.AddAsync(entity);
                        await context.SaveChangesAsync();

                        await LogAsync($"Invoice [{entity.InvoiceNumber}] written to DB (CustomerCode={entity.CustomerCode})");
                    }
                    catch (Exception ex)
                    {
                        await LogErrorAsync(ex);
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

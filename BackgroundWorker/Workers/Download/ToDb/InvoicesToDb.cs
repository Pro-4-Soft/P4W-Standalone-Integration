using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Dto.Database;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

public class InvoicesToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        try
        {
            var summaries = await P4WClient.GetInvokeAsync<List<ClientInvoiceP4>>("client-invoices");
            if (summaries == null || summaries.Count == 0)
                return;

            // Use master DB context so we can store invoices for any client returned by P4W
            await using var context = new DatabaseContext(Config.SqlConnection);

            foreach (var summary in summaries)
            {
                try
                {
                    var clientId = summary.ClientId;

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

                    // P4W provided client name
                    var p4wClientName = src?.Client?.Name ?? src?.Name ?? summary.InvoiceNumber;

                    // Try to map to configured company to obtain SAP CustomerCode
                    var mappedCompany = Config.Companies.FirstOrDefault(c =>
                        (!string.IsNullOrWhiteSpace(c.ClientId) && Guid.TryParse(c.ClientId, out var gid) && gid == clientId)
                        || (!string.IsNullOrWhiteSpace(c.CustomerCode) && string.Equals(c.CustomerCode, p4wClientName, StringComparison.OrdinalIgnoreCase))
                        || (!string.IsNullOrWhiteSpace(c.CompanyName) && string.Equals(c.CompanyName, p4wClientName, StringComparison.OrdinalIgnoreCase))
                    );

                    // Use SAP customer code from settings when available, otherwise use P4W client name
                    var customerCode = mappedCompany != null && !string.IsNullOrWhiteSpace(mappedCompany.CustomerCode)
                        ? mappedCompany.CustomerCode
                        : p4wClientName;

                    var entity = new ServiceInvoice
                    {
                        InvoiceNumber = src.InvoiceNumber,
                        ClientId = clientId,
                        // store the SAP CardCode (CustomerCode) when mapping exists, otherwise P4W client name
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

                    await LogAsync($"Invoice [{entity.InvoiceNumber}] written to master DB (ClientId={entity.ClientId}, CustomerCode={entity.CustomerCode})");
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

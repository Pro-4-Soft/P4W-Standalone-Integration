using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToP4W;

public class CustomersToP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            await using var context = await company.CreateContext(Config.SqlConnection);

            var customers = await context.Customers
                .Where(c => c.State == DownloadState.ReadyForDownload)
                .ToListAsync();

            foreach (var cust in customers)
            {
                var payload = new CustomerP4
                {
                    ClientId = cust.ClientId,

                    Code = cust.Code,
                    Description = cust.Description,
                    CompanyName = cust.CompanyName,
                };

                try
                {
                    var existing = await P4WClient.GetInvokeAsync<List<CustomerP4>>($"/customers?customerCode={payload.Code}&clientId={payload.ClientId}");
                    if (existing.Count > 0)
                        payload.Id = existing.First().Id;

                    CustomerP4 p4Prod;
                    if (payload.Id != null)
                        p4Prod = await P4WClient.PutInvokeAsync<CustomerP4>("/customers", payload);
                    else
                        p4Prod = await P4WClient.PostInvokeAsync<CustomerP4>("/customers", payload);

                    cust.P4WId = p4Prod.Id;
                    cust.State = DownloadState.Downloaded;

                    await LogAsync($"Customer [{cust.Code}] sent to P4W");
                }
                catch (Exception e)
                {
                    cust.ErrorMessage = e.ToString();
                    cust.State = DownloadState.DownloadFailed;

                    await LogAsync($"Customer [{cust.Code}] failed to be sent to P4W\n{e}");
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
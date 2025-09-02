using System.Net.WebSockets;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

public class CustomerToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            try
            {
                await using var context = await company.CreateContext(Config.SqlConnection);

                var clients = await P4WClient.GetInvokeAsync<List<ClientP4>>($"clients?clientName={company.P4WClientName}");
                if (clients.Count == 0)
                {
                    await LogErrorAsync($"Client [{company.P4WClientName}] does not exist in P4W");
                    continue;
                }

                var client = clients.First();

                foreach (var cust in _sampleCustomers)
                {
                    var existing = await context.Customers
                        .Where(c => c.Code == cust.Code)
                        .SingleOrDefaultAsync();

                    if (existing == null)
                    {
                        existing = new()
                        {
                            Code = cust.Code,
                            ClientId = client.Id ?? throw new BusinessWebException($"Client id does not exist"),
                        };
                        await context.Customers.AddAsync(existing);
                    }

                    existing.Description = $"{cust.Description} - {Guid.NewGuid()}";

                    existing.State = DownloadState.ReadyForDownload;
                    existing.DownloadError = null;

                    await context.SaveChangesAsync();

                    await LogAsync($"Customer [{existing.Code}] saved to db");
                }
            }
            catch (Exception e)
            {
                await LogErrorAsync(e);
            }
        }
    }

    private readonly List<Customer> _sampleCustomers =
    [
        new()
        {
            CompanyName = "C1",
            Code = "C1",
            Description = "Sample customer 1",
        },
        new()
        {
            CompanyName = "C2",
            Code = "C2",
            Description = "Sample customer 2",
        },
    ];
}
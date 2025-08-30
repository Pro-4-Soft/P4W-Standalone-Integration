using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

public class CustomerToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        await using var context = CreateContext();

        var defaultClient = await context.Clients.FirstOrDefaultAsync();
        if (defaultClient == null)
            return;

        foreach (var cust in _sampleCustomers)
        {
            var existing = await context.Customers
                .Where(c => c.Code == cust.Code && c.ClientId == defaultClient.Id)
                .SingleOrDefaultAsync();

            if (existing == null)
            {
                existing = new()
                {
                    Code = cust.Code,
                    ClientId = defaultClient.Id
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
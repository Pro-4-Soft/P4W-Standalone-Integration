using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

public class VendorToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        await using var context = CreateContext();

        var defaultClient = await context.Clients.FirstOrDefaultAsync();
        if (defaultClient == null)
            return;

        foreach (var vend in _sampleVendors)
        {
            var existing = await context.Vendors
                .Where(c => c.Code == vend.Code && c.ClientId == defaultClient.Id)
                .SingleOrDefaultAsync();

            if (existing == null)
            {
                existing = new()
                {
                    Code = vend.Code,
                    ClientId = defaultClient.Id
                };
                await context.Vendors.AddAsync(existing);
            }

            existing.Description = $"{vend.Description} - {Guid.NewGuid()}";

            existing.State = DownloadState.ReadyForDownload;
            existing.DownloadError = null;

            await context.SaveChangesAsync();

            await LogAsync($"Vendor [{existing.Code}] saved to db");
        }
    }

    private readonly List<Vendor> _sampleVendors =
    [
        new()
        {
            Code = "V1",
            CompanyName = "V1",
            Description = "Sample vendor 1",
        },
        new()
        {
            Code = "V2",
            CompanyName = "V2",
            Description = "Sample vendor 2",
        },
    ];
}
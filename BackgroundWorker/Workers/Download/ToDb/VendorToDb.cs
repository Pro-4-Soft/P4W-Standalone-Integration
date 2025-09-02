using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

public class VendorToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            await using var context = await company.CreateContext(Config.SqlConnection);
            var client = await P4WClient.GetInvokeAsync<ClientP4>($"clients?clientName={company.P4WClientName}");
            if (client == null)
            {
                await LogErrorAsync($"Client [{company.P4WClientName}] does not exist in P4W");
                continue;
            }

            foreach (var vend in _sampleVendors)
            {
                var existing = await context.Vendors
                    .Where(c => c.Code == vend.Code && c.ClientId == client.Id)
                    .SingleOrDefaultAsync();

                if (existing == null)
                {
                    existing = new()
                    {
                        Code = vend.Code,
                        ClientId = client.Id ?? throw new BusinessWebException($"Client id does not exist"),
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
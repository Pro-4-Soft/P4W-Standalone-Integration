using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToP4W;

public class VendorsToP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            await using var context = await company.CreateContext(Config.SqlConnection);

            var vendors = await context.Vendors
                .Where(c => c.State == DownloadState.ReadyForDownload)
                .ToListAsync();

            foreach (var vend in vendors)
            {
                var payload = new VendorP4
                {
                    ClientId = vend.ClientId,
                    Code = vend.Code,
                    CompanyName = vend.CompanyName
                };

                try
                {
                    var existing = await P4WClient.GetInvokeAsync<List<VendorP4>>($"/vendors?code={payload.Code}&clientId={payload.ClientId}");
                    if (existing.Count > 0)
                        payload.Id = existing.First().Id;

                    VendorP4 p4Prod;
                    if (payload.Id != null)
                        p4Prod = await P4WClient.PutInvokeAsync<VendorP4>("/vendors", payload);
                    else
                        p4Prod = await P4WClient.PostInvokeAsync<VendorP4>("/vendors", payload);

                    vend.P4WId = p4Prod.Id;
                    vend.State = DownloadState.Downloaded;

                    await LogAsync($"Vendor [{vend.Code}] sent to P4W");
                }
                catch (Exception e)
                {
                    vend.ErrorMessage = e.ToString();
                    vend.State = DownloadState.DownloadFailed;

                    await LogAsync($"Vendor [{vend.Code}] failed to be sent to P4W\n{e}");
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
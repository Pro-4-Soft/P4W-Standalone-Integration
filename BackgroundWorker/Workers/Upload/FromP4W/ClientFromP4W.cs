using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Execution;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromP4W;

public class ClientFromP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        var clients = P4WClient.GetInvoke<List<ClientP4>>("/clients");

        await using var context = CreateContext();

        foreach (var p4Client in clients)
        {
            var existing = await context.Clients.SingleOrDefaultAsync(c => c.P4WId == p4Client.Id);
            if (existing == null)
            {
                existing = new()
                {
                    P4WId = p4Client.Id,
                };
                await context.Clients.AddAsync(existing);
            }

            existing.Name = p4Client.Name;
            existing.Description = p4Client.Description;
            existing.SsccCompanyId = p4Client.SsccCompanyId;

            await context.SaveChangesAsync();
        }
    }
}
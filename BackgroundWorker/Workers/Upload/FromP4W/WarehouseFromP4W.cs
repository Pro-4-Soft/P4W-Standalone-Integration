using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Execution;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromP4W;

public class WarehouseFromP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        var warehouses = P4WClient.GetInvoke<List<WarehouseP4>>("/warehouses");

        await using var context = CreateContext();

        foreach (var p4Wh in warehouses)
        {
            var existing = await context.Warehouses.SingleOrDefaultAsync(c => c.P4WId == p4Wh.Id);
            if (existing == null)
            {
                existing = new()
                {
                    P4WId = p4Wh.Id,
                };
                await context.Warehouses.AddAsync(existing);
            }

            existing.Code = p4Wh.Code;

            await context.SaveChangesAsync();
        }
    }
}
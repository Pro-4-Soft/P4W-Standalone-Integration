using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToP4W;

public class ProductToP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        await using var context = CreateContext();

        var products = await context.Products
            .Where(c => c.Client.P4WId != null)
            .Where(c => c.State == DownloadState.ReadyForDownload)
            .Include(c => c.Packsizes)
            .Include(c => c.Client)
            .ToListAsync();

        foreach (var prod in products)
        {
            var payload = new ProductP4
            {
                ClientId = prod.Client?.P4WId,

                Sku = prod.Sku,
                Description = prod.Description,

                IsPacksizeController = prod.IsPacksizeController
            };

            if (payload.IsPacksizeController)
            {
                payload.Packsizes = prod.Packsizes.Select(c => new PacksizeP4()
                {
                    Name = c.Name,
                    EachCount = c.EachCount
                }).ToList();
            }

            try
            {
                var existing = await P4WClient.GetInvokeAsync<List<ProductP4>>($"/products?sku={payload.Sku}&clientId={payload.ClientId}");
                if (existing.Count > 0)
                    payload.Id = existing.First().Id;

                ProductP4 p4Prod;
                if (payload.Id != null)
                    p4Prod = await P4WClient.PutInvokeAsync<ProductP4>("/products", payload);
                else
                    p4Prod = await P4WClient.PostInvokeAsync<ProductP4>("/products", payload);

                prod.P4WId = p4Prod.Id;
                prod.State = DownloadState.Downloaded;

                await LogAsync($"Product [{prod.Sku}] sent to P4W");
            }
            catch (Exception e)
            {
                prod.DownloadError = e.ToString();
                prod.State = DownloadState.Failed;

                await LogAsync($"Product [{prod.Sku}] failed to be sent to P4W\n{e}");
            }

            await context.SaveChangesAsync();
        }
    }
}
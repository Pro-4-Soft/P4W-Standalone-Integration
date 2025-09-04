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
        foreach (var company in Config.Companies)
        {
            await using var context = await company.CreateContext(Config.SqlConnection);

            var products = await context.Products
                .Where(c => c.State == DownloadState.ReadyForDownload)
                .Include(c => c.Packsizes)
                .ToListAsync();

            foreach (var prod in products)
            {
                if (!prod.IsInventoryItem)
                {
                    prod.State = DownloadState.External;
                    await context.SaveChangesAsync();
                    continue;
                }

                var payload = new ProductP4
                {
                    ClientId = prod.ClientId,

                    Sku = prod.Sku,
                    Description = prod.Description,
                    Category = prod.Category,
                    Upc = prod.Upc,
                    Length = prod.Length,
                    Width = prod.Width,
                    Height = prod.Height,
                    Weight = prod.Weight,
                    IsSerialControlled = prod.IsSerialControlled,
                    IsLotControlled = prod.IsLotControlled,

                    IsPacksizeController = prod.IsPacksizeControlled
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
                    prod.ErrorMessage = e.ToString();
                    prod.State = DownloadState.DownloadFailed;

                    await LogAsync($"Product [{prod.Sku}] failed to be sent to P4W\n{e}");
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
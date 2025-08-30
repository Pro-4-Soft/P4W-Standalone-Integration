using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

public class ProductToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        await using var context = CreateContext();

        var defaultClient = await context.Clients.FirstOrDefaultAsync();
        if (defaultClient == null)
            return;

        foreach (var prod in _sampleProds)
        {
            var existing = await context.Products
                .Include(c => c.Packsizes)
                .Where(c => c.Sku == prod.Sku && c.ClientId == defaultClient.Id)
                .SingleOrDefaultAsync();

            if (existing == null)
            {
                existing = new()
                {
                    Sku = prod.Sku,
                    ClientId = defaultClient.Id
                };
                await context.Products.AddAsync(existing);
            }

            existing.IsPacksizeController = prod.IsPacksizeController;
            if (existing.IsPacksizeController)
            {
                foreach (var pack in prod.Packsizes)
                {
                    var existingPack = existing.Packsizes.SingleOrDefault(c => c.EachCount == pack.EachCount);
                    if (existingPack == null)
                    {
                        existingPack = new()
                        {
                            EachCount = pack.EachCount
                        };
                        existing.Packsizes.Add(existingPack);
                    }

                    existingPack.Name = pack.Name;
                }
            }

            existing.Description = $"{prod.Description} - {Guid.NewGuid()}";

            existing.State = DownloadState.ReadyForDownload;
            existing.DownloadError = null;

            await context.SaveChangesAsync();

            await LogAsync($"Product [{existing.Sku}] saved to db");
        }
    }

    private readonly List<Product> _sampleProds =
    [
        new()
        {
            Sku = "prod1",
            Description = "Regular prod1",
        },
        new()
        {
            Sku = "prod2",
            Description = "Regular prod2",
        },
        new()
        {
            Sku = "prod3",
            Description = "Regular prod2",
        },
        new()
        {
            Sku = "prod4",
            Description = "Regular prod2",
        },
        new()
        {
            Sku = "prod5",
            Description = "Regular prod2",
        },
        new()
        {
            Sku = "prod6",
            Description = "Regular prod2",
        },
        new()
        {
            Sku = "prod7",
            Description = "Regular prod2",
        },
        new()
        {
            Sku = "prod8",
            Description = "Regular prod2",
        },
        new()
        {
            Sku = "prod9",
            Description = "Regular prod2",
        },
        new()
        {
            Sku = "prod10",
            Description = "Regular prod2",
        },
        new()
        {
            Sku = "prod11",
            Description = "Regular prod2",
        },
        new()
        {
            Sku = "prod12",
            Description = "Regular prod2",
        },
        new()
        {
            Sku = "prod13",
            Description = "Regular prod2",
        },
        new()
        {
            Sku = "pack1",
            Description = "Packsized prod 1",
            IsPacksizeController = true,
            Packsizes =
            [
                new()
                {
                    Name = "x6",
                    EachCount = 6
                },
                new()
                {
                    Name = "x24",
                    EachCount = 24
                }
            ]
        }
    ];
}
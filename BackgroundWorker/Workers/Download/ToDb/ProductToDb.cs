using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.P4E.Common.Utilities;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

public class ProductToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            var masterContext = await company.CreateContext(Config.SqlConnection, true);

            //Reset config
            //await masterContext.SetStringConfig(ConfigConstants.Download_Product_LastSync, null);

            var clients = await P4WClient.GetInvokeAsync<List<ClientP4>>($"clients?clientName={company.P4WClientName}");
            if (clients.Count == 0)
            {
                await LogErrorAsync($"Client [{company.P4WClientName}] does not exist in P4W");
                continue;
            }
            var client = clients.First();

            var sapService = new SapServiceClient(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);
            var lastRead = await masterContext.GetStringAsync(ConfigConstants.Download_Product_LastSync);
            var products = await sapService.GetProducts(lastRead?.ParseDateTimeNullable());
            if (products.Count == 0)
                continue;

            var itemGroupsCodes = await sapService.GetGroupCodes();

            await products.ExecuteInParallel(async prod =>
            {
                try
                {
                    var context = await company.CreateContext(Config.SqlConnection);
                    var existing = await context.Products
                        .Include(c => c.Packsizes)
                        .Where(c => c.Sku == prod.ItemCode && c.ClientId == client.Id)
                        .SingleOrDefaultAsync();

                    if (existing == null)
                    {
                        existing = new()
                        {
                            Sku = prod.ItemCode,
                            ClientId = client.Id ?? throw new BusinessWebException($"Client id does not exist"),
                        };
                        await context.Products.AddAsync(existing);
                    }

                    //existing.IsPacksizeController = prod.IsPacksizeController;
                    //if (existing.IsPacksizeController)
                    //{
                    //    foreach (var pack in prod.Packsizes)
                    //    {
                    //        var existingPack = existing.Packsizes.SingleOrDefault(c => c.EachCount == pack.EachCount);
                    //        if (existingPack == null)
                    //        {
                    //            existingPack = new()
                    //            {
                    //                EachCount = pack.EachCount
                    //            };
                    //            existing.Packsizes.Add(existingPack);
                    //        }

                    //        existingPack.Name = pack.Name;
                    //    }
                    //}

                    existing.Description = prod.ItemName;
                    existing.Category = itemGroupsCodes.SingleOrDefault(c => c.Number == prod.ItemsGroupCode)?.GroupName;

                    existing.Upc = prod.BarCode;

                    existing.Length = prod.PurchaseUnitLength;
                    existing.Width = prod.PurchaseUnitWidth;
                    existing.Height = prod.PurchaseUnitHeight;
                    existing.Weight = prod.PurchaseUnitWeight;

                    existing.IsSerialControlled = prod.ManageSerialNumbers == "tYes";
                    existing.IsLotControlled = prod.ManageBatchNumbers == "tYes";

                    existing.State = DownloadState.ReadyForDownload;
                    existing.DownloadError = null;

                    await context.SaveChangesAsync();

                    await LogAsync($"Product [{existing.Sku}] saved to db");
                }
                catch (Exception e)
                {
                    await LogErrorAsync(e);
                }
            }, 20);

            var maxUpdated = products.Max(c => c.ActualUpdated);
            await masterContext.SetStringConfig(ConfigConstants.Download_Product_LastSync, maxUpdated?.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"));
        }
    }
}
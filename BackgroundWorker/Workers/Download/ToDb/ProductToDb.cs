using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities;
using Pro4Soft.BackgroundWorker.Business.SAP;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers.Download.ToP4W;
using Pro4Soft.P4E.Common.Utilities;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

public class ProductToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            var masterContext = await company.CreateContext(Config.SqlConnection);

            //Reset config
            //await masterContext.SetStringConfig(ConfigConstants.Download_Product_LastSync, null);

            var sapService = SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);
            var lastRead = await masterContext.GetStringAsync(ConfigConstants.Download_Product_LastSync);
            var products = await sapService.Get<ProductSap>("Items", SapServiceClient.GetLastUpdatedRule(lastRead?.ParseDateTimeNullable()));
            if (products.Count == 0)
                continue;

            var itemGroupsCodes = await sapService.Get<ItemGroupCodeSap>("ItemGroups");
            var packsizeGroups = await sapService.Get<UnitOfMeasurementGroup>("UnitOfMeasurementGroups");

            var clients = await P4WClient.GetInvokeAsync<List<ClientP4>>($"clients?clientName={company.P4WClientName}");
            if (clients.Count == 0)
            {
                await LogErrorAsync($"Client [{company.P4WClientName}] does not exist in P4W");
                continue;
            }

            var clientId = await GetClientId(company) ?? throw new BusinessWebException($"Client id does not exist");

            foreach (var prod in products)
            {
                try
                {
                    var context = await company.CreateContext(Config.SqlConnection);
                    var existing = await context.Products
                        .Include(c => c.Packsizes)
                        .Where(c => c.Sku == prod.ItemCode && c.ClientId == clientId)
                        .SingleOrDefaultAsync();

                    if (existing == null)
                    {
                        existing = new()
                        {
                            Sku = prod.ItemCode,
                            ClientId = clientId,
                        };
                        await context.Products.AddAsync(existing);
                    }

                    existing.Description = prod.ItemName;
                    existing.Category = itemGroupsCodes.SingleOrDefault(c => c.Number == prod.ItemsGroupCode)?.GroupName;

                    existing.Upc = prod.BarCode;

                    existing.Length = prod.PurchaseUnitLength;
                    existing.Width = prod.PurchaseUnitWidth;
                    existing.Height = prod.PurchaseUnitHeight;
                    existing.Weight = prod.PurchaseUnitWeight;

                    existing.IsInventoryItem = prod.InventoryItem?.ToLower() == "tyes";
                    existing.IsSerialControlled = prod.ManageSerialNumbers?.ToLower() == "tyes";
                    existing.IsLotControlled = prod.ManageBatchNumbers?.ToLower() == "tyes";
                    existing.IsPacksizeControlled = prod.UoMGroupEntry != -1;

                    if (existing.IsPacksizeControlled)
                    {
                        var packsizeGroup = packsizeGroups.FirstOrDefault(c => c.AbsEntry == prod.UoMGroupEntry);

                        foreach (var pack in packsizeGroup.UoMGroupDefinitionCollection.Where(c=>c.BaseQuantity != 1))
                        {
                            var existingPack = existing.Packsizes.SingleOrDefault(c => c.EachCount == pack.BaseQuantity);
                            if (existingPack == null)
                            {
                                existingPack = new()
                                {
                                    EachCount = pack.BaseQuantity,
                                    ProductId = existing.Id
                                };
                                context.Packsizes.Add(existingPack);
                            }

                            existingPack.Name = $"Box x{existingPack.EachCount}";
                        }
                    }

                    existing.State = DownloadState.ReadyForDownload;
                    existing.ErrorMessage = null;

                    await context.SaveChangesAsync();

                    await LogAsync($"Product [{existing.Sku}] saved to db");
                }
                catch (Exception e)
                {
                    await LogErrorAsync(e);
                }
            }

            var maxUpdated = products.Max(c => c.ActualUpdated);
            await masterContext.SetStringConfig(ConfigConstants.Download_Product_LastSync, maxUpdated?.ToString("yyyy-MM-dd'T'HH:mm:ss"));
        }

        await new ProductToP4W(Settings).ExecuteAsync();
    }
}
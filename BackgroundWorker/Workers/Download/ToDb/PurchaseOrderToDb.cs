using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Business.SAP;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers.Download.ToP4W;
using Pro4Soft.P4E.Common.Utilities;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

public class PurchaseOrderToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        //var warehouses = await P4WClient.GetInvokeAsync<List<WarehouseP4>>("/warehouses");

        foreach (var company in Config.Companies)
        {
            var masterContext = await company.CreateContext(Config.SqlConnection);

            //reset config
            //await masterContext.SetStringConfig(ConfigConstants.Download_PurchaseOrder_LastSync, null);

            var sapService = SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);
            var lastRead = await masterContext.GetStringAsync(ConfigConstants.Download_PurchaseOrder_LastSync);
            var pos = await sapService.Get<PurchaseOrderSap>("PurchaseOrders",
                new(ConditionType.And, [
                    new FilterRule("DocumentStatus", Operator.Eq, "'bost_Open'"),
                    SapServiceClient.GetLastUpdatedRule(lastRead?.ParseDateTimeNullable())
                ]));

            if (pos.Count == 0)
                continue;

            await DownloadPos(company, pos);

            var maxUpdated = pos.Max(c => c.ActualUpdated);
            await masterContext.SetStringConfig(ConfigConstants.Download_PurchaseOrder_LastSync, maxUpdated?.ToString("yyyy-MM-dd'T'HH:mm:ss"));
        }

        await new PurchaseOrderToP4W(Settings).ExecuteAsync();
    }

    public async Task DownloadPos(CompanySettings company, List<PurchaseOrderSap> pos)
    {
        if (pos.Count == 0)
            return;

        var sapService = SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);
        var clientId = await GetClientId(company) ?? throw new BusinessWebException($"Client id does not exist");

        foreach (var po in pos)
        {
            try
            {
                var context = await company.CreateContext(Config.SqlConnection);
                var vendor = await context.Vendors
                    .Where(c => c.Code == po.CardCode && c.ClientId == clientId)
                    .SingleOrDefaultAsync();

                if (vendor == null)
                {
                    var sapVendor = await sapService.GetFirst<VendorSap>("BusinessPartners", new(ConditionType.And, [
                        new(nameof(VendorSap.CardCode), Operator.Eq, $"'{po.CardCode}'"),
                        new(nameof(VendorSap.CardType), Operator.Eq, "'cSupplier'"),
                    ]));
                    vendor = new()
                    {
                        ClientId = clientId,
                        Code = sapVendor.CardCode,
                        CompanyName = sapVendor.CardName,
                    };
                    await context.Vendors.AddAsync(vendor);
                    await context.SaveChangesAsync();

                    //Push to P4W
                    await new VendorToP4W(Settings).ExecuteAsync();
                    await context.Entry(vendor).ReloadAsync();
                }

                var whGroups = po.DocumentLines.GroupBy(c => c.WarehouseCode).Where(c => company.Warehouses.Contains(c.Key)).ToList();
                foreach (var whGroup in whGroups)
                {
                    if (whGroup.All(c => c.RemainingOpenQuantity == 0))
                        continue;

                    var poNum = $"{po.DocNum}{(whGroups.Count == 1 ? "" : $"-{whGroup.Key}")}";

                    var bo = await context.PurchaseOrders.CountAsync(c => c.Reference1 == po.DocEntry && c.WarehouseCode == whGroup.Key && c.Uploaded == true);
                    if (bo > 0)
                        poNum = $"{poNum}-{bo}";

                    var poP4W = await context.PurchaseOrders.Include(c => c.Lines).SingleOrDefaultAsync(c => c.PurchaseOrderNumber == poNum && c.Uploaded == false);
                    if (poP4W == null)
                    {
                        poP4W = new()
                        {
                            VendorId = vendor.Id,
                            PurchaseOrderNumber = poNum,
                        };
                        await context.PurchaseOrders.AddAsync(poP4W);
                    }
                    else
                    {
                        context.PurchaseOrderLines.RemoveRange(poP4W.Lines);
                        await context.SaveChangesAsync();
                        await context.Entry(poP4W).ReloadAsync();
                    }

                    poP4W.Reference1 = po.DocEntry;
                    poP4W.WarehouseCode = whGroup.Key;
                    poP4W.Comments = po.Comments;

                    foreach (var line in whGroup.Where(c => c.RemainingOpenQuantity > 0).OrderBy(c => c.LineNum))
                    {
                        var product = await context.Products
                            .Include(c => c.Packsizes)
                            .Where(c => c.Sku == line.ItemCode && c.ClientId == clientId)
                            .SingleOrDefaultAsync() ?? throw new BusinessWebException($"Product [{line.ItemCode}], line [{line.LineNum}] on PO [{po.DocNum}] does not exist");

                        if (product.IsPacksizeControlled)
                        {
                            await context.PurchaseOrderLines.AddAsync(new()
                            {
                                ProductId = product.Id,
                                Description = line.ItemDescription,
                                NumberOfPacks = (int)line.RemainingOpenQuantity,
                                Packsize = (int)line.UnitsOfMeasurment,
                                PurchaseOrderId = poP4W.Id,
                                LineNumber = line.LineNum,
                            });
                        }
                        else
                        {
                            await context.PurchaseOrderLines.AddAsync(new()
                            {
                                ProductId = product.Id,
                                Description = line.ItemDescription,
                                Quantity = line.RemainingOpenQuantity,
                                PurchaseOrderId = poP4W.Id,
                                LineNumber = line.LineNum,
                            });
                        }
                    }

                    poP4W.State = DownloadState.ReadyForDownload;
                    await context.SaveChangesAsync();

                    await LogAsync($"PO [{poP4W.PurchaseOrderNumber}] written to DB");
                }
            }
            catch (Exception e)
            {
                await LogErrorAsync(e);
            }
        }
    }
}
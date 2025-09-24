using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Dto.SAP;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers.Download.ToP4W;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToDb;

public class CustomerReturnsToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            var masterContext = await company.CreateContext(Config.SqlConnection);

            var sapService = SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);
            var lastRead = await masterContext.GetStringAsync(ConfigConstants.Download_ReturnRequest_LastSync);
            var pos = await sapService.Get<CustomerReturnSap>("ReturnRequest", SapServiceClient.GetLastUpdatedRule(lastRead?.ParseDateTimeNullable()));
            if (pos.Count == 0)
                continue;

            await Download(company, pos);

            var maxUpdated = pos.Max(c => c.ActualUpdated);
            await masterContext.SetStringConfig(ConfigConstants.Download_ReturnRequest_LastSync, maxUpdated?.ToString("yyyy-MM-dd'T'HH:mm:ss"));
        }

        await new CustomerReturnsToP4W(Settings).ExecuteAsync();
    }

    public async Task Download(CompanySettings company, List<CustomerReturnSap> pos)
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
                if (po.DocumentStatus != "bost_Open")//Close/Cancel scenario
                {
                    var existingOrders = await context.CustomerReturns
                        .Where(c => !c.Uploaded)
                        .Where(c => c.P4WId != null)
                        .Where(c => c.Reference1 == po.DocEntry)
                        .ToListAsync()
                        ;
                    foreach (var existing in existingOrders)
                    {
                        try
                        {
                            await LogAsync($"PO [{existing.CustomerReturnNumber}] cancelled in SAP");
                            existing.State = DownloadState.ReadyForDownload;
                            existing.IsCancelled = true;
                            await context.SaveChangesAsync();
                        }
                        catch (Exception e)
                        {
                            await LogErrorAsync(e);
                        }
                    }
                    continue;
                }
                
                var customer = await context.Customers
                    .Where(c => c.Code == po.CardCode && c.ClientId == clientId)
                    .SingleOrDefaultAsync();
                // var vendor = await context.Vendors
                //     .Where(c => c.Code == po.CardCode && c.ClientId == clientId)
                //     .SingleOrDefaultAsync();

                if (customer == null)
                {
                    var sapCustomer = await sapService.GetFirst<VendorSap>("BusinessPartners", new(ConditionType.And, [
                        new(nameof(VendorSap.CardCode), Operator.Eq, $"'{po.CardCode}'"),
                        new(nameof(VendorSap.CardType), Operator.Eq, "'cCustomer'"),
                    ]));
                    customer = new()
                    {
                        ClientId = clientId,
                        Code = sapCustomer.CardCode,
                        CompanyName = sapCustomer.CardName,
                    };
                    await context.Customers.AddAsync(customer);
                    await context.SaveChangesAsync();

                    //Push to P4W
                    // await new CustomersToP4W(Settings).ExecuteAsync();
                    // await context.Entry(customer).ReloadAsync();
                }

                var whGroups = po.DocumentLines.GroupBy(c => c.WarehouseCode).Where(c => company.Warehouses.Contains(c.Key)).ToList();
                foreach (var whGroup in whGroups)
                {
                    if (whGroup.All(c => c.RemainingOpenQuantity == 0))
                        continue;

                    var poNum = $"{po.DocNum}{(whGroups.Count == 1 ? "" : $"-{whGroup.Key}")}";

                    var bo = await context.CustomerReturns.CountAsync(c => c.Reference1 == po.DocEntry && c.WarehouseCode == whGroup.Key && c.Uploaded == true);
                    if (bo > 0)
                        poNum = $"{poNum}-{bo}";

                    var poP4W = await context.CustomerReturns.Include(c => c.Lines).SingleOrDefaultAsync(c => c.CustomerReturnNumber == poNum && c.Uploaded == false);
                    if (poP4W == null)
                    {
                        poP4W = new()
                        {
                            CustomerId = customer.Id,
                            CustomerReturnNumber = poNum,
                        };
                        await context.CustomerReturns.AddAsync(poP4W);
                    }
                    else
                    {
                        context.CustomerReturnLines.RemoveRange(poP4W.Lines);
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
                            await context.CustomerReturnLines.AddAsync(new()
                            {
                                ProductId = product.Id,
                                // Description = line.ItemDescription,
                                NumberOfPacks = (int)line.RemainingOpenQuantity,
                                Packsize = (int)line.UnitsOfMeasurment,
                                CustomerReturnId = poP4W.Id,
                                LineNumber = line.LineNum,
                            });
                        }
                        else
                        {
                            await context.CustomerReturnLines.AddAsync(new()
                            {
                                ProductId = product.Id,
                                // Details = line.ItemDescription,
                                Quantity = line.RemainingOpenQuantity,
                                CustomerReturnId = poP4W.Id,
                                LineNumber = line.LineNum,
                            });
                        }
                    }

                    poP4W.State = DownloadState.ReadyForDownload;
                    await context.SaveChangesAsync();

                    await LogAsync($"PO [{poP4W.CustomerReturnNumber}] written to DB");
                }
            }
            catch (Exception e)
            {
                await LogErrorAsync(e);
            }
        }
    }
}
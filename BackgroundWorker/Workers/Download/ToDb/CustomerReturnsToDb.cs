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

            //reset config
            //await masterContext.SetStringConfig(ConfigConstants.Download_ReturnRequest_LastSync, null);

            var sapService = SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);
            var lastRead = await masterContext.GetStringAsync(ConfigConstants.Download_ReturnRequest_LastSync);
            var rmas = await sapService.Get<CustomerReturnSap>("ReturnRequest", SapServiceClient.GetLastUpdatedRule(lastRead?.ParseDateTimeNullable()));
            if (rmas.Count == 0)
                continue;

            await Download(company, rmas);

            var maxUpdated = rmas.Max(c => c.ActualUpdated);
            await masterContext.SetStringConfig(ConfigConstants.Download_ReturnRequest_LastSync, maxUpdated?.ToString("yyyy-MM-dd'T'HH:mm:ss"));
        }

        await new CustomerReturnsToP4W(Settings).ExecuteAsync();
    }

    public async Task Download(CompanySettings company, List<CustomerReturnSap> rmas)
    {
        if (rmas.Count == 0)
            return;

        var sapService = SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);
        var clientId = await GetClientId(company) ?? throw new BusinessWebException($"Client id does not exist");

        foreach (var rma in rmas)
        {
            try
            {
                var context = await company.CreateContext(Config.SqlConnection);
                if (rma.DocumentStatus != "bost_Open")//Close/Cancel scenario
                {
                    var existingOrders = await context.CustomerReturns
                        .Where(c => !c.Uploaded)
                        .Where(c => c.P4WId != null)
                        .Where(c => c.Reference1 == rma.DocEntry)
                        .ToListAsync();

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
                    .Where(c => c.Code == rma.CardCode && c.ClientId == clientId)
                    .SingleOrDefaultAsync();
                // var vendor = await context.Vendors
                //     .Where(c => c.Code == po.CardCode && c.ClientId == clientId)
                //     .SingleOrDefaultAsync();

                if (customer?.P4WId == null)
                {
                    var sapCustomer = await sapService.GetFirst<VendorSap>("BusinessPartners", new(ConditionType.And, [
                        new(nameof(VendorSap.CardCode), Operator.Eq, $"'{rma.CardCode}'"),
                        new(nameof(VendorSap.CardType), Operator.Eq, "'cCustomer'"),
                    ]));

                    if (customer == null)
                    {
                        customer = new()
                        {
                            ClientId = clientId,
                            Code = sapCustomer.CardCode,
                            CompanyName = sapCustomer.CardName,
                        };
                        await context.Customers.AddAsync(customer);
                        await context.SaveChangesAsync();
                    }

                    //Push to P4W
                    await new CustomersToP4W(Settings).ExecuteAsync();
                    await context.Entry(customer).ReloadAsync();
                }

                var whGroups = rma.DocumentLines.GroupBy(c => c.WarehouseCode).Where(c => company.Warehouses.Contains(c.Key)).ToList();
                foreach (var whGroup in whGroups)
                {
                    if (whGroup.All(c => c.RemainingOpenQuantity == 0))
                        continue;

                    var rmaNum = $"{rma.DocNum}{(whGroups.Count == 1 ? "" : $"-{whGroup.Key}")}";

                    var bo = await context.CustomerReturns.CountAsync(c => c.Reference1 == rma.DocEntry && c.WarehouseCode == whGroup.Key && c.Uploaded == true);
                    if (bo > 0)
                        rmaNum = $"{rmaNum}-{bo}";

                    var rmaP4W = await context.CustomerReturns.Include(c => c.Lines).SingleOrDefaultAsync(c => c.CustomerReturnNumber == rmaNum && c.Uploaded == false);
                    if (rmaP4W == null)
                    {
                        rmaP4W = new()
                        {
                            CustomerId = customer.Id,
                            CustomerReturnNumber = rmaNum,
                        };
                        await context.CustomerReturns.AddAsync(rmaP4W);
                    }
                    else
                    {
                        context.CustomerReturnLines.RemoveRange(rmaP4W.Lines);
                        await context.SaveChangesAsync();
                        await context.Entry(rmaP4W).ReloadAsync();
                    }

                    rmaP4W.Reference1 = rma.DocEntry;
                    rmaP4W.WarehouseCode = whGroup.Key;
                    rmaP4W.Comments = rma.Comments;

                    foreach (var line in whGroup.Where(c => c.RemainingOpenQuantity > 0).OrderBy(c => c.LineNum))
                    {
                        var product = await context.Products
                            .Include(c => c.Packsizes)
                            .Where(c => c.Sku == line.ItemCode && c.ClientId == clientId)
                            .SingleOrDefaultAsync() ?? throw new BusinessWebException($"Product [{line.ItemCode}], line [{line.LineNum}] on PO [{rma.DocNum}] does not exist");

                        if (product.IsPacksizeControlled)
                        {
                            await context.CustomerReturnLines.AddAsync(new()
                            {
                                ProductId = product.Id,
                                // Description = line.ItemDescription,
                                NumberOfPacks = (int)line.RemainingOpenQuantity,
                                Packsize = (int)line.UnitsOfMeasurment,
                                CustomerReturnId = rmaP4W.Id,
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
                                CustomerReturnId = rmaP4W.Id,
                                LineNumber = line.LineNum,
                            });
                        }
                    }

                    rmaP4W.State = DownloadState.ReadyForDownload;
                    await context.SaveChangesAsync();

                    await LogAsync($"RMA [{rmaP4W.CustomerReturnNumber}] written to DB");
                }
            }
            catch (Exception e)
            {
                await LogErrorAsync(e);
            }
        }
    }
}
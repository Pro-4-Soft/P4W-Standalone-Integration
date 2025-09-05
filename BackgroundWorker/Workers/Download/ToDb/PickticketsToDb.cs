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

public class PickticketsToDb(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            var masterContext = await company.CreateContext(Config.SqlConnection);

            //reset config
            //await masterContext.SetStringConfig(ConfigConstants.Download_SalesOrder_LastSync, null);

            var sapService = SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);
            var lastRead = await masterContext.GetStringAsync(ConfigConstants.Download_SalesOrder_LastSync);
            var sos = await sapService.Get<SalesOrderSap>("Orders",
                new(ConditionType.And, [
                    new FilterRule("DocumentStatus", Operator.Eq, "'bost_Open'"),
                    SapServiceClient.GetLastUpdatedRule(lastRead?.ParseDateTimeNullable())
                ]));

            if (sos.Count == 0)
                continue;

            await DownloadSos(company, sos);

            var maxUpdated = sos.Max(c => c.ActualUpdated);
            await masterContext.SetStringConfig(ConfigConstants.Download_SalesOrder_LastSync, maxUpdated?.ToString("yyyy-MM-dd'T'HH:mm:ss"));
        }

        await new PickTicketsToP4W(Settings).ExecuteAsync();
    }

    public async Task DownloadSos(CompanySettings company, List<SalesOrderSap> pos)
    {
        if (pos.Count == 0)
            return;

        var sapService = SapServiceClient.GetInstance(company.SapUrl, company.SapCompanyDb, company.SapUsername, company.SapPassword, LogAsync, LogErrorAsync);
        var clientId = await GetClientId(company) ?? throw new BusinessWebException($"Client id does not exist");

        foreach (var so in pos)
        {
            try
            {
                var context = await company.CreateContext(Config.SqlConnection);
                var customer = await context.Customers
                    .Where(c => c.Code == so.CardCode && c.ClientId == clientId)
                    .SingleOrDefaultAsync();

                if (customer == null)
                {
                    var sapCustomer = await sapService.GetFirst<VendorSap>("BusinessPartners", new(ConditionType.And, [
                        new(nameof(VendorSap.CardCode), Operator.Eq, $"'{so.CardCode}'"),
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
                    await new CustomersToP4W(Settings).ExecuteAsync();
                    await context.Entry(customer).ReloadAsync();
                }

                var whGroups = so.DocumentLines.GroupBy(c => c.WarehouseCode).Where(c => company.Warehouses.Contains(c.Key)).ToList();
                foreach (var whGroup in whGroups)
                {
                    if (whGroup.All(c => c.RemainingOpenQuantity == 0))
                        continue;

                    var soNum = $"{so.DocNum}{(whGroups.Count == 1 ? "" : $"-{whGroup.Key}")}";

                    var bo = await context.PickTickets.CountAsync(c => c.Reference1 == so.DocEntry && c.WarehouseCode == whGroup.Key && c.Uploaded == true);
                    if (bo > 0)
                        soNum = $"{soNum}-{bo}";

                    var pickTicketP4W = await context.PickTickets.Include(c => c.Lines).SingleOrDefaultAsync(c => c.PickTicketNumber == soNum && c.Uploaded == false);
                    if (pickTicketP4W == null)
                    {
                        pickTicketP4W = new()
                        {
                            CustomerId = customer.Id,
                            PickTicketNumber = soNum,
                        };
                        await context.PickTickets.AddAsync(pickTicketP4W);
                    }
                    else
                    {
                        context.PickTicketLines.RemoveRange(pickTicketP4W.Lines);
                        await context.SaveChangesAsync();
                        await context.Entry(pickTicketP4W).ReloadAsync();
                    }

                    pickTicketP4W.Reference1 = so.DocEntry;
                    pickTicketP4W.WarehouseCode = whGroup.Key;
                    pickTicketP4W.Comments = so.Comments;

                    pickTicketP4W.FreightType = FreightType.PrivateFleet;
                    pickTicketP4W.PaymentType = PaymentType.PrePay;

                    pickTicketP4W.ShipToName = so.CardName;
                    pickTicketP4W.ShipToAddress1 = so.AddressExtension.ShipToStreet;
                    pickTicketP4W.ShipToAddress2 = so.AddressExtension.ShipToAddress2;
                    pickTicketP4W.ShipToCity = so.AddressExtension.ShipToCity;
                    pickTicketP4W.ShipToStateProvince = so.AddressExtension.ShipToState;
                    pickTicketP4W.ShipToZipPostal = so.AddressExtension.ShipToZipCode;
                    pickTicketP4W.ShipToCountry = so.AddressExtension.ShipToCountry;

                    pickTicketP4W.BillToName = so.CardName;
                    pickTicketP4W.BillToAddress1 = so.AddressExtension.BillToStreet;
                    pickTicketP4W.BillToAddress2 = so.AddressExtension.BillToAddress2;
                    pickTicketP4W.BillToCity = so.AddressExtension.BillToCity;
                    pickTicketP4W.BillToStateProvince = so.AddressExtension.BillToState;
                    pickTicketP4W.BillToZipPostal = so.AddressExtension.BillToZipCode;
                    pickTicketP4W.BillToCountry = so.AddressExtension.BillToCountry;
                    
                    foreach (var line in whGroup.Where(c => c.RemainingOpenQuantity > 0).OrderBy(c => c.LineNum))
                    {
                        var product = await context.Products
                            .Include(c => c.Packsizes)
                            .Where(c => c.Sku == line.ItemCode && c.ClientId == clientId)
                            .SingleOrDefaultAsync() ?? throw new BusinessWebException($"Product [{line.ItemCode}], line [{line.LineNum}] on PO [{so.DocNum}] does not exist");

                        if (product.IsPacksizeControlled)
                        {
                            await context.PickTicketLines.AddAsync(new()
                            {
                                ProductId = product.Id,
                                
                                NumberOfPacks = (int)line.RemainingOpenQuantity,
                                Packsize = (int)line.UnitsOfMeasurment,

                                PickTicketId = pickTicketP4W.Id,
                                SalesPrice = line.Price,
                                LineNumber = line.LineNum,
                            });
                        }
                        else
                        {
                            await context.PickTicketLines.AddAsync(new()
                            {
                                ProductId = product.Id,

                                Quantity = line.RemainingOpenQuantity,

                                PickTicketId = pickTicketP4W.Id,
                                SalesPrice = line.Price,
                                LineNumber = line.LineNum,
                            });
                        }
                    }

                    pickTicketP4W.State = DownloadState.ReadyForDownload;
                    await context.SaveChangesAsync();

                    await LogAsync($"Pickticket [{pickTicketP4W.PickTicketNumber}] written to DB");
                }
            }
            catch (Exception e)
            {
                await LogErrorAsync(e);
            }
        }
    }
}
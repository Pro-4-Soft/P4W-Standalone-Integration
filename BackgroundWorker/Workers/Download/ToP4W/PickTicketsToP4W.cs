using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Workers.Download.ToP4W;

public class PickTicketsToP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        List<WarehouseP4> warehouses = null;
        foreach (var company in Config.Companies)
        {
            try
            {
                await using var context = await company.CreateContext(Config.SqlConnection);

                var sos = await context.PickTickets
                    .Where(c => c.Lines.Count > 0)
                    .Where(c => c.State == DownloadState.ReadyForDownload)
                    .Include(c => c.Customer)
                    .Include(c => c.Lines).ThenInclude(c => c.Product)
                    .ToListAsync();
                if (sos.Count == 0)
                    continue;

                warehouses ??= await P4WClient.GetInvokeAsync<List<WarehouseP4>>("/warehouses");

                foreach (var so in sos)
                {
                    if (so.IsManualCancelledClosed)
                    {
                        if (so.P4WId != null)
                        {
                            try
                            {
                                await P4WClient.WebInvokeAsync($"/pick-tickets/{so.P4WId}", HttpMethod.Delete);
                                await LogAsync($"Pickticket [{so.PickTicketNumber}] delete from P4W");
                            }
                            catch (Exception e)
                            {
                                await LogAsync($"Cannot delete pickticket [{so.PickTicketNumber}] - [{e.Message}]");
                                await P4WClient.WebInvokeAsync($"/pick-tickets/suspend/{so.P4WId}", HttpMethod.Post);
                                await LogAsync($"Pickticket [{so.PickTicketNumber}] suspended");
                            }
                        }

                        context.PickTickets.Remove(so);
                        await context.SaveChangesAsync();
                        continue;
                    }

                    var count = 0;
                    var warehouse = warehouses.SingleOrDefault(c => c.Code == so.WarehouseCode) ?? throw new BusinessWebException($"Warehouse [{so.WarehouseCode}] is not setup in P4W");
                    var payload = new PickTicketP4
                    {
                        //Id = po.P4WId,
                        CustomerId = so.Customer.P4WId ?? throw new BusinessWebException($"Customer [{so.Customer.Code}] has not been synced"),
                        WarehouseId = warehouse.Id.Value,
                        PickTicketNumber = so.PickTicketNumber,
                        RouteNumber = so.RouteNumber,
                        Comments = so.Comments,
                        ReferenceNumber = so.referenceNumber,
                        FreightType = so.FreightType.ToString(),
                        ShipFrom = new()
                        {
                            Name = warehouse.Name,
                            Address1 = warehouse.Address1,
                            Address2 = warehouse.Address2,
                            Email = warehouse.Email,
                            Phone = warehouse.Phone,
                            StateProvince = warehouse.StateProvince,
                            City = warehouse.City,
                            ZipPostal = warehouse.ZipPostalCode,
                            Country = warehouse.Country
                        },
                        ShipTo = new()
                        {
                            Name = so.ShipToName,
                            Address1 = so.ShipToAddress1,
                            Address2 = so.ShipToAddress2,
                            Email = so.ShipToEmail,
                            Phone = so.ShipToPhone,
                            StateProvince = so.ShipToStateProvince,
                            City = so.ShipToCity,
                            ZipPostal = so.ShipToZipPostal,
                            Country = so.ShipToCountry
                        },
                        BillTo = new()
                        {
                            Name = so.BillToName,
                            Address1 = so.BillToAddress1,
                            Address2 = so.BillToAddress2,
                            Email = so.BillToEmail,
                            Phone = so.BillToPhone,
                            StateProvince = so.BillToStateProvince,
                            City = so.BillToCity,
                            ZipPostal = so.BillToZipPostal,
                            Country = so.BillToCountry
                        },
                        Lines = so.Lines
                            .Where(c => (c.NumberOfPacks * c.Packsize ?? c.Quantity) > 0)
                            .Where(c => c.Product.IsInventoryItem)
                            .OrderBy(c => c.LineNumber).Select(c => new PickTicketLineP4()
                            {
                                LineNumber = ++count,
                                ProductId = c.Product.P4WId ?? throw new BusinessWebException($"Product [{c.Product.Sku}] has not been synced"),
                                OrderedQuantity = c.NumberOfPacks * c.Packsize ?? c.Quantity,
                                NumberOfPacks = c.NumberOfPacks,
                                Packsize = c.Packsize,
                                Reference1 = c.Id.ToString()
                            }).ToList()
                    };

                    if (payload.Lines.Count == 0)
                    {
                        so.State = DownloadState.External;
                        await context.SaveChangesAsync();
                        await LogAsync($"Pickticket [{so.PickTicketNumber}] not downloaded. No lines on an order");
                        continue;
                    }

                    try
                    {
                        var existing = await P4WClient.GetInvokeAsync<List<PickTicketP4>>($"/pick-tickets/PickTicketNumber/{payload.PickTicketNumber}");
                        if (existing.Count > 0)
                            payload.Id = existing.First().Id;

                        PickTicketP4 p4Pickticket;
                        if (payload.Id != null)
                            p4Pickticket = await P4WClient.PutInvokeAsync<PickTicketP4>("/pick-tickets", payload);
                        else
                            p4Pickticket = await P4WClient.PostInvokeAsync<PickTicketP4>("/pick-tickets", payload);

                        so.P4WId = p4Pickticket.Id;
                        so.State = DownloadState.Downloaded;
                        if (!so.FileDownloadPath.IsNullOrEmpty())
                        {
                            var fileName = Path.GetFileName(so.FileDownloadPath);
                            var destinationPath = Path.Combine(company.SoDownloadPathCompleted, fileName);
                            File.Move(so.FileDownloadPath, destinationPath);
                        }

                        await LogAsync($"Pickticket [{so.PickTicketNumber}] sent to P4W");
                    }
                    catch (Exception e)
                    {
                        so.ErrorMessage = e.ToString();
                        so.State = DownloadState.DownloadFailed;

                        await LogAsync($"Pickticket [{so.PickTicketNumber}] failed to be sent to P4W\n{e}");
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                await LogErrorAsync(e);
            }
        }
    }
}
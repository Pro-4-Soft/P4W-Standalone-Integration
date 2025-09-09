using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;
using ConditionType = Pro4Soft.BackgroundWorker.Dto.P4W.Entities.ConditionType;
using Operator = Pro4Soft.BackgroundWorker.Dto.P4W.Entities.Operator;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromP4W;

public class AdjustmentsFromP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        var adjusts = await P4WClient.PostInvokeAsync<List<AdjustmentP4>>("/adjustments", new FilterRuleP4(ConditionType.And, [
            new FilterRuleP4("IntegrationReference", Operator.IsNull),
            new FilterRuleP4(ConditionType.Or, [
                new FilterRuleP4("Type", Operator.Equal, AuditType.ProductAdd),
                new FilterRuleP4("Type", Operator.Equal, AuditType.ProductRemove),
            ]),
            new FilterRuleP4(ConditionType.Or, [
                new FilterRuleP4("SubType", Operator.Equal, AuditSubType.AdjustIn),
                new FilterRuleP4("SubType", Operator.Equal, AuditSubType.AdjustOut),
                new FilterRuleP4("SubType", Operator.Equal, AuditSubType.CycleCount),
            ])
        ]));

        if (adjusts.Any())
        {
            var now = DateTime.Now;
            var clientGroups = adjusts.GroupBy(c => c.Client).ToList();
            foreach (var clientGroup in clientGroups)
            {
                var company = Config.Companies.SingleOrDefault(c => c.P4WClientName == clientGroup.Key);
                if (company == null)
                    continue;//Ignore adjustments for clients that we don't know about

                var clientId = await GetClientId(company) ?? throw new BusinessWebException($"Client id does not exist");
                var intReference = $"{now:s} - {now.Ticks}";

                var ids = clientGroup.Select(c => c.Id.Value).ToList();

                foreach (var adj in clientGroup)
                {
                    try
                    {
                        var context = await company.CreateContext(Config.SqlConnection);
                        var existing = await context.Adjustments.SingleOrDefaultAsync(c => c.P4WId == adj.Id);
                        if (existing != null)
                            continue;

                        var product = await context.Products.SingleOrDefaultAsync(c => c.P4WId == adj.ProductId) 
                                      ?? throw new BusinessWebException($"Product [{adj.Sku}] with P4WId [{adj.ProductId}] does not exist in Database");
                        await context.Adjustments.AddAsync(new()
                        {
                            State = DownloadState.ReadyForUpload,

                            P4WId = adj.Id,
                            Timestamp = adj.Timestamp,
                            ClientId = clientId,
                            ProductId = product.Id,
                            Client = company.P4WClientName,

                            FromWarehouse = adj.FromWarehouse,
                            ToWarehouse = adj.ToWarehouse,

                            Type = adj.Type,
                            SubType = adj.SubType,
                            Quantity = adj.Quantity,

                            EachCount = adj.EachCount,
                            NumberOfPacks = adj.NumberOfPacks,
                            LotNumber = adj.LotNumber,
                            SerialNumber = adj.SerialNumber,
                            ExpiryDate = adj.ExpiryDate,

                            Reason = adj.Reason,
                        });
                        await context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        await P4WClient.PostInvokeAsync("/adjustments/upload", new AdjustmentConfirmationP4
                        {
                            Ids = [adj.Id.Value],
                            Message = e.Message,
                            Reference = intReference
                        });

                        ids.Remove(adj.Id.Value);
                        await LogErrorAsync(e);
                    }
                }

                await P4WClient.PostInvokeAsync("/adjustments/upload", new AdjustmentConfirmationP4
                {
                    Ids = ids,
                    Message = null,
                    Reference = intReference
                });
            }

            await LogAsync($"[{adjusts.Count}] adjustments written to db");
        }

        await new AdjustmentsFromDb(Settings).ExecuteAsync();
    }
}
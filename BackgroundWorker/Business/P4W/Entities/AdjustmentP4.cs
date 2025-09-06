using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Business.P4W.Entities;

public class AdjustmentP4 : BaseP4Entity
{
    [JsonProperty("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonProperty("client")]
    public string Client { get; set; }

    [JsonProperty("subType")]
    public string SubType { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("fromWarehouse")]
    public string FromWarehouse { get; set; }

    [JsonProperty("toWarehouse")]
    public string ToWarehouse { get; set; }

    [JsonProperty("productId")]
    public Guid ProductId { get; set; }

    [JsonProperty("sku")]
    public string Sku { get; set; }

    [JsonProperty("eachCount")]
    public int? EachCount { get; set; }

    [JsonProperty("numberOfPacks")]
    public int? NumberOfPacks { get; set; }

    [JsonProperty("lotNumber")]
    public string LotNumber { get; set; }

    [JsonProperty("expiryDate")]
    public DateTimeOffset? ExpiryDate { get; set; }

    [JsonProperty("serialNumber")]
    public string SerialNumber { get; set; }

    [JsonProperty("quantity")]
    public decimal Quantity { get; set; }

    [JsonProperty("reason")]
    public string Reason { get; set; }
}

public class FilterRuleP4
{
    public FilterRuleP4()
    {
    }

    public FilterRuleP4(string field, string op)
    {
        Field = field;
        Operator = op;
        Value = null;
    }

    public FilterRuleP4(string field, string op, string value)
    {
        Field = field;
        Operator = op;
        Value = value;
    }

    public FilterRuleP4(string condition, List<FilterRuleP4> rules)
    {
        Condition = condition;
        Rules = rules;
    }

    [JsonProperty("condition")]
    public string Condition { get; set; }

    [JsonProperty("field")]
    public string Field { get; set; }

    [JsonProperty("operator")]
    public string Operator { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }

    [JsonProperty("rules")]
    public List<FilterRuleP4> Rules { get; set; }
}

public class AdjustmentConfirmationP4
{
    [JsonProperty("ids")] 
    public List<Guid> Ids { get; set; }
    
    [JsonProperty("integrationReference")] 
    public string Reference { get; set; }
    
    [JsonProperty("integrationMessage")] 
    public string Message { get; set; }
}

public static class ConditionType
{
    public const string And = nameof(And);
    public const string Or = nameof(Or);
}

public static class Operator
{
    public const string In = nameof(In);
    public const string NotIn = nameof(NotIn);
    public const string Equal = nameof(Equal);
    public const string NotEqual = nameof(NotEqual);
    public const string Between = nameof(Between);
    public const string NotBetween = nameof(NotBetween);
    public const string Contains = nameof(Contains);
    public const string NotContains = nameof(NotContains);
    public const string Less = nameof(Less);
    public const string LessOrEqual = nameof(LessOrEqual);
    public const string Greater = nameof(Greater);
    public const string GreaterOrEqual = nameof(GreaterOrEqual);
    public const string IsEmpty = nameof(IsEmpty);
    public const string IsNotEmpty = nameof(IsNotEmpty);
    public const string IsNull = nameof(IsNull);
    public const string IsNotNull = nameof(IsNotNull);
    public const string BeginsWith = nameof(BeginsWith);
    public const string NotBeginsWith = nameof(NotBeginsWith);
    public const string EndsWith = nameof(EndsWith);
    public const string NotEndsWith = nameof(NotEndsWith);
}

public static class AuditType
{
    public const string ProductAdd = nameof(ProductAdd);
    public const string ProductMove = nameof(ProductMove);
    public const string ProductRemove = nameof(ProductRemove);
    public const string LicensePlateMove = nameof(LicensePlateMove);
    public const string Handling = nameof(Handling);
    public const string CustomAction = nameof(CustomAction);
    public const string UserAction = nameof(UserAction);
}

public static class AuditSubType
{
    public static readonly string BillingCycleStart = nameof(BillingCycleStart);
    public static readonly string BillingCycleEnd = nameof(BillingCycleEnd);
    public static readonly string Pick = nameof(Pick);
    public static readonly string OneScanShip = nameof(OneScanShip);

    public static readonly string ProdOrderPick = nameof(ProdOrderPick);
    public static readonly string ProdOrderProdStep = nameof(ProdOrderProdStep);
    public static readonly string ProdOrderConsume = nameof(ProdOrderConsume);
    public static readonly string ProdOrderBuild = nameof(ProdOrderBuild);

    public static readonly string SubstituteConvert = nameof(SubstituteConvert);

    public static readonly string ReceivePo = nameof(ReceivePo);
    public static readonly string UnreceivePo = nameof(UnreceivePo);
    public static readonly string NonPoReceive = nameof(NonPoReceive);
    public static readonly string ReceiveRma = nameof(ReceiveRma);
    public static readonly string UnreceiveRma = nameof(UnreceiveRma);
    public static readonly string NonRmaReceive = nameof(NonRmaReceive);
    public static readonly string SmallParcelShip = nameof(SmallParcelShip);
    public static readonly string ExternalShip = nameof(ExternalShip);
    public static readonly string TruckLoadShip = nameof(TruckLoadShip);
    public static readonly string PrivateFleetShip = nameof(PrivateFleetShip);

    public static readonly string MasterTruckLoadShip = nameof(MasterTruckLoadShip);

    public static readonly string VendorReturn = nameof(VendorReturn);
    public static readonly string CustomerReturn = nameof(CustomerReturn);
    public static readonly string AdjustOut = nameof(AdjustOut);

    public static readonly string AdjustIn = nameof(AdjustIn);
    public static readonly string PacksizeConvert = nameof(PacksizeConvert);
    public static readonly string PacksizeBreakdown = nameof(PacksizeBreakdown);
    public static readonly string Letdown = nameof(Letdown);
    public static readonly string LicensePlateMove = nameof(LicensePlateMove);
    public static readonly string BulkStageMove = nameof(BulkStageMove);
    public static readonly string ProductMove = nameof(ProductMove);
    public static readonly string WarehouseMove = nameof(WarehouseMove);
    public static readonly string BinMove = nameof(BinMove);
    public static readonly string UnpickPickTicket = nameof(UnpickPickTicket);
    public static readonly string UnpickTote = nameof(UnpickTote);
    public static readonly string ToteFloorMove = nameof(ToteFloorMove);
    public static readonly string ToteDockDoorMove = nameof(ToteDockDoorMove);
    public static readonly string ToteBinMove = nameof(ToteBinMove);
    public static readonly string ToteLpnMove = nameof(ToteLpnMove);
    public static readonly string ShipToteCount = nameof(ShipToteCount);
    public static readonly string DeliveryToteCount = nameof(DeliveryToteCount);
    public static readonly string Repackage = nameof(Repackage);
    public static readonly string Init = nameof(Init);
    public static readonly string CycleCount = nameof(CycleCount);
    public static readonly string CycleCountApproval = nameof(CycleCountApproval);

    public static readonly string PalletPrinting = nameof(PalletPrinting);

    public static readonly string LoginWeb = nameof(LoginWeb);
    public static readonly string LoginHandheld = nameof(LoginHandheld);
    public static readonly string LoginAttemptNoLicense = nameof(LoginAttemptNoLicense);
    public static readonly string LoginExternal = nameof(LoginExternal);
    public static readonly string LoginImpersonate = nameof(LoginImpersonate);

    public static readonly string LogoutRegular = nameof(LogoutRegular);
    public static readonly string LogoutNoLicense = nameof(LogoutNoLicense);
    public static readonly string LogoutUserDuplicate = nameof(LogoutUserDuplicate);
    public static readonly string LogoutUserDeactivated = nameof(LogoutUserDeactivated);
    public static readonly string LogoutByAdmin = nameof(LogoutByAdmin);
    public static readonly string LogoutExpiry = nameof(LogoutExpiry);
}
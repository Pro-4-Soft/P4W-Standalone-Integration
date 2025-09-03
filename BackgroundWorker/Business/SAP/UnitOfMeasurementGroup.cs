namespace Pro4Soft.BackgroundWorker.Business.SAP;

public class UnitOfMeasurementGroup : BaseSapEntity
{
    public int AbsEntry { get; set; }

    public List<UoMGroupDefinition> UoMGroupDefinitionCollection { get; set; } = [];

}

public class UoMGroupDefinition
{
    public int BaseQuantity { get; set; }
    public string Active { get; set; }
}
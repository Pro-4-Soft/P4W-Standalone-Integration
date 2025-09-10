namespace Pro4Soft.BackgroundWorker.Dto.SAP;

public class UnitOfMeasurementGroup : BaseSapEntity
{
    public int AbsEntry { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }

    public List<UoMGroupDefinition> UoMGroupDefinitionCollection { get; set; } = [];

}

public class UoMGroupDefinition
{
    public int AlternateUoM { get; set; }
    public int BaseQuantity { get; set; }
    public string Active { get; set; }
}
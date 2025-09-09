namespace Pro4Soft.BackgroundWorker.Dto.SAP;

public class CustomerSap : BaseSapEntity
{
    public string CardCode { get; set; }
    public string CardName { get; set; }
    public string CardType { get; set; }

    public string Phone1 { get; set; }

    public DateTime? UpdateDate { get; set; }
    public string UpdateTime { get; set; }
    public DateTime? ActualUpdated => UpdateDate?.Add(TimeSpan.Parse(UpdateTime ?? "00:00:00"));
}
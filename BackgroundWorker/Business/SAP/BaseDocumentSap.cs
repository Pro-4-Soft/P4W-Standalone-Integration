namespace Pro4Soft.BackgroundWorker.Business.SAP;

public class BaseDocumentSap : BaseSapEntity
{
    public string DocNum { get; set; }
    public string DocEntry { get; set; }
    public string DocType { get; set; }
    public DateTime? DocDate { get; set; }
    public DateTime? DocDueDate { get; set; }
}
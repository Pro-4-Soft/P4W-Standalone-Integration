namespace Pro4Soft.BackgroundWorker.Business.SAP;

public class BaseSapEntity
{
    public DateTime? UpdateDate { get; set; }
    public string UpdateTime { get; set; }
    public DateTime? ActualUpdated => UpdateDate?.Add(TimeSpan.Parse(UpdateTime ?? "00:00:00"));

    //[JsonProperty("@odata.etag")]
    //public string ODataEtag { get; set; }
}
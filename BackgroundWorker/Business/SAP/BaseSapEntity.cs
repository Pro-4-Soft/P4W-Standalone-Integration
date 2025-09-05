using Newtonsoft.Json;

namespace Pro4Soft.BackgroundWorker.Business.SAP;

public class BaseSapEntity
{
    //[JsonProperty("@odata.etag")]
    //public string ODataEtag { get; set; }
}

public class ErrorSap
{
    public string Code { get; set; }
    public List<Detail> Details { get; set; }
    public string Message { get; set; }
}

public class Detail
{
    public string Code { get; set; }
    public string Message { get; set; }
}
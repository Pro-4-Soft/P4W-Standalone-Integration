using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Business.P4W.Entities;

public class ClientP4: BaseP4Entity
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("ssccCompanyId")]
    public string SsccCompanyId { get; set; }
        
    [JsonProperty("description")]
    public string Description { get; set; }
}
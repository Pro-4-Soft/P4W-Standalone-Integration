using Newtonsoft.Json;

namespace Pro4Soft.BackgroundWorker.Dto.P4W.Entities.Base;

public class BaseP4Entity
{
    [JsonProperty("id")]
    public Guid? Id { get; set; }
}
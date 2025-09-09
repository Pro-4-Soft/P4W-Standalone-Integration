using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Dto.P4W.Entities;

public class VendorP4 : BaseP4Entity
{
    [JsonProperty("vendorCode")]
    public string Code { get; set; }

    [JsonProperty("companyName")]
    public string CompanyName { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("clientId")]
    public Guid? ClientId { get; set; }

    [JsonProperty("client")]
    public ClientP4 Client { get; set; }
}
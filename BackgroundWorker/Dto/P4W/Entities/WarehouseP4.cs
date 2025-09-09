using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Dto.P4W.Entities;

public class WarehouseP4 : BaseP4Entity
{
    [JsonProperty("warehouseCode")]
    public string Code { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("phone")]
    public string Phone { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("referenceNumber")]
    public string ReferenceNumber { get; set; }

    [JsonProperty("address1")]
    public string Address1 { get; set; }

    [JsonProperty("address2")]
    public string Address2 { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("stateProvince")]
    public string StateProvince { get; set; }

    [JsonProperty("zipPostalCode")]
    public string ZipPostalCode { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }
}
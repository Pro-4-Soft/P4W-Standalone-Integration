using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Dto.P4W.Entities;

public class ProductP4: BaseP4Entity
{
    [JsonProperty("sku")]
    public string Sku { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; }

    [JsonProperty("upc")]
    public string Upc { get; set; }

    [JsonProperty("reference1")]
    public string Reference1 { get; set; }
    [JsonProperty("reference2")]
    public string Reference2 { get; set; }
    [JsonProperty("reference3")]
    public string Reference3 { get; set; }

    [JsonProperty("length")]
    public decimal? Length { get; set; }
    [JsonProperty("width")]
    public decimal? Width { get; set; }
    [JsonProperty("height")]
    public decimal? Height { get; set; }
    [JsonProperty("weight")]
    public decimal? Weight { get; set; }

    [JsonProperty("isSerialControlled")]
    public bool IsSerialControlled { get; set; }
    [JsonProperty("isLotControlled")]
    public bool IsLotControlled { get; set; }
    [JsonProperty("isPacksizeControlled")]
    public bool IsPacksizeController { get; set; }

    [JsonProperty("clientId")]
    public Guid? ClientId { get; set; }
    
    [JsonProperty("packsizes")]
    public List<PacksizeP4> Packsizes { get; set; } = [];
}

public class PacksizeP4
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("eachCount")]
    public int EachCount { get; set; }
}
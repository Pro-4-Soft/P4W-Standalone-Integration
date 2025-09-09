using Newtonsoft.Json;

namespace Pro4Soft.BackgroundWorker.Dto.P4W.Entities.Base;

public class UploadConfirmationP4
{
    [JsonProperty("ids")]public List<Guid> Ids { get; set; }
    [JsonProperty("uploadedSuceeded")]public bool UploadSucceeded { get; set; }
    [JsonProperty("resetUploadCount")] public bool ResetUploadCount { get; set; } = true;
    [JsonProperty("uploadMessage")]public string UploadMessage { get; set; }
}
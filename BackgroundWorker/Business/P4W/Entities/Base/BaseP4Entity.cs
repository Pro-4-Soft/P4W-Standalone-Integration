using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pro4Soft.BackgroundWorker.Business.P4W.Entities.Base;

public class BaseP4Entity
{
    [JsonProperty("id")]
    public Guid? Id { get; set; }
}

public class UploadConfirmationP4
{
    [JsonProperty("ids")]public List<Guid> Ids { get; set; }
    [JsonProperty("uploadedSuceeded")]public bool UploadSucceeded { get; set; }
    [JsonProperty("resetUploadCount")] public bool ResetUploadCount { get; set; } = true;
    [JsonProperty("uploadMessage")]public string UploadMessage { get; set; }
}
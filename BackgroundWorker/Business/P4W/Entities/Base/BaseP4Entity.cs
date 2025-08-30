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
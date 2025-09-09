using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pro4Soft.BackgroundWorker.Execution.Common;

namespace Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;

public class ConfigEntry : EntityBase
{
    public string Name { get; set; }

    [NotMapped]
    public string Description { get; set; }

    [NotMapped]
    public ConfigType Type { get; set; }

    [NotMapped]
    public string[] MultiSelectOptions { get; set; }

    [MaxLength(int.MaxValue)]
    public string StringValue { get; set; }
    public int? IntValue { get; set; }
    public double? DoubleValue { get; set; }
    public bool? BoolValue { get; set; }
}
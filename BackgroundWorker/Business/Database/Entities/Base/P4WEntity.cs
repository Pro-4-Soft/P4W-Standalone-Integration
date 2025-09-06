using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;

[Index(nameof(P4WId))]
public class P4WEntity : EntityBase
{
    public Guid? P4WId { get; set; }
}

[Index(nameof(State))]
public class P4WStateEntity : P4WEntity
{
    public DownloadState State { get; set; } = DownloadState.ReadyForDownload;

    [MaxLength(int.MaxValue)]
    public string ErrorMessage { get; set; }
}


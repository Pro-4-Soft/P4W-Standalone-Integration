using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;

public abstract class EntityBase
{
    protected EntityBase()
    {
        Id = Guid.NewGuid();
        DateCreated = DateTimeOffset.UtcNow;
        DateModified = DateCreated;
    }

    [Key]
    public Guid Id { get; set; }

    public DateTimeOffset DateCreated { get; set; }

    public DateTimeOffset DateModified { get; set; }

    [Timestamp]
    [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public byte[] RowVersion { get; set; }

    [NotMapped]
    public string TypeName => GetType().FullName;
}

public abstract class EntityBaseMap<T> : IEntityTypeConfiguration<T> where T : EntityBase
{
    public abstract void Configure(EntityTypeBuilder<T> builder);
}

public enum DownloadState
{
    External,
    ReadyForDownload,
    Downloaded,
    Failed
}
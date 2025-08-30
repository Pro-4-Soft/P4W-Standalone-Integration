using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pro4Soft.BackgroundWorker.Business.Database.Entities;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Business.Database;

public class DatabaseContext(DbContextOptions options) : DbContext(options)
{
    public DatabaseContext() : this($"Server=localhost;Database=int_sample;User Id=sa;Password=;TrustServerCertificate=True;")
    {
        
    }

    public readonly string ConnectionString;

    public DatabaseContext(string connectionString) : this(new DbContextOptionsBuilder<DatabaseContext>().UseSqlServer(connectionString).Options)
    {
        ConnectionString = connectionString;
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }

    public DbSet<Product> Products { get; set; }
    public DbSet<Packsize> Packsizes { get; set; }
    
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Vendor> Vendors { get; set; }

    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }

    public DbSet<PickTicket> PickTickets { get; set; }
    public DbSet<PickTicketLine> PickTicketLines { get; set; }
    public DbSet<Tote> Totes { get; set; }
    public DbSet<ToteLine> ToteLines { get; set; }

    public DbSet<CustomerReturn> CustomerReturns { get; set; }
    public DbSet<CustomerReturnLine> CustomerReturnLines { get; set; }

    //Overrides
    public override int SaveChanges()
    {
        return SaveChanges(true);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PreSave();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        return SaveChangesAsync(true, cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
    {
        PreSave();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void PreSave()
    {
        var now = DateTimeOffset.UtcNow;
        var dbChanges = ChangeTracker.Entries()
            .Where(c => c.Entity is EntityBase)
            .Where(e => e.State != EntityState.Unchanged)
            .ToList();

        foreach (var dbChange in dbChanges)
        {
            if (dbChange.Entity is not EntityBase changedEntity)
                continue;

            changedEntity.DateModified = dbChange.State switch
            {
                EntityState.Added or EntityState.Modified => now,
                _ => changedEntity.DateModified
            };
        }
    }

    protected static List<Type> EntityTypes => typeof(EntityBase).Assembly.GetTypes()
        .Where(c => c.BaseType != null)
        .Where(c => c.BaseType?.IsGenericType == true)
        .Where(c => c.BaseType.GetGenericTypeDefinition() == typeof(EntityBaseMap<>))
        .ToList();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var type in EntityTypes)
            modelBuilder.ApplyConfiguration((dynamic)Activator.CreateInstance(type));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                Type type = null;
                if (property.ClrType.IsEnum)
                    type = typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType);
                else if (Nullable.GetUnderlyingType(property.ClrType)?.IsEnum == true)
                    type = typeof(EnumToStringConverter<>).MakeGenericType(Nullable.GetUnderlyingType(property.ClrType));

                if (type == null)
                    continue;

                var converter = Activator.CreateInstance(type, new ConverterMappingHints()) as ValueConverter;
                property.SetValueConverter(converter);
                property.SetMaxLength(64);
            }
        }

        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => p.ClrType == typeof(string)))
        {
            if (property.GetMaxLength() == null)
                property.SetMaxLength(256);
            else if (property.GetMaxLength() == int.MaxValue)
                property.SetMaxLength(null);
        }

        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            if (property.GetPrecision() != null || property.GetScale() != null)
                continue;

            property.SetPrecision(15);
            property.SetScale(6);
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.AddInterceptors(new CommentsInterceptor());
        // below line to watch the ef core sql queries generation
        // not recommended for the production code
        //optionsBuilder.LogTo(Console.WriteLine);
    }
}
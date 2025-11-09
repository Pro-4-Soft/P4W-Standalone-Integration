using System.Data;
using System.Reflection;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Workers;

namespace Pro4Soft.BackgroundWorker.Dto.Database;

public class DatabaseContext(DbContextOptions options) : DbContext(options)
{
    public DatabaseContext() : this($"Server=p4sql.westus3.cloudapp.azure.com;Database=master;User Id=p4portal-server-admin;Password=YUSj$ui1UueoIGGM;TrustServerCertificate=True;")
    {
        
    }

    public readonly string ConnectionString;

    public DatabaseContext(string connectionString) : this(new DbContextOptionsBuilder<DatabaseContext>().UseSqlServer(connectionString).Options)
    {
        ConnectionString = connectionString;
    }

    public DbSet<ConfigEntry> ConfigEntries { get; set; }

    public DbSet<Product> Products { get; set; }
    public DbSet<Packsize> Packsizes { get; set; }
    
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Vendor> Vendors { get; set; }

    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
    public DbSet<PurchaseOrderLineDetail> PurchaseOrderLineDetails { get; set; }

    public DbSet<Pickticket> PickTickets { get; set; }
    public DbSet<PickTicketLine> PickTicketLines { get; set; }
    public DbSet<Tote> Totes { get; set; }
    public DbSet<ToteLine> ToteLines { get; set; }
    public DbSet<ToteLineDetail> ToteLineDetails { get; set; }

    public DbSet<CustomerReturn> CustomerReturns { get; set; }
    public DbSet<CustomerReturnLine> CustomerReturnLines { get; set; }
    public DbSet<CustomerReturnLineDetail> CustomerReturnLineDetails { get; set; }

    public DbSet<Adjustment> Adjustments { get; set; }
    public DbSet<ProductInventory> ProductInventory { get; set; }
    public DbSet<ProductInventoryDetail> ProductInventoryDetails { get; set; }

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

    public IDbConnection GetConnection() => Database.GetDbConnection();

    //Seed
    public async Task Seed()
    {
        var props = typeof(ConfigConstants).GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var oldVal in ConfigEntries.ToList().Where(oldVal => !props.Select(c => c.Name).Contains(oldVal.Name)))
            ConfigEntries.Remove(oldVal);

        foreach (var prop in props)
        {
            if (Attribute.GetCustomAttribute(prop, typeof(ConfigDefinitionAttribute)) is not ConfigDefinitionAttribute attr)
                continue;
            if (ConfigEntries.Any(c => c.Name == prop.Name))
                continue;

            var newConfig = new ConfigEntry
            {
                Name = prop.Name,
                Type = attr.Type
            };

            switch (newConfig.Type)
            {
                case ConfigType.String:
                case ConfigType.MultilineString:
                case ConfigType.MultiSelect:
                case ConfigType.EmailBody:
                    newConfig.StringValue = (string)attr.DefaultValue;
                    break;
                case ConfigType.Password:
                    newConfig.StringValue = !string.IsNullOrWhiteSpace((string)attr.DefaultValue) ? SymmetricCrypt.Encrypt((string)attr.DefaultValue) : null;
                    break;
                case ConfigType.Int:
                    newConfig.IntValue = (int?)attr.DefaultValue ?? default;
                    break;
                case ConfigType.Double:
                    newConfig.DoubleValue = (double?)attr.DefaultValue ?? default;
                    break;
                case ConfigType.Bool:
                    newConfig.BoolValue = (bool?)attr.DefaultValue ?? default;
                    break;
            }
            ConfigEntries.Add(newConfig);
        }

        await SaveChangesAsync();
    }

    //Configuration
    public async Task<ConfigEntry> GetAsync(string name) => await Database.GetDbConnection().QuerySingleOrDefaultAsync<ConfigEntry>($@"
select 
    * 
from 
    {nameof(ConfigEntries)} 
where 
    {nameof(ConfigEntry.Name)} = @Name", new
    {
        Name = name,
    }) ?? throw new BusinessWebException($"Config [{name}] does not exist");

    public async Task<bool> GetBoolAsync(string name)
    {
        return (await GetAsync(name)).BoolValue ?? false;
    }

    public async Task<int> GetIntAsync(string name)
    {
        return (await GetAsync(name)).IntValue ?? 0;
    }

    public async Task<string> GetStringAsync(string name)
    {
        return (await GetAsync(name))?.StringValue;
    }

    public async Task<string> GetPasswordAsync(string name)
    {
        var res = (await GetAsync(name))?.StringValue;
        if (string.IsNullOrWhiteSpace(res))
            return res;
        return SymmetricCrypt.TryDecrypt(res, out var s) ? s : null;
    }

    public async Task<double> GetDoubleAsync(string name)
    {
        return (await GetAsync(name)).DoubleValue ?? 0;
    }

    public async Task<T> GetEnumAsync<T>(string name) where T : struct
    {
        var strValue = (await GetAsync(name))?.StringValue;
        return Enum.TryParse(strValue, out T result) ? result : throw new BusinessWebException($"Cannot parse [{strValue}] as [{typeof(T).Name}]");
    }

    public async Task SetStringConfig(string name, string value)
    {
        await Database.GetDbConnection().ExecuteAsync($@"
update 
    {nameof(ConfigEntries)} 
set 
    {nameof(ConfigEntry.StringValue)} = @Value 
where 
    {nameof(ConfigEntry.Name)} = @Name", new
        {
            Name = name,
            Value = value,
        });
    }

    public async Task SetIntConfig(string name, int? value)
    {
        await Database.GetDbConnection().ExecuteAsync($@"
update 
    {nameof(ConfigEntries)} 
set 
    {nameof(ConfigEntry.IntValue)} = @Value 
where 
    {nameof(ConfigEntry.Name)} = @Name", new
        {
            Name = name,
            Value = value,
        });
    }

    public async Task SetDoubleConfig(string name, double? value)
    {
        await Database.GetDbConnection().ExecuteAsync($@"
update 
    {nameof(ConfigEntries)} 
set 
    {nameof(ConfigEntry.DoubleValue)} = @Value 
where 
    {nameof(ConfigEntry.Name)} = @Name", new
        {
            Name = name,
            Value = value,
        });
    }

    public async Task SetBoolConfig(string name, bool? value)
    {
        await Database.GetDbConnection().ExecuteAsync($@"
update 
    {nameof(ConfigEntries)} 
set 
    {nameof(ConfigEntry.BoolValue)} = @Value 
where 
    {nameof(ConfigEntry.Name)} = @Name", new
        {
            Name = name,
            Value = value,
        });
    }
}
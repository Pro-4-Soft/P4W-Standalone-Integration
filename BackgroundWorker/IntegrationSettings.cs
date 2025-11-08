using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers;

namespace Pro4Soft.BackgroundWorker;

public class IntegrationSettings : Settings
{
    public string SqlConnection { get; set; }

    public string P4WUrl { get; set; }
    public string P4WApiKey { get; set; }

    public readonly List<CompanySettings> Companies = [];

    public string TempFolder { get; set; }
}

public class CompanySettings
{
    public string SapUrl { get; set; }
    public string SapCompanyDb { get; set; }
    public string SapUsername { get; set; }
    public string SapPassword { get; set; }
    public string P4WClientName { get; set; }
    
    public string SoDownloadPath { get; set; }
    public string SoDownloadPathCompleted { get; set; }
    
    public string ClientId { get; set; }
    public string CustomerCode { get; set; }
    public string CompanyName { get; set; }
    
    public List<string> Warehouses { get; set; } = [];
    public List<string> SyncWarehouses { get; set; } = [];

    public async Task<DatabaseContext> CreateContext(string masterConnectionString, bool forceCreateMigrate = false)
    {
        var builder = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = "master",
        };
        var dbName = $"{Constants.DatabasePrefix}_{SapCompanyDb}";
        if (!forceCreateMigrate)
        {
            builder.InitialCatalog = dbName;
            return new(builder.ConnectionString);
        }

        using IDbConnection masterDbConnection = new SqlConnection(builder.ConnectionString);
        var dbExist = await masterDbConnection.ExecuteScalarAsync($"SELECT 1 FROM sys.databases WHERE Name = '{dbName}'") != null;
        if (!dbExist)
        {
            await masterDbConnection.ExecuteAsync($"CREATE DATABASE {dbName}");
            await masterDbConnection.ExecuteAsync($"ALTER DATABASE [{dbName}] SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE");
        }

        builder.InitialCatalog = dbName;
        DatabaseContext context = new(builder.ConnectionString);
        await context.Database.MigrateAsync();
        await context.Seed();

        return context;
    }
}

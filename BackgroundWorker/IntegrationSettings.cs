using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business;
using Pro4Soft.BackgroundWorker.Business.Database;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker;

public class IntegrationSettings : Settings
{
    public string SqlConnection { get; set; }

    public string P4WUrl { get; set; }
    public string P4WApiKey { get; set; }

    public List<CompanySettings> Companies = [];

    public string TempFolder { get; set; }
}

public class CompanySettings
{
    public string SapUrl { get; set; }
    public string SapCompanyDb { get; set; }
    public string SapUsername { get; set; }
    public string SapPassword { get; set; }
    public string P4WClientName { get; set; }
    public List<string> Warehouses { get; set; } = [];

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

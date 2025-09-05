using Pro4Soft.BackgroundWorker.Execution;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers.Upload.FromDb;

namespace Pro4Soft.BackgroundWorker.Workers.Upload.FromP4W;

public class AdjustmentsFromP4W(ScheduleSetting settings) : BaseWorker(settings)
{
    public override async Task ExecuteAsync()
    {
        foreach (var company in Config.Companies)
        {
            try
            {
                
            }
            catch (Exception e)
            {
                await LogErrorAsync(e);
            }
        }

        await new PickticketsFromDb(Settings).ExecuteAsync();
    }
}
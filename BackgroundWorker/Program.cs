using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pro4Soft.BackgroundWorker.Execution;

namespace Pro4Soft.BackgroundWorker;

internal class Program
{
    internal static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseWindowsService(config =>
            {
                config.ServiceName = "Pro4Soft - Background worker";
            })
            .ConfigureServices((_, services) =>
            {
                services.AddHostedService<WorkerHostThread>();
            })
            .Build();
        await host.RunAsync();
    }
}

public class WorkerHostThread : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await BaseWorker.LogAsync($"Process started.");
        ScheduleThread.Instance.Start();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        ScheduleThread.Instance.Stop();
        await BaseWorker.LogAsync($"Process finished.");
    }
}
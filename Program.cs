using CodeMechanic.FileSystem;
using CodeMechanic.Types;
using Coravel;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using worker2.Services;

namespace worker2;

public class Program
{
    public static async Task Main(string[] args)
    {
        DotEnv.Load();
        IHost host = CreateHostBuilder(args)
            .UseSystemd()
            .Build();

        host.Services.UseScheduler(scheduler =>
            {
                var prod_maybe = Environment.GetEnvironmentVariable("MODE").ToMaybe();

                var settings = ConfigReader
                    .LoadConfig<WorkerSettings>(
                        "worker_settings.json"
                        , fallback: new WorkerSettings());

                // scheduler
                //     .Schedule<Regex101Invocable>()
                //     .Daily()
                //     .Once()
                //     ;

                // FILE WATCHER
                scheduler
                    .Schedule<FileWatcherInvocable>()
                    .Daily()
                    .RunOnceAtStart();


                // settings.Dump(nameof(settings));

                /** SMART OVERDUE TASKS RESCHEDULER **/

                scheduler
                    .Schedule<TodoistRescheduler>()
                    .Daily()
                    // .Cron("00 9,13,20 * * *")
                    .RunOnceAtStart()
                    .PreventOverlapping(nameof(InvocableTodoistBumper));

                if (settings.bump.enabled)
                    /** AUTO BUMPER */

                    prod_maybe.Case<string>(some: _ =>
                    {
                        scheduler
                            .Schedule<InvocableTodoistBumper>()
                            .EverySeconds(settings.bump.wait_seconds)
                            .PreventOverlapping(nameof(TodoistRescheduler));
                        return _;
                    }, none: () =>
                    {
                        scheduler
                            .Schedule<InvocableTodoistBumper>()
                            .EverySeconds(60 * settings.bump.wait_minutes)
                            .PreventOverlapping(nameof(TodoistRescheduler))
                            ;

                        return "";
                    });
            })
            .OnError((exception) => LogExceptionToDB(exception));

        host.Run();
    }

    private static async Task LogExceptionToDB(Exception exception)
    {
        await TemporaryExceptionLogger.LogException(exception);
    }


    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<ITodoistSchedulerService, TodoistSchedulerService>();

                services.AddScheduler();
                services.AddTransient<InvocableTodoistBumper>();
                services.AddTransient<TodoistRescheduler>();
                services.AddSingleton<Regex101Invocable>();

                // FS Watcher
                services.AddSingleton<FileSystemQueue>();
                services.AddSingleton<Listener>();
                services.AddSingleton<FileWatcherInvocable>();
                // services.AddHostedService<FileSystemWorker>();
            });
}
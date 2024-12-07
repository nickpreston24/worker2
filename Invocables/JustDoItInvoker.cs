using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using CliWrap;
using CodeMechanic.Bash;
using CodeMechanic.Diagnostics;
using Coravel.Invocable;
using MySql.Data.MySqlClient;

namespace worker2;

public class JustDoItInvoker : IInvocable
{
    private ITodoSchedulerService todos_scheduler = new TodoSchedulerService();

    public JustDoItInvoker(ITodoSchedulerService service)
    {
        todos_scheduler = service;
    }

    public async Task Invoke()
    {
        string cwd = Directory.GetCurrentDirectory();
        var watch = Stopwatch.StartNew();

        await todos_scheduler.CreateRandomSchedule();

        watch.Stop();
        Console.WriteLine(watch.Elapsed);
    }

    private async Task<List<Todo>> AutoRescheduleFilteredTasks(
        Reschedule rescheduling_options,
        bool debug = true
    )
    {
        if (debug)
            Console.WriteLine(nameof(AutoRescheduleFilteredTasks));
        var results = await todos_scheduler.SearchTodos(new TodoSearch("guns") { });
        results.Dump("all todos");

        return default;
    }

    private async ValueTask<bool> SaveRun(
        List<TodoUpdates> actualUpdates,
        Reschedule reschedulingOptions
    )
    {
        string connectionString = SQLConnections.GetMySQLConnectionString();
        using var connection = new MySqlConnection(connectionString);
        string insert_query =
            @"insert into run_history (method_name, filter, created_by) values (@method_name, @filter, @created_by)";

        var results = await Dapper.SqlMapper.QueryAsync(
            connection,
            insert_query,
            new
            {
                method_name = reschedulingOptions.name,
                filter = reschedulingOptions.filter,
                created_by = nameof(worker2),
            }
        );

        return true;
    }
}

public static class ShellExtensions
{
    /// <summary>
    /// Run any bash command and see the output
    /// Sources:
    ///    Basics - https://code-maze.com/csharp-execute-cli-applications/
    ///    Events - https://jackma.com/2019/04/20/execute-a-bash-script-via-c-net-core/
    ///    AVOID - CLIWrap
    /// </summary>
    /// <param name="command"></param>
    /// <param name="verbose">Show/hide output</param>
    /// <param name="writeline">Overload to whatever output function you like for verbose mode</param>
    /// <returns></returns>
    public static async Task<string> BashV2(
        this string command,
        bool verbose = false,
        Action<string> writeline = null
    )
    {
        if (writeline == null)
            writeline = Console.WriteLine;

        var escapedArgs = command.Replace("\"", "\\\"");

        var psi = new ProcessStartInfo();
        // psi.FileName = "/bin/bash";
        psi.FileName = "bash";
        psi.Arguments = $"-c \"{escapedArgs}\"";
        // psi.Arguments = command;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;
        // psi.CreateNoWindow = true;

        if (verbose)
            writeline($"Running command `{command}`");
        using var process = Process.Start(psi);

        ArgumentNullException.ThrowIfNull(process);

        // process.WaitForExit();
        await process.WaitForExitAsync();

        if (verbose)
            writeline("Done!");

        var output = process.StandardOutput.ReadToEnd();

        if (verbose)
            writeline(output);

        return output;
    }

    public static Task<int> BashJackMa(this string cmd)
    {
        TaskCompletionSource<int> source = new TaskCompletionSource<int>();
        string str = cmd.Replace("\"", "\\\"");
        Process process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "bash",
                Arguments = "-c \"" + str + "\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = true,
        };
        process.Exited += (EventHandler)(
            (sender, args) =>
            {
                if (process.ExitCode == 0)
                {
                    source.SetResult(0);
                }
                else
                {
                    TaskCompletionSource<int> completionSource = source;
                    DefaultInterpolatedStringHandler interpolatedStringHandler =
                        new DefaultInterpolatedStringHandler(35, 2);
                    interpolatedStringHandler.AppendLiteral("Command `");
                    interpolatedStringHandler.AppendFormatted(cmd);
                    interpolatedStringHandler.AppendLiteral("` failed with exit code `");
                    interpolatedStringHandler.AppendFormatted<int>(process.ExitCode);
                    interpolatedStringHandler.AppendLiteral("`");
                    Exception exception = new Exception(
                        interpolatedStringHandler.ToStringAndClear()
                    );
                    completionSource.SetException(exception);
                }

                process.Dispose();
            }
        );
        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            DefaultInterpolatedStringHandler interpolatedStringHandler =
                new DefaultInterpolatedStringHandler(16, 2);
            interpolatedStringHandler.AppendLiteral("Command ");
            interpolatedStringHandler.AppendFormatted(cmd);
            interpolatedStringHandler.AppendLiteral(" failed\n");
            interpolatedStringHandler.AppendFormatted<Exception>(ex);
            Console.WriteLine(interpolatedStringHandler.ToStringAndClear());
            source.SetException(ex);
        }

        return source.Task;
    }
}

public static class Notifiers
{
    public static async Task<int> SendMessage_JackMa(string title = "", string message = "")
    {
        string name = "Script Name";
        var output =
            await $@"notify-send '{title}' '{message}' -a '{name}' -u normal -i face-smile".BashJackMa();

        return output;
    }

    public static async Task<string> SendMessage_CliWrap(string title = "", string message = "")
    {
        var output = new StringBuilder();
        string cwd = Directory.GetCurrentDirectory();
        string cmd = $@"notify-send '{title}' '{message}' -a '{title}' -u normal -i face-smile";
        var result = await Cli.Wrap("bash")
            .WithArguments(cmd)
            .WithWorkingDirectory(cwd)
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        return output.ToString(); //todo: update to match cliwrap's tutorial
    }

    public static async Task<string> SendMessage(string title = "", string message = "")
    {
        string name = "Script Name";
        string output =
            await $@"notify-send '{title}' '{message}' -a '{name}' -u normal -i face-smile".Bash(
                verbose: true
            );

        return output;
    }
}

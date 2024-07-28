using System.Diagnostics;
using CodeMechanic.Diagnostics;
using CodeMechanic.FileSystem;
using CodeMechanic.Types;
using Coravel.Invocable;
using worker2.Concurrency;

namespace worker2.Services;

public class Regex101Invocable : IInvocable
{
    public async Task Invoke()
    {
        string root = "/home/nick/Desktop/projects/personal/";

        // var url_lines = await QuickGrep(root, Regex101Pattern.Regex101);

        var grepResults = await QuickGrep(root, Regex101Pattern.RegexPatternString);
        grepResults.Select(x => x.FilePath).Dump("results from quick grep (outside)");
        Console.WriteLine($"quick grep results count :>> {grepResults.Count}");
    }

    private static async void RunCodemazeExample()
    {
        var messageBus = new OrderMessageBus();

        var producer1 = new Producer(messageBus, 10);
        var producer2 = new Producer(messageBus, 10);
        var producer3 = new Producer(messageBus, 10);

        var consumer1 = new Consumer(messageBus);
        var consumer2 = new Consumer(messageBus);
        var consumer3 = new Consumer(messageBus);

        Console.WriteLine("Waiting for queue to finish....");

        Task.WaitAll(
            producer1.Produce()
            , producer2.Produce()
            , producer3.Produce()
            , consumer1.Process()
            , consumer2.Process()
            , consumer3.Process()
        );

        Console.WriteLine("queue finished!");
    }

    private static async Task<List<Grepper.GrepResult>> QuickGrep(string root, Regex101Pattern rgx)
    {
        var dirfiles = Directory
                .EnumerateFiles(root, "*", new EnumerationOptions() { RecurseSubdirectories = true })
            // .Count()
            ;

        // dirfiles.TakeRandom(5).Dump("dirfiles");

        int total_files_searched = dirfiles.Count();
        Console.WriteLine($"file count :>> {total_files_searched}");

        var watch = Stopwatch.StartNew();

        Console.WriteLine("Looking for Regex 101.com patterns  on drive ... ");
        // var grepResults = await GetGrepResultsThreaded(root, rgx);
        var grepResults = await GrepDirectories(root, rgx);

        watch.Stop();
        Console.WriteLine(watch);
        Console.WriteLine($"total files searched: {total_files_searched}\n total grep results: {grepResults.Count}");
        return grepResults;
    }

    private static async Task<List<Grepper.GrepResult>> GetGrepResultsThreaded(string root, Regex101Pattern rgx)
    {
        var subdirectories = Directory
            .GetDirectories(root)
            // .Select(dir => dir.AsDirectory())
            // .Select(dir => dir.FullName)
            // .Select(dir => dir
            //     .Extract<Subdirectory>(SubDirectoryRegex.Basic.CompiledRegex))
            // .Flatten()
            // .Select(x => x.dirname)
            // .Dump("subdirectories")
            .ToArray();

        var tasks = subdirectories.Select(sdir =>
        {
            var t = Task.Run(() =>
            {
                Console.WriteLine($"looking in subdirectory '{sdir}'");

                var results = new Grepper()
                {
                    RootPath = root,
                    FileSearchMask = "*.cs*",
                    FileSearchLinePattern = rgx.Pattern
                }.GetMatchingFiles().DistinctBy(x => x.FilePath).ToList();

                // Console.WriteLine($"done looking in subdir '{sdir}'.");
                Console.WriteLine($"files found {results.Count}");
                return results;
            });

            t.ConfigureAwait(false);
            return t;
        });

        var all_results = await Task.WhenAll(tasks);
        return all_results
            .Flatten()
            .DistinctBy(res => res.FilePath)
            .ToList();
    }

    private static async Task<List<Grepper.GrepResult>> GrepDirectories(string root, Regex101Pattern rgx)
    {
        var grepper = new Grepper()
        {
            RootPath = root,
            FileSearchMask = "*.cs*",
            FileSearchLinePattern = rgx.Pattern
        };

        var dirs = await GrepperExtensions.SearchDirectories(grepper, root,
            subfolder_patterns: new[]
                { ".*" } // sadly, I cannot append the subfolders to .* and await each for some reason.
            // subfolder_patterns: subdirectories
        );

        // var grepResults = new List<Grepper.GrepResult>();

        var grepResults = dirs
                .SelectMany(x => x.Value)
                .DistinctBy(x => x.FilePath)
                .ToList()
            // .Dump("files containing regular expressions")
            ;
        return grepResults;
    }
}
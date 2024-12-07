using System.Collections.Concurrent;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;

// using Coravel.Queuing.Interfaces.ICancellableTask;

namespace worker2;

public class FileWatcherInvocable : IInvocable
{
    private readonly ILogger<FileSystemWorker> _logger;
    private readonly FileSystemQueue _queue;
    private readonly Listener _listener;

    private BlockingCollection<string> queue;
    private ConcurrentDictionary<string, DateTime> processedFileMap;

    CancellationToken cancellationToken { get; set; }

    public FileWatcherInvocable(
        ILogger<FileSystemWorker> logger,
        FileSystemQueue queue,
        Listener listener
    )
    {
        _logger = logger;
        _queue = queue;
        _listener = listener;
    }

    public async Task Invoke()
    {
        Console.WriteLine("invoked");
        await RunAsChannel();
    }

    // private async Task ProcessFiles()
    // {
    //     while (!queue.IsCompleted)
    //     {
    //         var filePath = queue.Take(); //Blocking dequeue
    //         var fileInfo = new FileInfo(filePath);
    //
    //         if (!fileInfo.Exists)
    //             continue;
    //
    //         if (processedFileMap.TryGetValue(filePath, out DateTime processedWithModDate))
    //         {
    //             if (processedWithModDate == fileInfo.LastWriteTimeUtc)
    //             {
    //                 Console.WriteLine($"Ignoring duplicate change event for file: {filePath}");
    //                 continue;
    //             }
    //
    //             //It's a new change, so process it, then update mod date.
    //             Console.WriteLine($"Processed file again: {filePath}");
    //             processedFileMap[filePath] = fileInfo.LastWriteTimeUtc;
    //         }
    //         else
    //         {
    //             //We haven't processed this file before. Process it, then save the mod date.
    //             Console.WriteLine($"Processed file for the first time: {filePath}.");
    //             processedFileMap.TryAdd(filePath, fileInfo.LastWriteTimeUtc);
    //         }
    //     }
    // }

    private async Task RunAsChannel()
    {
        Console.WriteLine("starting channel..");
        while (!cancellationToken.IsCancellationRequested)
        {
            var @event = await _queue.Consume(cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            // await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
            await WebAppOperations.FireTheDEI(@event);

            _logger.LogInformation(
                "[{date}] {fileName} arrived at worker.",
                $"{DateTime.Now:O}",
                @event.Name
            );
        }
    }

    #region OLD

    // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    // {
    //     _listener.Start();
    //     while (!stoppingToken.IsCancellationRequested)
    //     {
    //         var @event = await _queue.Consume(stoppingToken);
    //         await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
    //         _logger.LogInformation("[{date}] {fileName} arrived to worker.", $"{DateTime.Now:O}", @event.Name);
    //     }
    // }

    // public FileWatcherInvocable()
    // {
    // }
    //
    // public async Task Invoke()
    // {
    //     // Console.WriteLine($"Invoked {nameof(FileWatcherInvocable)}");
    //     string projects_root = "/home/nick/Desktop/projects/personal";
    //
    //     var all_projects = new Grepper()
    //     {
    //         RootPath = projects_root,
    //         FileSearchMask = "*.csproj"
    //     }.GetFileNames().ToList();
    //     Console.WriteLine($"total cs projects: {all_projects.Count}");
    //
    //     FileSystemWatcher fileSystemWatcher = new FileSystemWatcher()
    //     {
    //         Path = projects_root,
    //         Filter = "*.cs*",
    //         IncludeSubdirectories = true,
    //         EnableRaisingEvents = true,
    //         /* Watch for changes in LastAccess and LastWrite times, and
    //   the renaming of files or directories. */
    //         // NotifyFilter = NotifyFilters.LastAccess
    //         //                | NotifyFilters.LastWrite
    //         //                | NotifyFilters.FileName
    //         //                // | NotifyFilters.Size
    //         //                | NotifyFilters.DirectoryName
    //     };
    //
    //     fileSystemWatcher.Created += OnFileCreation;
    //     fileSystemWatcher.Renamed += OnFileRename;
    //     fileSystemWatcher.Deleted += OnFileDelete;
    //     fileSystemWatcher.Changed += OnFileChange;
    //     fileSystemWatcher.Error += OnErrorFired;
    // }
    //
    // private void OnErrorFired(object sender, ErrorEventArgs e)
    // {
    //     Console.WriteLine("oh snap! Something went wrong!");
    // }
    //
    // private void OnFileChange(object sender, FileSystemEventArgs e)
    // {
    //     Console.WriteLine($" file {e.Name} changed at path '{e.FullPath}'");
    //     // Console.WriteLine(e.ChangeType);
    //     // Console.WriteLine("sender: \n" + sender.ToString());
    //     // FireTheDEI(e);
    // }
    //
    // private void OnFileDelete(object sender, FileSystemEventArgs e)
    // {
    //     Console.WriteLine($" file {e.Name} was deleted (at '{e.FullPath}')");
    // }
    //
    // private void OnFileRename(object sender, RenamedEventArgs e)
    // {
    //     Console.WriteLine("File: '{0}' renamed to '{1}'", e.OldFullPath, e.FullPath);
    // }
    //
    // private void OnFileCreation(object sender, FileSystemEventArgs e)
    // {
    //     Console.WriteLine($" a file {e.Name} was created at '{e.FullPath}'");
    // }

    #endregion OLD
}

using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace worker2;

/// <summary>
/// Credit: https://medium.com/@hanifi.yildirimdagi/efficient-file-processing-using-_watcher-in-net-e7f1c994e91d
/// </summary>
public sealed class Listener
{
    private readonly FileSystemWatcher _watcher;
    private readonly ILogger<Listener> _logger;
    private readonly FileSystemQueue _queue;
    private BlockingCollection<string> file_queue;
    private ConcurrentDictionary<string, DateTime> processedFileMap;

    public Listener(IConfiguration configuration, ILogger<Listener> logger, FileSystemQueue queue)
    {
        _logger = logger;
        _queue = queue;
        var options = configuration.GetSection("ListenOptions").Get<ListenOptions>();
        ArgumentNullException.ThrowIfNull(options);
        string projects_root = "/home/nick/Desktop/projects/personal";

        _watcher = new FileSystemWatcher()
        {
            Path = projects_root,
            Filter = "*.cs*",
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            /* Watch for changes in LastAccess and LastWrite times, and
      the renaming of files or directories. */
            // NotifyFilter = NotifyFilters.LastAccess
            //                | NotifyFilters.LastWrite
            //                | NotifyFilters.FileName
            //                 | NotifyFilters.Size
            //                | NotifyFilters.DirectoryName
            // | NotifyFilters.CreationTime
        };

        _watcher.Created += Created;
        // _watcher.Renamed += OnFileRename;
        // _watcher.Deleted += OnFileDelete;
        _watcher.Changed += OnFileChange;
        // _watcher.Error += OnErrorFired;

        if (!string.IsNullOrEmpty(options.Filter))
            _watcher.Filter = options.Filter;
        _logger.LogDebug($"[{DateTime.Now:O}] {nameof(Listener)}: Instance created.");
    }

    public void Start() => _watcher.EnableRaisingEvents = true;

    private void OnFileChange(object sender, FileSystemEventArgs e)
    {
        file_queue.Add(e.FullPath);
        Console.WriteLine($" file {e.Name} changed at path '{e.FullPath}'");
        _queue.Produce(e).Wait();
        Console.WriteLine("Waiting complete.");
        // Console.WriteLine(e.ChangeType);
        // Console.WriteLine("sender: \n" + sender.ToString());
        // FireTheDEI(e);
    }

    private void Created(object sender, FileSystemEventArgs e)
    {
        file_queue.Add(e.FullPath);
        _logger.LogInformation(
            $"[{DateTime.Now:O}] {nameof(Listener)}: The file has been created. File : {e.Name}"
        );
        _queue.Produce(e).Wait();
    }
}

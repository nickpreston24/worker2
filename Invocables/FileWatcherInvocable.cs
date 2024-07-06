using CodeMechanic.FileSystem;
using Coravel.Invocable;

namespace worker2;

public class FileWatcherInvocable : IInvocable
{
    public FileWatcherInvocable()
    {
    }

    public async Task Invoke()
    {
        Console.WriteLine($"Inovoked {nameof(FileWatcherInvocable)}");
        string projects_root = "/home/nick/Desktop/projects/personal";

        var all_projects = new Grepper()
        {
            RootPath = projects_root,
            FileSearchMask = "*.csproj"
        }.GetFileNames().ToList();
        Console.WriteLine($"total cs projects: {all_projects.Count}");

        FileSystemWatcher fileSystemWatcher = new FileSystemWatcher()
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
            //                // | NotifyFilters.Size
            //                | NotifyFilters.DirectoryName
        };

        fileSystemWatcher.Created += OnFileCreation;
        fileSystemWatcher.Renamed += OnFileRename;
        fileSystemWatcher.Deleted += OnFileDelete;
        fileSystemWatcher.Changed += OnFileChange;
        fileSystemWatcher.Error += OnErrorFired;
    }

    private void OnErrorFired(object sender, ErrorEventArgs e)
    {
        Console.WriteLine("oh snap! Something went wrong!");
    }

    private void OnFileChange(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($" file {e.Name} changed at path '{e.FullPath}'");
        Console.WriteLine(e.ChangeType);
        // Console.WriteLine("sender: \n" + sender.ToString());
    }

    private void OnFileDelete(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($" file {e.Name} was deleted (at '{e.FullPath}')");
    }

    private void OnFileRename(object sender, RenamedEventArgs e)
    {
        Console.WriteLine("File: '{0}' renamed to '{1}'", e.OldFullPath, e.FullPath);
    }

    private void OnFileCreation(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($" a file {e.Name} was created at '{e.FullPath}'");
    }
}
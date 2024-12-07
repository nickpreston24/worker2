using CodeMechanic.Bash;
using CodeMechanic.FileSystem;

namespace worker2;

public static class WebAppOperations
{
    public static async ValueTask<bool> FireTheDEI(FileSystemEventArgs e)
    {
        // Console.WriteLine(nameof(FireTheDEI));
        string name = e.Name;
        string filepath = e.FullPath;

        string dir = Path.GetDirectoryName(filepath);
        Console.WriteLine($"dir: {dir}");

        // if (dir.Contains("Pages"))
        // {
        //     dir = dir.GoUp();
        //     Console.WriteLine($"dir: {dir}");
        // }

        var project_folder = dir.AsDirectory().GoUpToDirectory("Pages").GoUp();

        Console.WriteLine("project dir: " + project_folder);

        string package_json_filepath = Path.Combine(project_folder, "package.json");
        Console.WriteLine($"package.json: {package_json_filepath}");

        if (File.Exists(package_json_filepath))
        {
            await $"cd {project_folder};".Bash(verbose: true);
            await "yarn buildcss:linux".Bash(verbose: true);
        }

        return true;
    }
}

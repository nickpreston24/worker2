namespace worker2;

public static class FSExtensions
{
    /// <summary>
    /// Create a file in the current dir.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static FileInfo CreateFileHere(string filename, params string[] lines)
    {
        string cwd = Directory.GetCurrentDirectory();
        string save_path = Path.Combine(cwd, filename);
        File.WriteAllLines(save_path, lines);
        return new FileInfo(save_path);
    }
}
using CodeMechanic.RegularExpressions;

namespace worker2.Services;

public class SubDirectoryRegex : RegexEnumBase
{
    public static SubDirectoryRegex Basic = new SubDirectoryRegex(
        1
        , nameof(Basic)
        , @"(?<front>.*\/)(?<dirname>[\w\.-]+)"
        , "https://regex101.com/r/LEpZdJ/1");

    protected SubDirectoryRegex(int id, string name, string pattern, string uri = "") : base(id, name, pattern, uri)
    {
    }
}
using System.Text.RegularExpressions;
using CodeMechanic.Types;

namespace worker2.Services;

public class SubDirectoryRegex : Enumeration // : CodeMechanic.RegularExpressions.RegexEnumBase
{
    public static SubDirectoryRegex Basic = new SubDirectoryRegex(
        1,
        nameof(Basic),
        @"(?<front>.*\/)(?<dirname>[\w\.-]+)",
        "https://regex101.com/r/LEpZdJ/1"
    );

    protected SubDirectoryRegex(int id, string name, string pattern, string uri = "")
        : base(id, name)
    {
        this.Pattern = pattern;
        this.CompiledRegex = new Regex(
            pattern,
            RegexOptions.Multiline
                | RegexOptions.IgnoreCase
                | RegexOptions.Compiled
                | RegexOptions.IgnorePatternWhitespace
        );
    }

    public Regex CompiledRegex { get; set; }

    public string Pattern { get; set; } = string.Empty;
}

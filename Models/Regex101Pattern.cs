using CodeMechanic.RegularExpressions;

namespace worker2.Services;

public class Regex101Pattern : RegexEnumBase
{
    // A url with regex101 in it, e.g: https://regex101.com/r/abCzYX/1
    public static Regex101Pattern Regex101 = new Regex101Pattern(1, @"Regex101",
        @"(?<domain>https?:\/\/regex101\.com)\/(?<api>\w*)\/(?<id>\w+)/(?<rest>\d*)",
        "https://regex101.com/r/W4ffzt/1");

    // an actual regex pattern string:
    public static Regex101Pattern RegexPatternString = new Regex101Pattern(2, nameof(RegexPatternString),
        @"Hello there");

    protected Regex101Pattern(int id, string name, string pattern, string uri = "") : base(id, name, pattern, uri)
    {
    }
}
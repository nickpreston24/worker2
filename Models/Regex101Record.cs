using CodeMechanic.Types;

namespace worker2.Services;

public record Regex101Record(string domain, string api = "", string id = "", string rest = "")
{
    public bool IsValid => domain.NotEmpty() && api.NotEmpty() && id.NotEmpty();
}

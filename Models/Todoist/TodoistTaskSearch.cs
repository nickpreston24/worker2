namespace worker2;

public class TodoistTaskSearch
{
    public TodoistTaskSearch(string filter)
    {
        this.filter = filter.AsParameterizedString();
    }

    public string filter { get; set; } = string.Empty;
    public string[] ids { get; set; } = Array.Empty<string>();
    public string label { get; set; } = string.Empty;
    public string project_id { get; set; } = string.Empty;
}
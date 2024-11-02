namespace worker2;

public sealed class TodoSearch
{
    public string[] ids { get; set; } = { };
    public string project_id { set; get; }
    public string filter { set; get; }
    public string label { get; set; }

    public TodoSearch(string label)
    {
        this.label = label;
    }
}
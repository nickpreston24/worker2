namespace worker2;

public sealed class TodoUpdates
{
    public string id { get; set; }
    public string content;
    public string due_date;
    public string description;
    public string[] labels;
    public int priority;
}
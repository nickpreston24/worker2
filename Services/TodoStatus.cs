using CodeMechanic.Types;

namespace worker2;

public class TodoStatus : Enumeration
{
    public static TodoStatus Done = new TodoStatus(1, nameof(Done));
    public static TodoStatus Pending = new TodoStatus(2, nameof(Pending));
    public static TodoStatus WIP = new TodoStatus(3, nameof(WIP));
    public static TodoStatus Postponed = new TodoStatus(4, nameof(Postponed));

    public TodoStatus(int id, string name)
        : base(id, name) { }

    public static implicit operator TodoStatus(string status)
    {
        var found = TodoStatus
            .GetAll<TodoStatus>()
            .SingleOrDefault(x => x.Name.Equals(status, StringComparison.CurrentCultureIgnoreCase));
        return found;
    }
}

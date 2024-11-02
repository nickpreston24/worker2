using CodeMechanic.Todoist;

namespace worker2;

public interface ITodoistSchedulerService
{
    Task<List<TodoistTask>> BumpTasks(int days = 7);

    Task<List<TodoistTask>> SearchTodos(TodoistTaskSearch search);
    Task<TodoistTask?> CreateTodo(TodoistUpdates todo);
    Task<TodoistTask> DeleteTodo(string id);
    Task<List<TodoistTask>> GetTodosById(string id);
    Task<List<TodoistTask>> UpdateTodos(List<TodoistUpdates> todoist_updates, int delay_in_seconds = 5);
}
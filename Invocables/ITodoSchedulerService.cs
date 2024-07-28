namespace worker2;

public interface ITodoSchedulerService
{
    Task<List<Todo>> BumpTasks(int days = 7);

    Task<List<Todo>> SearchTodos(TodoSearch search);
    Task<Todo> CreateTodo(Todo todo);
    Task<Todo> DeleteTodo(string id);
    Task<List<Todo>> GetTodosById(string id);
    Task<int> UpdateTodos(List<Todo> todo_updates, int delay_in_seconds = 5);

    /// <summary>
    /// Creates an entirely random schedule from all todos
    /// </summary>
    /// <returns></returns>
    Task<int> CreateRandomSchedule();
}
using System.Diagnostics;
using CodeMechanic.Async;
using CodeMechanic.Diagnostics;
using CodeMechanic.Types;
using Dapper;

namespace worker2;

public class TodoSchedulerService : ITodoSchedulerService
{
    public async Task<List<Todo>> SearchTodos(TodoSearch search)
    {
        Console.WriteLine(nameof(SearchTodos));
        if (search == null) throw new ArgumentNullException(nameof(search));

        string joined_ids = string.Join(",", search.ids);
        string label = search.label.Dump("labels");
        string filter = search.filter;
        string project_id = search.project_id;

        string query = @"
        select *
            from todos
            where todos.is_sample_data = 1";
        using var connection = SQLConnections.CreateConnection();
        var todos = (await connection.QueryAsync<Todo>(query)).ToList();
        Console.WriteLine("total todos:>> " + todos.Count);
        return todos;
    }

    /// <summary>
    /// Creates an entirely random schedule from all todos
    /// </summary>
    /// <returns></returns>
    public async Task<int> CreateRandomSchedule()
    {
        int[] days = new[] { 1, 3, 5, 7, 9, 11, 13, 31 };
        int[] priorities = Enumerable.Range(1, 4).ToArray();

        var sample_todos = Enumerable.Range(1, 10)
            .Select(i =>
            {
                var now = DateTime.Now;
                var due_date = now.AddDays(days.TakeFirstRandom());
                var start_date = now.AddDays(days.TakeFirstRandom());
                var end_date = start_date.AddDays(days.TakeFirstRandom());
                int priority = priorities.TakeFirstRandom();
                var duration = TimeSpan.FromMinutes(60 / priority);
                var todo = new Todo
                {
                    content = "testzzz",
                    due = due_date,
                    start = start_date,
                    end = end_date,
                    priority = priority,
                    created_at = now,
                    duration = duration,
                    is_sample_data = true
                };

                return todo;
            })
            .ToList();

        // sample_todos.Dump(" random sample ", ignoreNulls: true);

        int rows = await UpdateTodos(sample_todos);

        // var all_todos = await SearchTodos(new TodoSearch(string.Empty));
        // Console.WriteLine("total " + all_todos.Count);
        // return all_todos.Count;
        return rows;
    }

    public async Task<int> UpdateTodos(List<Todo> todo_updates, int delay_in_seconds = 5)
    {
        try
        {
            if (todo_updates.Count == 0) return 0;
            var Q = new SerialQueue();

            // var updated_todos = new List<Todo>();
            int update_count = 0;
            Stopwatch sw = Stopwatch.StartNew();
            var tasks = todo_updates
                .Select(update => Q
                    .Enqueue(async () =>
                        {
                            var updates = await PerformUpdate(update);
                            // updated_todos.AddRange(updates);
                            update_count += updates;
                        }
                    ));

            await Task.WhenAll(tasks);

            sw.Stop();
            Console.WriteLine(sw.Elapsed);

            return update_count;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Todo> CreateTodo(Todo todo)
    {
        return new Todo(); // temporary
    }

    private async Task<int> PerformUpdate(Todo todo)
    {
        bool debug = true;
        string update_query = @"INSERT INTO todos (id, content, due, start, end, priority, status, is_sample_data)
        VALUES (@id, @content, @due, @start, @end, @priority, @status, @is_sample_data)
        ON DUPLICATE KEY UPDATE content = VALUES(content),
                            priority=VALUES(priority),
                            status=VALUES(status)
       ";

        using var connection = SQLConnections.CreateConnection();
        int affected = await connection.ExecuteAsync(update_query, new
        {
            id = todo.id,
            status = todo.status,
            content = todo.content,
            due = todo.due,
            start = todo.start,
            end = todo.end,
            priority = todo.priority,
            is_sample_data = todo.is_sample_data,
        });
        Console.WriteLine(affected);

        return affected;
    }


    public async Task<Todo> DeleteTodo(string id)
    {
        try
        {
            var doomed_todos = await GetTodosById(id);
            return new Todo();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<Todo>> GetTodosById(string id)
    {
        return (await SQLConnections
                .CreateConnection()
                .QueryAsync<Todo>(@"select * from todos where id = @id", new
                {
                    id = id.ToInt()
                })
            )
            .ToList();
    }

    /// <summary>
    /// Bumps any tasks marked with 'bump' to the specified days in the description.
    /// </summary>
    /// <returns></returns>
    public async Task<List<Todo>> BumpTasks(int days = 7)
    {
        // todo: create a view for updates with bumps
        // var updated_tasks = await UpdateTodos(actual_updates);
        return default;
    }
}
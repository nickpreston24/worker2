using System.Diagnostics;
using CodeMechanic.Async;
using CodeMechanic.Diagnostics;
using CodeMechanic.FileSystem;
using CodeMechanic.Todoist;
using CodeMechanic.Types;
using Coravel.Invocable;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace worker2;

public class TodoistRescheduler : IInvocable
{
    private readonly ITodoistSchedulerService todoist;

    public virtual ReschedulingOptions rescheduling_options { get; set; } = new();

    public TodoistRescheduler(ITodoistSchedulerService service)
    {
        todoist = service;
    }

    public async Task Invoke()
    {
        Console.WriteLine("Starting Rescheduler Invoke");
        rescheduling_options.Dump("current options");
        return;
        try
        {
            // string log_message = "Beginning Invoke at '" + DateTime.Now.ToString("o") + "'";
            // File.AppendAllText("rescheduler.log", log_message);
            // rescheduling_options.Dump(nameof(rescheduling_options));

            var Q = new SerialQueue();
            int delay_after_task = 250;

            Stopwatch sw = Stopwatch.StartNew();
            var tasks = rescheduling_options.Reschedules
                .Where(rs => rs.enabled)
                .Select(reschedule => Q
                    .Enqueue(async () =>
                        {
                            var changed = await AutoRescheduleFilteredTasks(reschedule);
                            // Console.WriteLine("changed " + changed.Count);
                            Thread.Sleep(delay_after_task);
                            //// other crap
                        }
                    ));

            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// AutoRescheduleFilteredTasks to a given size.
    /// Never have overdue tasks again.
    /// </summary>
    /// <param name="daily_task_limit">How large I want each day to be for reschudeled overdue tasks.  Too large, and I'll get overwhelmed.  Too small, and I'm procrastinating</param>
    /// <returns></returns>
    private async Task<List<TodoistTask>> AutoRescheduleFilteredTasks(
        Reschedule rescheduling_options,
        bool debug = false
    )
    {
        try
        {
            // rescheduling_options.Dump();
            if (!rescheduling_options.enabled)
            {
                Console.WriteLine("disabled.  returning.");
                return new List<TodoistTask>(0);
            }

            if (rescheduling_options.filter.IsEmpty()) throw new ArgumentNullException(nameof(rescheduling_options));

            debug = rescheduling_options.debug;

            List<TodoistTask> candidates = new List<TodoistTask>(0);

            string cache_file_name = "todo_cache.json";
            if (rescheduling_options.use_cache)
            {
                string lines = File.ReadAllText(cache_file_name);
                candidates = JsonConvert.DeserializeObject<List<TodoistTask>>(lines);
                Console.WriteLine($"Total todos loaded from cache {candidates.Count}");
            }
            else if (!rescheduling_options.use_cache)
            {
                candidates = await todoist.SearchTodos(new TodoistTaskSearch("guns")
                {
                });
            }


            string candidates_json = JsonConvert.SerializeObject(candidates);
            var savecache = new SaveAs(cache_file_name);
            FS.SaveAs(savecache, candidates_json);


            bool include_non_recurring =
                rescheduling_options.filter.Contains(
                    "!recurring"); // if filter contains the ! before recurring, then don't allow recurring.  The API designers forgot to not mess this up.

            // if this is set, then we want all non-recurring tasks, regardless of label or overdue status...
            // bool exactly_equals_non_recurring = rescheduling_options.filter.Equals("!recurring");

            Console.WriteLine("Include non recurring? " + include_non_recurring);

            var any_recurring = candidates
                .Where(x => x.due != null && !x.due.is_recurring.ToBoolean())
                .ToList();

            // if (!include_non_recurring)
            // Console.WriteLine("TOTAL NON-RECURRING TASKS FOUND: " + any_recurring.Count);

            var today = DateTime.Now;

            // create batches based on how old each task is. If there's a tie, sort by priority.
            var filtered_candidates = candidates
                // there should never be a null date time, but if there is, set the new due date to today, so nothing is hurt.
                .OrderBy(x =>
                    x.due.ToMaybe().Case(some: due => due.date.ToDateTime(today), none: () => today)
                )
                // order by priority (fixed so 4 from the API means 1 to me... API devs... I know, right?)
                .OrderBy(x => x.priority.FixPriorityBug().Id)
                // I shouldn't have to filter by 'is_recurring'.
                // I REALLY shouldn't have to...
                // but I am b/c the Todoist API team forgot to check !recurring & overdue use case (see: https://api.todoist.com/rest/v2/tasks?todos=&label=&filter=overdue%20&%20recurring).
                // so here it is:
                .If(include_non_recurring // && !exactly_equals_non_recurring
                    , tasks => tasks
                        .Where(x => x.due != null
                                    && !x.due.is_recurring.ToBoolean()
                                    || x.due == null
                        )
                )
                .Take(rescheduling_options.task_limit)
                .ToList();

            // if (debug) 
            Console.WriteLine("TOTAL Filtered candidates: " + filtered_candidates.Count);
            if (filtered_candidates.Count == 0)
            {
                Console.WriteLine("Nothing to do, so returning....");
                return new List<TodoistTask>();
            }

            var batches = filtered_candidates.Batch(rescheduling_options.daily_limit);

            if (debug) Console.WriteLine("Batches made : " + batches.Count());
            List<TodoistUpdates> actual_updates = new(0);
            foreach ((var todo_batch, int index) in batches.WithIndex())
            {
                foreach (var todo in todo_batch)
                {
                    // if (debug) Console.WriteLine($"old due date for task {todo?.content}: {todo.due.date}");
                    // if (debug) Console.WriteLine($"adding {index + 1} days!");
                    var updates = new TodoistUpdates()
                    {
                        id = todo.id,
                        content = todo.content,
                        description = todo.description,
                        priority = todo.priority,
                        labels = todo.labels,
                        due_date = today.AddDays(index + 1).ToString("o")
                    };

                    if (debug)
                    {
                        Console.WriteLine("new due date set to :" + updates.due_date);
                        Console.WriteLine("for task w/ priority :" + updates.priority.FixPriorityBug());
                    }

                    actual_updates.Add(updates);
                }
            }

            // string json = JsonConvert.SerializeObject(actual_updates);
            // string logfile = "reschedule_" + today.ToString("yy-MM-dd") + ".json";
            // string current_text = File.ReadAllText(logfile);
            // if (current_text.Length >= 10000)
            //     File.Delete(logfile);
            // File.AppendAllText(logfile, json);

            // actual_updates.Take(2).Dump("sample updates for filter " + rescheduling_options.filter);

            Console.WriteLine($"Total updated tasks: {filtered_candidates.Count}");
            // filtered_candidates.Dump(nameof(filtered_candidates));
            // Console.WriteLine("Total batches:" + batches.Count() );

            var updated_tasks = new List<TodoistTask>();
            if (@rescheduling_options.dry_run)
            {
                Console.WriteLine("Dry run enabled, so no updates");

                string save_json = JsonConvert.SerializeObject(actual_updates);
                string cwd = Directory.GetCurrentDirectory();
                var save = new SaveAs("rescheduler_plan.json")
                {
                    save_folder = cwd
                };
                FS.SaveAs(save, save_json);
            }
            else if (!rescheduling_options.dry_run)
            {
                await todoist.UpdateTodos(actual_updates);

                if (debug) Console.WriteLine($"Saving run ... '{rescheduling_options.name}'");
                await SaveRun(actual_updates, rescheduling_options);
            }

            return updated_tasks;
            // return default;
            // return candidates;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async ValueTask<bool> SaveRun(List<TodoistUpdates> actualUpdates, Reschedule reschedulingOptions)
    {
        string connectionString = SQLConnections.GetMySQLConnectionString();
        using var connection = new MySqlConnection(connectionString);
        string insert_query =
            @"insert into run_history (method_name, filter, created_by) values (@method_name, @filter, @created_by)";

        var results = await Dapper.SqlMapper
            .QueryAsync(connection, insert_query,
                new
                {
                    method_name = reschedulingOptions.name,
                    filter = reschedulingOptions.filter,
                    created_by = nameof(worker2)
                });

        return true;
    }

    // var createme = new TodoistUpdates()
    // {
    //     content = "Buy Milk zzz",
    //     due_string = "tomorrow at 12:00",
    //     priority = "4".FixPriorityBug().ToString()
    // };
    //
    // var created_todo = await todoist.CreateTodo(createme);
    // Console.WriteLine($"created todo {created_todo.content} with id:{created_todo.id}");

    // await TestDeletionById(created_todo);
}
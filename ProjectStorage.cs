using System.Text.Json;

namespace Prj {
    /// <summary>
    /// Owns the data/types/functions for projects and their children.
    /// </summary>
    class ProjectStorage {
        static public Dictionary<int, Project> Projects = [];

        public static void AddNewProject(string title, string? descr, int id) {
            Projects.Add(id, new Project(title, descr, [], [], DateTime.Now));
        }

        /// <summary>It's the responsibility of the caller to check that the
        /// supplied project index is valid</summary>
        public static void AddNewTask(int project, string text, int id) {
            Projects[project].Tasks.Add(new Task(text, DateTime.Now, false, id));
        }

        /// <summary>It's the responsibility of the caller to check that the
        /// supplied project index is valid</summary>
        public static void AddNewLog(int project, string text, int id) {
            Projects[project].Logs.Add(new Log(text, DateTime.Now, id));
        }

        public static int? FindVacantIdProject() {
            for (int i = 1; i < 999; i++) {
                if (!Projects.ContainsKey(i))
                    return i;
            }
            return null;
        }

        public static int? FindVacantIdTaskLog(Project project) {
            var usedIds = new HashSet<int>();
            foreach (var log in project.Logs)
                usedIds.Add(log.Id);
            foreach (var task in project.Tasks)
                usedIds.Add(task.Id);

            for (int i = 1; i < 999; i++) {
                if (!usedIds.Contains(i))
                    return i;
            }
            return null;
        }

        public class Project(string title, string? descr, List<Task> tasks, List<Log> logs, DateTimeOffset? lastChanged) {
            public string Title = title;
            public string? Descr = descr;
            public List<Task> Tasks = tasks;
            public List<Log> Logs = logs;
            public DateTimeOffset? LastChanged = lastChanged;

            public DateTimeOffset? DateCreated() {
                var oldestTask = Tasks.MinBy(i => i.Date)?.Date;
                var oldestLog = Logs.MinBy(i => i.Created)?.Created;
                return (oldestTask, oldestLog) switch {
                    (null, null) => LastChanged,
                    (null, { } log) => log,
                    ( { } task, null) => task,
                    ( { } task, { } log) => task > log ? log : task
                };
            }

            public int LogsTotal() => Logs.Count;
            public int TasksTotal() => Tasks.Count;
            public int TasksDone() => Tasks.Count(s => s.Done);
            public double CompletionPercent() {
                if (TasksTotal() == 0)
                    return 0;
                return TasksDone() / (float)TasksTotal() * 100;
            }

            public string DescrConcat(int threshold) {
                if (Descr != null) {
                    return Descr.Length <= threshold
                        ? Descr
                        : Descr[..threshold] + "...";
                }
                return "";
            }
        }

        public class Task(string text, DateTimeOffset date, bool done, int id) {
            public string Text { get; set; } = text;
            public DateTimeOffset Date { get; init; } = date;
            public bool Done { get; set; } = done;
            public int Id { get; init; } = id;

            public string TextConcat(int threshold) {
                if (Text != null) {
                    return Text.Length <= threshold
                        ? Text
                        : Text[..threshold] + "...";
                }
                return "";
            }
        }

        public class Log(string text, DateTimeOffset created, int id) {
            public string Text { get; set; } = text;
            public DateTimeOffset Created { get; init; } = created;
            public int Id { get; init; } = id;
        }

        static public bool SaveToFile() {
            if (Projects.Count == 0)
                return false;
            string? path = Utils.GetStoragePath();
            if (path == null) {
                Console.Error.WriteLine("Error: could not save to file, failed to fetch storage path");
                return false;
            }
            string jsonString = JsonSerializer.Serialize(Projects, Utils.JsonOptions);
            File.WriteAllText(path, jsonString);
            return true;
        }

        static public bool LoadFromFile() {
            string? path = Utils.GetStoragePath();
            if (path == null) {
                Console.Error.WriteLine("Error: failed to get storage path");
                return false;
            }
            else {
                if (!File.Exists(path)) {
                    if (!Utils.InitStorage())
                        return false;
                }

                string jsonString
                    = File.ReadAllText(path);
                if (jsonString.Length > 1) {
                    Dictionary<int, Project>? DeserProjects =
                        JsonSerializer.Deserialize<Dictionary<int, Project>>(jsonString, Utils.JsonOptions);
                    Projects = DeserProjects ?? throw new InvalidOperationException($"Failed to serialize storage file: '{path}'");
                }
                return true;

            }
        }
    }
}


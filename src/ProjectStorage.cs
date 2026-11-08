using System.Text.Json;

namespace Prj.src {
    /// <summary>
    /// Owns the data/types/functions for projects and their children.
    /// </summary>
    class ProjectStorage {
        static public Dictionary<int, Project> Projects = [];

        public static void AddNewProject(string title, string? descr, int id) {
            Projects.Add(id, new Project(title, descr, [], [], DateTime.Now));
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

        public class Project(string title, string? descr, List<Item> tasks, List<Item> logs, DateTimeOffset? lastChanged) {
            public string Title = title;
            public string? Descr = descr;
            public List<Item> Tasks = tasks;
            public List<Item> Logs = logs;
            public DateTimeOffset? LastChanged = lastChanged;

            public DateTimeOffset? DateCreated() {
                var oldestTask = Tasks.MinBy(i => i.Created)?.Created;
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
        }

        public class Item(string text, int id) {
            public string Text { get; set; } = text;
            public bool Done { get; set; } = false;
            public DateTimeOffset Created { get; init; } = DateTime.Now;
            public int Id { get; init; } = id;
        }

        static public bool SaveToFile() {
            if (Projects.Count == 0)
                return false;
            string? path = Util.GetStoragePath();
            if (path == null) {
                Console.Error.WriteLine("Error: could not save to file, failed to fetch storage path");
                return false;
            }
            string jsonString = JsonSerializer.Serialize(Projects, Util.JsonOptions);
            File.WriteAllText(path, jsonString);
            return true;
        }

        static public bool LoadFromFile() {
            string? path = Util.GetStoragePath();
            if (path == null) {
                Console.Error.WriteLine("Error: failed to get storage path");
                return false;
            }
            else {
                if (!File.Exists(path)) {
                    if (!Util.InitStorage())
                        return false;
                }

                string jsonString
                    = File.ReadAllText(path);
                if (jsonString.Length > 1) {
                    Dictionary<int, Project>? DeserProjects =
                        JsonSerializer.Deserialize<Dictionary<int, Project>>(jsonString, Util.JsonOptions);
                    Projects = DeserProjects ?? throw new InvalidOperationException($"Failed to serialize storage file: '{path}'");
                }
                return true;

            }
        }
    }
}


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

        /// <summary> DEBUG </summary>
        static public void DebugProjectsPrint() {
            foreach (KeyValuePair<int, Project> project in Projects) {
                Console.WriteLine($"------\nPROJECT: {project.Value.Title}");
                Console.WriteLine($"\tDESCR: {project.Value.Descr}");

            }
        }

        /// <summary> DEBUG </summary>
        static public void DebugPopulateProjects() {
            // NOTE: this function was generated by Proton Lumo
            Projects.Clear();

            Projects[1] = new Project(
                "Website Redesign",
                "Overhaul the marketing site. Target: end of quarter.",
                [
                    new("Draft new wireframes",       new(2026, 10, 05, 9, 0, 0, TimeSpan.FromHours(1)), true, 0),
                    new("Pick color palette",         new(2026, 10, 08, 14, 30, 0, TimeSpan.FromHours(1)), true, 1),
                    new("Implement landing page",     new(2026, 10, 20, 11, 15, 0, TimeSpan.FromHours(1)), false, 2),
                    new("Write copy for About page",  new(2026, 11, 01, 8, 45, 0, TimeSpan.FromHours(1)), false, 3),
                ],
                [
                    new(    "Agreed on scope: 5 pages, no CMS for now.",
                    new(2026, 10, 05, 10, 0, 0, TimeSpan.FromHours(1)), 4),
                    new("Client signed off on version 3.",
                    new(2026, 10, 12, 16, 20, 0, TimeSpan.FromHours(1)), 5),
                    new("Went with teal accents instead of orange. Jury is out.",
                    new(2026, 10, 18, 13, 5, 0, TimeSpan.FromHours(1)), 6),
                ],
                new(2026, 10, 18, 13, 5, 0, TimeSpan.FromHours(1)));

            Projects[2] = new Project(
                "Garage Sensor Firmware",
                "Ultrasonic distance sensor reporting parking position over MQTT.",
                [
                    new("Calibrate sensor at 2m range",
                    new(2026, 09, 14, 19, 0, 0, TimeSpan.FromHours(2)), true, 0),
                    new("Add deep-sleep mode",
                    new(2026, 09, 21, 21, 10, 0, TimeSpan.FromHours(2)), false, 1),
                    new("Test with car parked",
                    new(2026, 10, 02, 17, 40, 0, TimeSpan.FromHours(2)), false, 2),
                ],
                [
                    new("Blink LED on boot. It lives!",
                    new(2026, 09, 14, 19, 55, 0, TimeSpan.FromHours(2)), 3),
                    new("Range readings noisy",
                    new(2026, 09, 16, 20, 30, 0, TimeSpan.FromHours(2)), 4),
                ],
                new(2026, 09, 16, 20, 30, 0, TimeSpan.FromHours(2)));

            Projects[3] = new Project(
                "Meal Planning",
                null, // exercises the nullable Descr
                [
                    new("Weekly shop: October wk 43",
                    new(2026, 10, 19, 7, 30, 0, TimeSpan.FromHours(1)), true, 0),
                    new("Try lentil bolognese recipe",
                    new(2026, 10, 26, 7, 30, 0, TimeSpan.FromHours(1)), false, 1),
                ],
                [
                    new("Recipes moved to notebook" ,
                    new(2026, 10, 19, 8, 0, 0, TimeSpan.FromHours(1)), 2),
                ],
                new(2026, 10, 26, 8, 15, 0, TimeSpan.FromHours(1)));

            Projects[4] = new Project(
                "Empty Shell",
                "Edge case: project with data but zero tasks and zero logs.",
                [],
                [],
                new(2026, 10, 01, 12, 0, 0, TimeSpan.FromHours(1)));

            Projects[5] = new Project(
                "Learn Reactive Extensions",
                "Working through IntroToRx, one chapter per evening.",
                [
                    new("Chapter 1: Creating sequences",
                    new(2026, 11, 02, 20, 0, 0, TimeSpan.FromHours(1)), true, 0),
                    new("Chapter 2: Reducing a sequence",
                    new(2026, 11, 03, 20, 0, 0, TimeSpan.FromHours(1)), false, 1),
                ],
                [],
                new(2026, 11, 03, 21, 0, 0, TimeSpan.FromHours(1)));
        }

    }
}



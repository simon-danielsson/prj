namespace Prj.src {
    static class Color {
        private readonly static List<(string, string)> ColorList = [
            ("reset", "\x1b[0m"),
            ("bold", "\x1b[1m"),
            ("underline", "\x1b[4;1m"),
            ("overline", "\x1b[53;1m"),
            ("inverted", "\x1b[7;1m"),
            ("crossed", "\x1b[9;1m"),
            ("grey", "\x1B[38;2;95;95;99m")
                           ];

        public static string Ansi(string key) {
            (string, string) c = ColorList.Find(c => c.Item1 == key);
            if (c.Item2.Length > 0)
                return c.Item2;
            else
                throw new InvalidOperationException($"\nColor '{key}' not found");
        }
    }

    class Draw {
        private readonly int termWidth = Console.WindowWidth;

        public static void OverviewHeader() {
            Console.WriteLine(Color.Ansi("underline") +
                    string.Format("{0,-4}{1, -34}{2,-16}{3, -21}{4,-18}", "ID", "Title", "Completion", "Last changed", "Created") + Color.Ansi("reset"));
        }

        public static void OverviewProject(KeyValuePair<int, ProjectStorage.Project> p) {
            Console.WriteLine(string.Format("{0,-4}{1,-34}{2, -16}{3,-21}{4,-4}",
                p.Key,
                Util.Concat(p.Value.Title, 30),
                $"{p.Value.TasksDone()}/{p.Value.TasksTotal()} - {p.Value.CompletionPercent():F0}%",
                Util.TimeDisplay(p.Value.LastChanged),
                Util.TimeDisplay(p.Value.DateCreated())
                ));
        }

        public static void LogHeader() {
            Console.WriteLine(Color.Ansi("underline") +
                    string.Format("{0,-93}", "Logs") + Color.Ansi("reset"));
        }

        public static void TaskHeader() {
            Console.WriteLine(Color.Ansi("underline") +
        string.Format("{0,-93}", "Tasks") + Color.Ansi("reset"));
        }

        public static void Log(ProjectStorage.Item item) {
            Console.Write(Color.Ansi("bold"));
            string line = new('-', 71);
            Console.Write(
                string.Format("{0,-3}{1,0}{2,4}", item.Id, line, " " + Util.TimeDisplay(item.Created)));
            Console.Write(Color.Ansi("reset"));
            Console.WriteLine("\n" +
                string.Format(item.Text));
        }

        public static void Task(ProjectStorage.Item item) {
            Console.Write(
                string.Format("{0,-3}{1,-2}", item.Id, "│"));
            if (item.Done)
                Console.Write(Color.Ansi("crossed"));
            Console.Write(string.Format("{0,-69}", Util.Concat(item.Text, 65)));
            Console.Write(Color.Ansi("reset"));
            Console.WriteLine(" " + Util.TimeDisplay(item.Created));
        }

        public static void ProjectHeader(ProjectStorage.Project project, int projectId) {
            Console.WriteLine(Color.Ansi("inverted") +
                    string.Format("{0,-93}", $"{projectId}: " + project.Title) + Color.Ansi("reset"));
            if (project.Descr != null && project.Descr.Length > 0)
                Console.WriteLine(project.Descr);
            Console.WriteLine();
        }

    }

}

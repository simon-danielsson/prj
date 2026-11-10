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
        private static readonly int termWidth = Console.WindowWidth;

        private static readonly int wOverviewId = 4;
        private static readonly int wOverviewCompl = termWidth / 8;
        private static readonly int wOverviewChanged = 20;
        private static readonly int wOverviewCreated = 20;

        public static void OverviewHeader() {
            int wOverviewTitle = Math.Max(termWidth - 1 - (wOverviewId + wOverviewCompl + wOverviewChanged + wOverviewCreated), "Title".Length + 1);

            string row =
                  "ID".PadRight(wOverviewId)
                + "Title".PadRight(wOverviewTitle)
                + "Completion".PadRight(wOverviewCompl)
                + "Last changed".PadRight(wOverviewChanged)
                + "Created".PadRight(wOverviewCreated);

            Console.Write(Color.Ansi("underline"));
            Console.WriteLine(row);
            Console.Write(Color.Ansi("reset"));
        }

        public static void OverviewProject(KeyValuePair<int, ProjectStorage.Project> p) {

            string text = Util.Concat(p.Value.Title, termWidth - 5 - (wOverviewId + wOverviewCompl + wOverviewChanged + wOverviewCreated));

            int wOverviewTitle = Math.Max(termWidth - 1 - (wOverviewId + wOverviewCompl + wOverviewChanged + wOverviewCreated), text.Length + 1);

            string row =
                  $"{p.Key}".PadRight(wOverviewId)
                + text.PadRight(wOverviewTitle)
                + $"{p.Value.TasksDone()}/{p.Value.TasksTotal()} - {p.Value.CompletionPercent():F0}%".PadRight(wOverviewCompl)
                + Util.TimeDisplay(p.Value.LastChanged).PadRight(wOverviewChanged)
                + Util.TimeDisplay(p.Value.DateCreated()).PadRight(wOverviewCreated);
            Console.WriteLine(row);
        }

        public static void LogHeader() {
            Console.Write(Color.Ansi("underline"));
            Console.WriteLine("Logs".PadRight(termWidth));
            Console.Write(Color.Ansi("reset"));
        }

        public static void TaskHeader() {
            Console.Write(Color.Ansi("underline"));
            Console.WriteLine("Tasks".PadRight(termWidth));
            Console.Write(Color.Ansi("reset"));
        }

        public static void Log(ProjectStorage.Item item) {
            Console.Write(Color.Ansi("bold"));

            int wId = 3;
            string created = Util.TimeDisplay(item.Created);
            int wLine = Math.Max(termWidth - 1 - (wOverviewCreated + wId - 1), 1);

            string line = new('-', wLine);

            string row = $"{item.Id}".PadRight(wId) + line.PadRight(wLine) + created.PadLeft(wOverviewCreated - 1);

            Console.WriteLine(row);
            Console.Write(Color.Ansi("reset"));

            Console.WriteLine(
                string.Format(item.Text));
        }

        public static void Task(ProjectStorage.Item item) {
            int wId = 3;
            int wIdDiv = 2;
            string created = Util.TimeDisplay(item.Created);
            string text = Util.Concat(item.Text, termWidth - 5 - (wOverviewCreated - 1 + wId + wIdDiv));
            int wText = Math.Max(termWidth - 1 - (wOverviewCreated - 1 + wId + wIdDiv), text.Length + 1);

            string row = $"{item.Id}".PadRight(wId) + "│".PadRight(wIdDiv) + (item.Done ? Color.Ansi("crossed") : "") + text.PadRight(wText) + Color.Ansi("reset") + created.PadLeft(wOverviewCreated - 1);

            Console.WriteLine(row);
            Console.Write(Color.Ansi("reset"));
        }

        public static void ProjectHeader(ProjectStorage.Project project, int projectId) {
            Console.Write(Color.Ansi("inverted"));
            Console.WriteLine($"{projectId}: {project.Title}".PadRight(termWidth));
            Console.Write(Color.Ansi("reset"));
            if (project.Descr != null && project.Descr.Length > 0)
                Console.WriteLine(project.Descr);
            Console.WriteLine();
        }

    }

}

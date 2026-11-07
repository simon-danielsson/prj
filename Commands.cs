namespace Prj {
    /// <summary>
    /// Handling and execution of program commands (hit after Program.cs)
    /// </summary>
    static class Commands {
        private static void CmdList(int _) {
            if (ProjectStorage.Projects.Count == 0) {
                Console.WriteLine("No projects were found!");
                return;
            }

            // TODO: sort with most recent projects at the bottom

            Console.WriteLine(Utils.Color("underline") +
                    string.Format("{0,-6}{1, -30}{2, -18}{3,-30}{4,-33}", "ID", "Created", "Completion", "Title", "Description") + Utils.Color("reset"));

            foreach (KeyValuePair<int, ProjectStorage.Project> p in ProjectStorage.Projects) {

                Console.WriteLine(
                    string.Format("{0,-6}{1,-30}{2,-18}{3,-30}{4,0}", p.Key, Utils.TimeDisplay(p.Value.DateCreated() ?? DateTime.Now), $"{p.Value.TasksDone()}/{p.Value.TasksTotal()} ({p.Value.CompletionPercent():F1}%)", p.Value.Title, p.Value.DescrConcat(30)));
            }
        }

        private static void CmdStat(int id) {
            if (id >= 0 && ProjectStorage.Projects.TryGetValue(id, out var row)) {

                Console.WriteLine(Utils.Color("bold") +
                        $"{id} - {row.Title}" + Utils.Color("reset"));
                if (row.Descr != null && row.Descr.Length > 0)
                    Console.WriteLine(row.Descr);

                Console.WriteLine();

                if (row.Tasks.Length > 0) {
                    Console.WriteLine(Utils.Color("underline") +
                            string.Format("{0,-6}{1,-8}{2,-34}{3,-69}", "ID", "Done", "Created", "Text") + Utils.Color("reset"));

                    // TODO: sort with most recent tasks at the bottom
                    foreach (ProjectStorage.Task task in row.Tasks) {
                        Console.Write(
                            string.Format("{0,-6}", task.Id));

                        if (task.Done)
                            Console.Write(
                                string.Format("{0,-8}", "X"));
                        else
                            Console.Write(
                                string.Format("{0,-8}", "."));

                        Console.WriteLine(
                            string.Format("{0,-34}{1,0}", Utils.TimeDisplay(task.Date), task.Text));
                    }
                }
                else {
                    Console.WriteLine("No tasks were found!");
                }

                Console.WriteLine();
                // Console.WriteLine(Utils.Color("bold") + "\nLOGS\n", Utils.Color("reset"));

                if (row.Logs.Length > 0) {
                    Console.WriteLine(Utils.Color("underline") +
                            string.Format("{0,-14}{1,-34}{2,-69}", "ID", "Created", "Text") + Utils.Color("reset"));

                    // TODO: sort with most recent logs at the bottom
                    foreach (ProjectStorage.Log log in row.Logs) {
                        Console.WriteLine(
                            string.Format("{0,-14}{1,-34}{2,0}", log.Id, Utils.TimeDisplay(log.Created), log.Text));

                    }
                }
                else {
                    Console.WriteLine("No logs were found!");
                }

            }
            else {
                Utils.MsgErr($"project id '{id}' does not exist");
            }
        }

        private static void CmdHelp(int _) {
            string helpTxt = Utils.GetEmbeddedContent("help.txt");
            Console.Write($"{helpTxt}");

            foreach (KeyValuePair<string, CmdTableRow> row in CmdTable) {
                Console.Write(
                    string.Format("{0,-8}", row.Key));
                Console.Write(
                    string.Format("{0,-8}", row.Value.Arg));
                Console.WriteLine(
                    string.Format("{1,0}", row.Key, row.Value.Descr));
            }
        }

        public record CmdTableRow(string Descr, string? Arg, bool HasArg, Action<int> Function);

        /// <summary>
        /// Dict. containing program commands with respective names,
        /// descriptions and functions.
        /// </summary>
        static public Dictionary<string, CmdTableRow> CmdTable = new(){
                {
                    "help", new("Display help.", null, false, CmdHelp)
                },
                {
                    "ls", new("List all projects.", null, false, CmdList)
                },
                {
                    "stat", new("Display project overview.", "<id>", true, CmdStat)
                },
            };

    }
}

/*

USAGE:
    prj <command> [args]

COMMANDS:
    ls                                 List all projects
    stat    <id>                       Show project overview: description,
                                       lists with completion, overall %, last log
    desc    <id> [text...]             Show or set a project description
    list    <id> add <listname>        Add a todo list to a project
    item    <id> <listname>            Item subcommands (below)
    log     <id> [text...]             Append a log entry; no text opens $EDITOR
            (<id> --show [n])          Print log entries (last n, default all)
    status  <id> <status>              Set status: active|paused|finished|archived
    export  <id> --format=<fmt>        Export project: json|md
                                       (--out=<file> to write to disk)
    help                               Show this help, or help for a command
    path                               Print path to wlk central storage.

ITEM SUBCOMMANDS:
    wlk item <id> <listname> add <text...>      Add a todo item
    wlk item <id> <listname> done <index>       Mark an item done
    wlk item <id> <listname> undo <index>       Mark an item not done
    wlk item <id> <listname> rm   <index>       Remove an item

EXAMPLES:
    wlk init parser-refactor
    wlk item 3 "error handling" add "rewrite error struct"
    wlk item 3 "error handling" done 1
    wlk log 3 "arena allocator leak finally fixed"
    wlk stat 3
    wlk export 3 --format=md --out=WALKTHROUGH.md

STORAGE:
    Central storage only. Use command 'path' to print its path.

    */

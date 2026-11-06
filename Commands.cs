namespace Wlk {
    /// <summary>
    /// Handling and execution of program commands (hit after Program.cs)
    /// </summary>
    static class Commands {
        private static void CmdList(string _) {
            if (ProjectStorage.Projects.Count == 0) {
                Console.WriteLine("No projects were found!");
                return;
            }

            foreach (KeyValuePair<int, ProjectStorage.Project> p in ProjectStorage.Projects) {
                Console.Write($"[{p.Key}] {p.Value.Title}");
                if (p.Value.Descr != null && p.Value.Descr.Length != 0)
                    Console.WriteLine($"{Utils.Color("grey")} - {p.Value.Descr}{Utils.Color("reset")}");
                else
                    Console.WriteLine();
            }
        }

        private static void CmdHelp(string _) {
            string helpTxt = Utils.GetEmbeddedContent("help.txt");
            Console.Write($"{helpTxt}");

            foreach (KeyValuePair<string, CmdTableRow> row in CmdTable) {
                Console.WriteLine(
                    string.Format("{0,-16}{1,0}", row.Key, row.Value.Descr));
            }
        }

        public record CmdTableRow(string Descr, Action<string> Function);

        /// <summary>
        /// Dict. containing program commands with respective names,
        /// descriptions and functions.
        /// </summary>
        static public Dictionary<string, CmdTableRow> CmdTable = new(){
                {
                    "help", new("Display help.", CmdHelp)
                },
                {
                    "ls", new("List all projects.", CmdList)
                },
            };

    }
}

/*
wlk 0.1.0 — personal project walker

USAGE:
    wlk <command> [args]

COMMANDS:
    init    <name>                     Create a new project
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

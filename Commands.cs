using System.Text.RegularExpressions;
#pragma warning disable SYSLIB1045

namespace Prj {

    /// <summary>
    /// Handling and execution of program commands (hit after Program.cs)
    /// </summary>
    static class Commands {
        private static void CmdList(string? _) {
            if (ProjectStorage.Projects.Count == 0) {
                Console.WriteLine("No projects were found!");
                return;
            }

            Utils.MsgInfo("ordered by 'Last changed' ascending.\n");
            Console.WriteLine(Utils.Color("underline") +
                    string.Format("{0,-4}{1, -34}{2,-16}{3, -21}{4,-18}", "ID", "Title", "Completion", "Last changed", "Created") + Utils.Color("reset"));

            foreach (var p in ProjectStorage.Projects.OrderBy(kvp => kvp.Value.LastChanged)) {
                Console.WriteLine(
                    string.Format("{0,-4}{1,-34}{2, -16}{3,-21}{4,-4}",
                        p.Key,
                        p.Value.TitleConcat(30),
                        $"{p.Value.TasksDone()}/{p.Value.TasksTotal()} - {p.Value.CompletionPercent():F0}%",
                        Utils.TimeDisplay(p.Value.LastChanged),
                        Utils.TimeDisplay(p.Value.DateCreated())
                        ));
            }
        }

        private static void CmdNotImpl(string? _) {
            Utils.MsgErr("this command is not implemented yet");
        }

        private static void CmdNew(string? _) {
            int? newId = ProjectStorage.FindVacantIdProject();
            if (newId == null) {
                Utils.MsgErr("project limit has been reached");
                return;
            }

            string? title = null;
            string? descr = null;
            string? edOutput = Utils.EditInEditor(Utils.GetEmbeddedContent("editorNewProject.txt"));
            if (edOutput is string userContent) {
                // Excuse: I needed to use Regex here since the regular Split method had no
                // overload for keeping the split delimeters.
                foreach (string s in Regex.Split(userContent, @"(\r?\n)")) {
                    if (title == null) {
                        if (!s.IsWhiteSpace() && title == null)
                            title = s.Trim();
                    }
                    else {
                        descr += s;
                    }
                }
            }
            else if (edOutput == null || title == null) {
                Utils.MsgErr("failed to create project due to null editor output.");
            }

            // Console.WriteLine($"new id: {newId}");
            if (descr != null)
                descr = descr.Trim();
            if (title != null && newId is int id) {
                ProjectStorage.AddNewProject(title, descr, id);
                Utils.MsgInfo($"new project with id '{id}' was created successfully.");
            }
        }

        // TODO: edit formatting of task/log lists to allow for larger bodies in project overview
        private static void CmdView(string? id_raw) {
            int id = CmdArgParse(id_raw)[0];

            if (id >= 0 && ProjectStorage.Projects.TryGetValue(id, out var row)) {

                Console.WriteLine(Utils.Color("bold") +
                        $"{id} - {row.Title}" + Utils.Color("reset"));
                if (row.Descr != null && row.Descr.Length > 0)
                    Console.WriteLine(row.Descr);

                Console.WriteLine();

                Console.WriteLine(Utils.Color("underline") +
                        string.Format("{0,-93}", "Logs") + Utils.Color("reset"));
                if (row.Logs.Length > 0) {
                    // TODO: sort with most recent logs at the bottom
                    foreach (var log in row.Logs.OrderBy(t => t.Created)) {
                        Console.WriteLine(Utils.Color("overline") +
                            string.Format($"[{log.Id}] {Utils.TimeDisplay(log.Created)}") + Utils.Color("reset"));
                        Console.WriteLine(
                            string.Format(log.Text));

                    }
                }
                else {
                    Console.WriteLine("No logs were found!");
                }
                Console.WriteLine();

                Console.WriteLine(Utils.Color("underline") +
                        string.Format("{0,-93}", "Tasks") + Utils.Color("reset"));
                if (row.Tasks.Length > 0) {
                    foreach (var task in row.Tasks.OrderByDescending(t => t.Done).ThenBy(t => t.Date)) {
                        Console.Write(
                            string.Format($"[{task.Id}] "));
                        if (task.Done)
                            Console.Write(Utils.Color("crossed"));
                        Console.Write(
                            string.Format($"{task.Text} ({Utils.TimeDisplay(task.Date)})"));
                        Console.WriteLine(Utils.Color("reset"));
                    }
                }
                else {
                    Console.WriteLine("No tasks were found!");
                }

            }
            else {
                Utils.MsgErr($"project id '{id}' does not exist");
            }
        }

        private static void CmdHelp(string? _) {
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

        public record CmdTableRow(string Descr, string? Arg, bool HasArg, Action<string?> Function);

        /// <summary>parse unknown arg string? like '1' and '2-3' into [1] and [2,3]</summary>
        static public int[] CmdArgParse(string? raw_arg) {
            if (raw_arg is string arg) {
                if (arg.Contains('-')) {
                    return [.. arg.Split('-').Select(int.Parse)];
                }
                else {
                    return [Convert.ToInt32(arg)];
                }
            }
            else {
                return [];
            }
        }

        /// <summary>
        /// Dict. containing program commands with respective names,
        /// descriptions and functions.
        /// </summary>
        static public Dictionary<string, CmdTableRow> CmdTable = new(){
                {
                    "help", new("Display help", null, false, CmdHelp)
                },
                {
                    "ls", new("List all projects", null, false, CmdList)
                },
                {
                    "new", new("Create a new project", null, false, CmdNew)
                },
                {
                    "pr", new("Display a project overview", "<id>", true, CmdView)
                },
                {
                    // TODO: implement CmdTask
                    "task", new("Create new task in a project -- ex. (new task in project 2): prj task 2", "<id>", true, CmdNotImpl)
                },
                {
                    // TODO: implement CmdLog
                    "log", new("Write a new log in a project -- ex. (new log in project 8): prj log 8", "<id>", true, CmdNotImpl)
                },
                {
                    // TODO: implement CmdFlip
                    "flip", new("Flip task state -- ex. (task 5 in project 12): prj flip 12-5", "<id>", true, CmdNotImpl)
                },
                {
                    // TODO: implement CmdEdit
                    "edit", new("Edit a project, log or task -- ex. (task/log 1 in project 3): prj edit 3-1", "<id>", true, CmdNotImpl)
                },
                {
                    // TODO: implement CmdRemove
                    "rm", new("Remove an item -- ex. (remove task 2 in project 1): prj rm 1-2", "<id>", true, CmdNotImpl)
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

    edit    <id> [text...]             Show or set a project description

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

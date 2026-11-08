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
                if (row.Logs.Count > 0) {
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
                if (row.Tasks.Count > 0) {
                    foreach (var task in row.Tasks.OrderByDescending(t => t.Done).ThenBy(t => t.Date)) {
                        Console.Write(
                            string.Format($"[{task.Id}] "));
                        if (task.Done)
                            Console.Write(Utils.Color("crossed"));
                        Console.Write(
                            string.Format("{0,-71}{1,0}", task.Text, Utils.TimeDisplay(task.Date)));
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

        private static void CmdTask(string? idRaw) {
            int[] ids = CmdArgParse(idRaw);
            if (ProjectStorage.Projects.TryGetValue(ids[0], out var project)) {
                int? newId = ProjectStorage.FindVacantIdTaskLog(project);
                if (newId == null) {
                    Utils.MsgErr($"task/log limit for project '{ids[0]}'has been reached");
                    return;
                }

                string? text = null;
                string? edOutput = Utils.EditInEditor("");
                if (edOutput is string userContent && newId is int id) {
                    ProjectStorage.AddNewTask(ids[0], userContent.Split('\n').First(), id);
                    Utils.MsgInfo($"new task '{id}' in project '{ids[0]}' was created successfully.");
                }
                else if (edOutput == null || text == null) {
                    Utils.MsgErr("failed to create task due to null editor output.");
                }
            }
            else {
                Utils.MsgErr($"project '{ids[0]}' does not exist");
            }
        }

        private static void CmdLog(string? idRaw) {
            int[] ids = CmdArgParse(idRaw);
            if (ProjectStorage.Projects.TryGetValue(ids[0], out var project)) {
                int? newId = ProjectStorage.FindVacantIdTaskLog(project);
                if (newId == null) {
                    Utils.MsgErr($"task/log limit for project '{ids[0]}'has been reached");
                    return;
                }

                string? text = null;
                string? edOutput = Utils.EditInEditor("");
                if (edOutput is string userContent && newId is int id) {
                    ProjectStorage.AddNewLog(ids[0], userContent.Trim(), id);
                    Utils.MsgInfo($"new log '{id}' in project '{ids[0]}' was created successfully.");
                }
                else if (edOutput == null || text == null) {
                    Utils.MsgErr("failed to create log due to null editor output.");
                }
            }
            else {
                Utils.MsgErr($"project '{ids[0]}' does not exist");
            }
        }

        private static void CmdFlip(string? idRaw) {
            int[] ids = CmdArgParse(idRaw);
            if (ProjectStorage.Projects.TryGetValue(ids[0], out var project)) {
                if (project.Tasks.Count == 0) {
                    Utils.MsgErr($"project {ids[0]} has no tasks");
                    return;
                }

                foreach (ProjectStorage.Task task in project.Tasks) {
                    if (task.Id == ids[1]) {
                        task.Done = !task.Done;
                        if (task.Done == true)
                            Utils.MsgInfo($"task '{task.Id}' in project '{ids[0]}' is done!");
                        else
                            Utils.MsgInfo($"task '{task.Id}' in project '{ids[0]}' needs more work!");
                        return;
                    }
                }
                Utils.MsgErr($"task '{ids[1]}' does not exist");
            }
            else {
                Utils.MsgErr($"project '{ids[0]}' does not exist");
            }
        }

        private static void CmdRemove(string? idRaw) {
            int[] ids = CmdArgParse(idRaw);

            // project
            if (ids.Length == 1) {
                if (ProjectStorage.Projects.Remove(ids[0]))
                    Utils.MsgInfo($"project '{ids[0]}' was successfully removed.");
                else
                    Utils.MsgInfo($"project '{ids[0]}' could not be found.");
                return;
            }

            // task/log in project
            if (ids.Length == 2) {
                if (ProjectStorage.Projects.TryGetValue(ids[0], out var project)) {
                    if (project.Logs.RemoveAll(t => t.Id == ids[1]) > 0) {
                        Utils.MsgInfo($"log '{ids[1]}' was successfully removed from project {ids[0]}.");
                        return;
                    }
                    else if (project.Tasks.RemoveAll(t => t.Id == ids[1]) > 0) {
                        Utils.MsgInfo($"task '{ids[1]}' was successfully removed from project {ids[0]}.");
                        return;
                    }
                    else {
                        Utils.MsgInfo($"a task or log with id '{ids[0]}' could not be found.");
                    }
                }
                else {
                    Utils.MsgInfo($"project '{ids[0]}' could not be found.");
                }

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
        static public int[] CmdArgParse(string? rawArg) {
            if (rawArg is string arg) {
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
                    "task", new("Create new task in a project -- ex. (new task in project 2): prj task 2", "<id>", true, CmdTask)
                },
                {
                    "log", new("Write a new log in a project -- ex. (new log in project 8): prj log 8", "<id>", true, CmdLog)
                },
                {
                    "flip", new("Flip task state -- ex. (task 5 in project 12): prj flip 12-5", "<id>", true, CmdFlip)
                },
                {
                    // TODO: implement CmdEdit
                    "edit", new("Edit a project, log or task -- ex. (task/log 1 in project 3): prj edit 3-1", "<id>", true, CmdNotImpl)
                },
                {
                    "rm", new("Remove an item -- ex. (remove task 2 in project 1): prj rm 1-2", "<id>", true, CmdRemove)
                },
            };

    }
}


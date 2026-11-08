using System.Text.RegularExpressions;
#pragma warning disable SYSLIB1045

// TODO: there is currently quite a lot of messy and duplicate code in this document. It's going to require substantial refactoring.

namespace Prj.src {

    /// <summary>
    /// Handling and execution of program commands (hit after Program.cs)
    /// </summary>
    static class Commands {
        private static void CmdList(string? idRaw) {
            if (idRaw is string id) {
                CmdListProject(idRaw);
                return;
            }

            if (ProjectStorage.Projects.Count == 0) {
                Console.WriteLine("No projects were found!");
                return;
            }

            Msg.Info("ordered by 'Last changed' ascending.\n");
            Console.WriteLine(Color.Ansi("underline") +
                    string.Format("{0,-4}{1, -34}{2,-16}{3, -21}{4,-18}", "ID", "Title", "Completion", "Last changed", "Created") + Color.Ansi("reset"));

            foreach (var p in ProjectStorage.Projects.OrderBy(kvp => kvp.Value.LastChanged)) {
                Console.WriteLine(
                    string.Format("{0,-4}{1,-34}{2, -16}{3,-21}{4,-4}",
                        p.Key,
                        Util.Concat(p.Value.Title, 30),
                        $"{p.Value.TasksDone()}/{p.Value.TasksTotal()} - {p.Value.CompletionPercent():F0}%",
                        Util.TimeDisplay(p.Value.LastChanged),
                        Util.TimeDisplay(p.Value.DateCreated())
                        ));
            }
        }

        private static void CmdListProject(string? idRaw) {
            int id = -1;
            if (idRaw != null)
                id = CmdArgParse(idRaw)[0];
            else {
                Msg.Error("no argument provided");
                return;
            }

            if (id >= 0 && ProjectStorage.Projects.TryGetValue(id, out var row)) {

                Console.WriteLine(Color.Ansi("underline") +
                        $"{row.Title}" + Color.Ansi("reset"));
                if (row.Descr != null && row.Descr.Length > 0)
                    Console.WriteLine(row.Descr);

                Console.WriteLine();

                Console.WriteLine(Color.Ansi("underline") +
                        string.Format("{0,-93}", "Logs") + Color.Ansi("reset"));
                if (row.Logs.Count > 0) {
                    foreach (var log in row.Logs.OrderBy(t => t.Created)) {
                        Console.WriteLine(Color.Ansi("bold") +
                            string.Format($"[{log.Id}] {Util.TimeDisplay(log.Created)}") + Color.Ansi("reset"));
                        Console.WriteLine(
                            string.Format(log.Text));

                    }
                }
                else {
                    Console.WriteLine("No logs were found!");
                }
                Console.WriteLine();

                Console.WriteLine(Color.Ansi("underline") +
                        string.Format("{0,-93}", "Tasks") + Color.Ansi("reset"));

                if (row.Tasks.Count > 0) {
                    foreach (var task in row.Tasks.OrderByDescending(t => t.Done).ThenBy(t => t.Date)) {
                        Console.Write(
                            string.Format($"[{task.Id}] "));
                        if (task.Done)
                            Console.Write(Color.Ansi("crossed"));
                        Console.Write(
                            string.Format("{0,-71}{1,0}", Util.Concat(task.Text, 67), Util.TimeDisplay(task.Date)));
                        Console.WriteLine(Color.Ansi("reset"));
                    }
                }
                else {
                    Console.WriteLine("No tasks were found!");
                }

            }
            else {
                Msg.Error($"project id '{id}' does not exist");
            }
        }

        private static void CmdNew(string? _) {
            int? newId = ProjectStorage.FindVacantIdProject();
            if (newId == null) {
                Msg.Error("project limit has been reached");
                return;
            }

            string? title = null;
            string? descr = null;
            string? edOutput = Util.EditInEditor(Util.GetEmbeddedContent("editorNewProject.txt"));
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
                Msg.Error("failed to create project due to null editor output.");
            }

            // Console.WriteLine($"new id: {newId}");
            if (descr != null)
                descr = descr.Trim();
            if (title != null && newId is int id) {
                ProjectStorage.AddNewProject(title, descr, id);
                Msg.Info($"new project with id '{id}' was created successfully.");
            }
        }

        private static void CmdTask(string? idRaw) {
            int[] ids = [];
            if (idRaw != null)
                ids = CmdArgParse(idRaw);
            else {
                Msg.Error("no argument provided");
                return;
            }
            if (ProjectStorage.Projects.TryGetValue(ids[0], out var project)) {
                int? newId = ProjectStorage.FindVacantIdTaskLog(project);
                if (newId == null) {
                    Msg.Error($"task/log limit for project '{ids[0]}'has been reached");
                    return;
                }

                string? text = null;
                string? edOutput = Util.EditInEditor("");
                if (edOutput is string userContent && newId is int id) {
                    ProjectStorage.AddNewTask(ids[0], userContent.Split('\n').First(), id);
                    Msg.Info($"new task '{id}' in project '{ids[0]}' was created successfully.");
                    project.LastChanged = DateTime.Now;
                }
                else if (edOutput == null || text == null) {
                    Msg.Error("failed to create task due to null editor output.");
                }
            }
            else {
                Msg.Error($"project '{ids[0]}' does not exist");
            }
        }

        private static void CmdLog(string? idRaw) {
            int[] ids = [];
            if (idRaw != null)
                ids = CmdArgParse(idRaw);
            else {
                Msg.Error("no argument provided");
                return;
            }
            if (ProjectStorage.Projects.TryGetValue(ids[0], out var project)) {
                int? newId = ProjectStorage.FindVacantIdTaskLog(project);
                if (newId == null) {
                    Msg.Error($"task/log limit for project '{ids[0]}'has been reached");
                    return;
                }

                string? text = null;
                string? edOutput = Util.EditInEditor("");
                if (edOutput is string userContent && newId is int id) {
                    ProjectStorage.AddNewLog(ids[0], userContent.Trim(), id);
                    Msg.Info($"new log '{id}' in project '{ids[0]}' was created successfully.");
                    project.LastChanged = DateTime.Now;
                }
                else if (edOutput == null || text == null) {
                    Msg.Error("failed to create log due to null editor output.");
                }
            }
            else {
                Msg.Error($"project '{ids[0]}' does not exist");
            }
        }

        private static void CmdFlip(string? idRaw) {
            int[] ids = [];
            if (idRaw != null)
                ids = CmdArgParse(idRaw);
            else {
                Msg.Error("no argument provided");
                return;
            }
            if (ProjectStorage.Projects.TryGetValue(ids[0], out var project)) {
                if (project.Tasks.Count == 0) {
                    Msg.Error($"project {ids[0]} has no tasks");
                    return;
                }

                foreach (ProjectStorage.Task task in project.Tasks) {
                    if (task.Id == ids[1]) {
                        task.Done = !task.Done;
                        if (task.Done == true)
                            Msg.Info($"task '{task.Id}' in project '{ids[0]}' is done!");
                        else
                            Msg.Info($"task '{task.Id}' in project '{ids[0]}' needs more work!");
                        project.LastChanged = DateTime.Now;
                        return;
                    }
                }
                Msg.Error($"task '{ids[1]}' does not exist");
            }
            else {
                Msg.Error($"project '{ids[0]}' does not exist");
            }
        }

        private static void CmdRemove(string? idRaw) {
            int[] ids = [];
            if (idRaw != null)
                ids = CmdArgParse(idRaw);
            else {
                Msg.Error("no argument provided");
                return;
            }

            // project
            if (ids.Length == 1) {
                if (ProjectStorage.Projects.Remove(ids[0]))
                    Msg.Info($"project '{ids[0]}' was successfully removed.");
                else
                    Msg.Info($"project '{ids[0]}' could not be found.");
                return;
            }

            // task/log in project
            if (ids.Length == 2) {
                if (ProjectStorage.Projects.TryGetValue(ids[0], out var project)) {
                    if (project.Logs.RemoveAll(t => t.Id == ids[1]) > 0) {
                        project.LastChanged = DateTime.Now;
                        Msg.Info($"log '{ids[1]}' was successfully removed from project '{ids[0]}'.");
                        return;
                    }
                    else if (project.Tasks.RemoveAll(t => t.Id == ids[1]) > 0) {
                        project.LastChanged = DateTime.Now;
                        Msg.Info($"task '{ids[1]}' was successfully removed from project '{ids[0]}'.");
                        return;
                    }
                    else {
                        Msg.Info($"a task or log with id '{ids[0]}' could not be found.");
                    }
                }
                else {
                    Msg.Info($"project '{ids[0]}' could not be found.");
                }

            }
        }

        private static void CmdEdit(string? idRaw) {
            int[] ids = [];
            if (idRaw != null)
                ids = CmdArgParse(idRaw);
            else {
                Msg.Error("no argument provided");
                return;
            }

            // project
            if (ids.Length == 1) {
                if (ProjectStorage.Projects.TryGetValue(ids[0], out var project)) {
                    string oldContent = project.Title + "\n\n" + project.Descr;
                    string? title = null, descr = null;
                    string? edOutput = Util.EditInEditor(oldContent);
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
                        Msg.Error("failed to create project due to null editor output");
                    }

                    // Console.WriteLine($"new id: {newId}");
                    if (descr != null)
                        project.Descr = descr.Trim();
                    if (title != null) {
                        project.Title = title.Trim();
                    }
                    else {
                        Msg.Info($"no new title was given - edit was cancelled");
                    }
                    project.LastChanged = DateTime.Now;
                    Msg.Info($"project '{ids[0]}' was edited successfully");

                }
                else {
                    Msg.Info($"project '{ids[0]}' could not be found");
                }
                return;
            }

            // task/log in project
            if (ids.Length == 2) {
                if (ProjectStorage.Projects.TryGetValue(ids[0], out var project)) {
                    int listIndex;
                    if ((listIndex = project.Tasks.FindIndex(t => t.Id == ids[1])) >= 0) {
                        string? edOutput = Util.EditInEditor(project.Tasks[listIndex].Text);
                        if (edOutput is string userContent) {
                            project.Tasks[listIndex].Text = userContent.Split('\n').First();
                            project.LastChanged = DateTime.Now;
                            Msg.Info($"task '{ids[1]}' in project '{ids[0]}' was edited successfully");
                            return;
                        }
                        else if (edOutput == null) {
                            Msg.Error("failed to edit task due to null editor output");
                            return;
                        }
                    }
                    else if ((listIndex = project.Logs.FindIndex(t => t.Id == ids[1])) >= 0) {
                        string? edOutput = Util.EditInEditor(project.Logs[listIndex].Text);
                        if (edOutput is string userContent) {
                            project.Logs[listIndex].Text = userContent.Split('\n').First();
                            project.LastChanged = DateTime.Now;
                            Msg.Info($"log '{ids[1]}' in project '{ids[0]}' was edited successfully");
                            return;
                        }
                        else if (edOutput == null) {
                            Msg.Error("failed to edit task due to null editor output");
                            return;
                        }

                    }

                }
                else {
                    Msg.Info($"project '{ids[0]}' could not be found.");
                }

            }

        }

        private static void CmdHelp(string? _) {
            string helpTxt = Util.GetEmbeddedContent("help.txt");
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

        public record CmdTableRow(string Descr, string? Arg, Action<string?> Function);

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
                    "help", new("Display information and available commands.", null, CmdHelp)
                },
                {
                    "new", new("Create a new project.", null, CmdNew)
                },
                {
                    "ls", new("List all projects or display a project overview using an id.", "[id]", CmdList)
                },
                {
                    "task", new("Create new task in a project -- ex. (new task in project 2): prj task 2", "<id>", CmdTask)
                },
                {
                    "log", new("Write a new log in a project -- ex. (new log in project 8): prj log 8", "<id>", CmdLog)
                },
                {
                    "flip", new("Flip task state -- ex. (task 5 in project 12): prj flip 12-5", "<id>", CmdFlip)
                },
                {
                    "edit", new("Edit a project, log or task -- ex. (task/log 1 in project 3): prj edit 3-1", "<id>", CmdEdit)
                },
                {
                    "rm", new("Remove an item -- ex. (remove task 2 in project 1): prj rm 1-2", "<id>", CmdRemove)
                },
            };

    }
}


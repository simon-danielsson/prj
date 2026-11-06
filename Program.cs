
/*
   Several projects and each project contains a list of tasks as well as free logs.

    Project[] (title, body) [key: id]
        Logs[] (title, body, date created)
        Tasks[] (text, date created, date finished, finished boolean)

    TODO: Editor (vim/neovim) integration

  */

namespace Wlk {
    /// <summary>
    /// Entrypoint and argument handling.
    /// </summary>
    static class Program {
        static void Main(string[] args) {

            if (!ProjectStorage.LoadFromFile())
                return;
            // ProjectStore.DebugPopulateProjects();
            // ProjectStore.SaveToFile();
            // ProjectStore.DebugProjectsPrint();

            if (args.Length != 0) {
                foreach (string arg in args) {
                    if (Commands.CmdTable.TryGetValue(arg, out var row)) {
                        row.Function(string.Empty);
                        return;
                    }
                    else {
                        Console.WriteLine($"Illegal command: '{arg}' -- try 'help'");
                        break;
                    }
                }
            }
            else {
                Console.WriteLine("Error: no command provided -- try 'help'");
            }
        }
    }
}


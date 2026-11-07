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

        // static void Main() {
        //     // editor integration
        //     Console.WriteLine("Hello world!");
        //     if (Utils.EditInEditor("This is some default file content") is string c)
        //         Console.WriteLine(c);
        //     else
        //         return;
        // }

        class Args {
            readonly static string[] args = Environment.GetCommandLineArgs();
            readonly int argsLen = args.Length;
            private int cursor = 1;

            public bool ArgLeft() {
                return cursor + 1 <= argsLen;
            }

            public string? Current() {
                return args[cursor];
            }

            public string? Next() {
                return ArgLeft() ? args[cursor += 1] : null;
            }

            public string? PeekNext() {
                return ArgLeft() ? args[cursor + 1] : null;
            }
        }

        static void Main() {

            // ProjectStorage.DebugPopulateProjects();
            // ProjectStore.DebugProjectsPrint();

            if (!ProjectStorage.LoadFromFile())
                return;

            var a = new Args();

            while (a.ArgLeft()) {
                if (a.Current() is string arg) {
                    if (Commands.CmdTable.TryGetValue(arg, out var row)) {
                        if (!row.HasArg) {
                            row.Function(-1);
                            break;
                        }
                        if (a.ArgLeft() && a.PeekNext() != null) {
                            row.Function(Convert.ToInt32(a.Next()));
                            break;
                        }
                        else {
                            Console.WriteLine($"No argument provided for command '{arg}' -- try 'help'");
                            return;
                        }
                    }
                    else {
                        Console.WriteLine($"Illegal command: '{arg}' -- try 'help'");
                        break;
                    }
                }
                a.Next();
            }

            if (!ProjectStorage.SaveToFile())
                return;
        }
    }
}

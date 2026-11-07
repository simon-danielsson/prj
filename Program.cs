/*
   Several projects and each project contains a list of tasks as well as free logs.

    Project[] (title, body) [key: id]
        Logs[] (title, body, date created)
        Tasks[] (text, date created, date finished, finished boolean)

  */

namespace Prj {
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

        static void Main() {

            // ProjectStore.DebugProjectsPrint();

            if (!ProjectStorage.LoadFromFile())
                return;
            // ProjectStorage.DebugPopulateProjects();

            var a = new Args();

            while (a.ArgLeft()) {
                if (a.Current() is string arg) {
                    if (Commands.CmdTable.TryGetValue(arg, out var row)) {
                        if (!row.HasArg) {
                            row.Function(-1);
                            break;
                        }
                        if (int.TryParse(a.Next(), out var functionArg)) {
                            row.Function(functionArg);
                            break;
                        }
                        else {
                            Utils.MsgErr($"missing or malformed argument for command '{arg}'");
                            break;
                        }
                    }
                    else {
                        Utils.MsgErr($"illegal command '{arg}'");
                        break;
                    }
                }
                a.Next();
            }

            ProjectStorage.SaveToFile();
        }

        class Args {
            readonly static string[] args = Environment.GetCommandLineArgs();
            readonly int argsLen = args.Length;
            private int cursor = 1;

            public bool ArgLeft() {
                return cursor + 1 <= argsLen;
            }

            public bool HasNext() {
                return cursor + 1 < argsLen;
            }

            public string? Current() {
                return args[cursor];
            }

            public string? Next() {
                return HasNext() ? args[cursor += 1] : null;
            }

            public string? PeekNext() {
                return HasNext() ? args[cursor + 1] : null;
            }
        }
    }
}

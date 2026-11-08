namespace Prj {
    /// <summary>
    /// Entrypoint and argument handling.
    /// </summary>
    static class Program {

        static void Main() {
            if (Console.WindowWidth < 120) {
                Utils.MsgErr("a minimum terminal width of 120 is required to run this program");
                return;
            }

            // ProjectStore.DebugProjectsPrint();

            if (!ProjectStorage.LoadFromFile())
                return;

            // ProjectStorage.DebugPopulateProjects();
            // ProjectStorage.SaveToFile();
            // return;

            var a = new Args();

            while (a.ArgLeft()) {
                if (a.Current() is string arg) {
                    if (Commands.CmdTable.TryGetValue(arg, out var row)) {
                        if (!row.HasArg) {
                            row.Function(null);
                            break;
                        }

                        if (a.Next() is string functionArg) {
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

namespace Prj {
    /// <summary>
    /// Entrypoint and argument handling.
    /// </summary>
    static class Program {

        static void Main() {
            if (Console.WindowWidth < 100) {
                Msg.Error("a minimum terminal width of 100 is required to run this program");
                return;
            }

            if (!ProjectStorage.LoadFromFile())
                return;

            var a = new Args();

            while (a.ArgLeft()) {
                if (a.Current() is string arg) {
                    if (Commands.CmdTable.TryGetValue(arg, out var row)) {
                        if (a.Next() is string functionArg) {
                            row.Function(functionArg);
                            break;
                        }
                        else {
                            row.Function(null);
                            break;
                        }
                    }
                    else {
                        Msg.Error($"illegal command '{arg}' - use command 'help' if you're feeling lost.");
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

namespace Prj.src {
    /// <summary>
    /// Entrypoint and argument handling.
    /// </summary>
    static class Program {

        static void Main() {
            if (Console.WindowWidth < 80) {
                Msg.Error("a minimum terminal width of 80 is required to run this program");
                return;
            }

            if (!ProjectStorage.LoadFromFile())
                return;

            Args args = new();

            if (args.AreNone) {
                Msg.Error($"no command was provided - use command 'help' if you're feeling lost.");
                return;
            }

            while (args.AreArgsLeft) {
                if (args.Current is string arg) {
                    if (Commands.CmdTable.TryGetValue(arg, out var row)) {
                        row.Command(args.Remaining);
                        break;
                    } else {
                        Msg.Error($"illegal command '{arg}' - use command 'help' if you're feeling lost.");
                        break;
                    }
                }
                args.IncrementNext();
            }

            ProjectStorage.SaveToFile();
        }

    }
}

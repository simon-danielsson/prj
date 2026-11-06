namespace Wlk {
    static class Program {
        static void Main(string[] args) {
            if (args.Length != 0) {
                foreach (string s in args) {
                    if (Commands.CmdTable.TryGetValue(s, out var row)) {
                        row.Function(string.Empty);
                        return;
                    }
                    else {
                        Console.WriteLine($"Illegal command: '{s}' -- try 'help'");
                        break;
                    }
                }
            }
            else {
                Console.WriteLine("Error: no command provided -- try 'help'");
            }
        }
    }

    static class Commands {
        private static void CmdMain(string input) {
            Console.Write($"CmdMain input: {input}");
        }

        private static void CmdHelp(string _) {
            string help_txt = Utils.GetEmbeddedContent("help.txt");
            Console.Write($"{help_txt}");

            foreach (KeyValuePair<string, CmdTableRow> row in CmdTable) {
                Console.WriteLine(
                    string.Format("{0,-16}{1,0}", row.Key, row.Value.Descr));
            }
        }

        public record CmdTableRow(string Descr, Action<string> Function);
        static public Dictionary<string, CmdTableRow> CmdTable = new(){
                {
                    "help", new("Display help.", CmdHelp)
                },
                {
                    "main", new("Main function", CmdMain)
                },
            };
    }

}


namespace Prj.src {
    class Args {
        readonly private string[] args;
        readonly private int argsLen;
        private int cursor;

        public Args() {
            args = Environment.GetCommandLineArgs();
            argsLen = args.Length;
            cursor = 1;
        }

        public bool AreNone => args.Length < 2;

        public List<string> Remaining => [.. args[(cursor + 1)..]];

        public bool AreArgsLeft => cursor + 1 <= argsLen;

        public bool HasNext => cursor + 1 < argsLen;

        public string? Current => args[cursor];

        public bool IncrementNext() {
            if (HasNext) {
                cursor += 1;
                return true;
            }
            return false;
        }

        public string? PeekNext => HasNext ? args[cursor + 1] : null;
    }
}

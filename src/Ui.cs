namespace Prj.src {

    /// <summary>
    /// ANSI Colors
    /// </summary>
    static class Color {
        private readonly static List<(string, string)> ColorList = [
            ("reset", "\x1b[0m"),
            ("bold", "\x1b[1m"),
            ("underline", "\x1b[4;1m"),
            ("overline", "\x1b[53;1m"),
            ("crossed", "\x1b[9;1m"),
            ("grey", "\x1B[38;2;95;95;99m")
        ];

        public static string Ansi(string key) {
            (string, string) c = ColorList.Find(c => c.Item1 == key);
            if (c.Item2.Length > 0)
                return c.Item2;
            else
                throw new InvalidOperationException($"\nColor '{key}' not found");
        }
    }

}

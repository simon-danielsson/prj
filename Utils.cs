using System.Reflection;
using System.Text.Json;

namespace Wlk {
    /// <summary>
    /// Various helpers and global settings.
    /// </summary>
    static class Utils {
        private readonly static Dictionary<string, string> Colors = new(){
            {"reset", "\x1b[0m"},
            {"bold", "\x1b[2m"},
            {"grey", "\x1B[38;2;85;85;85m"},
        };

        public static string Color(string key) {
            Colors.TryGetValue(key, out string? c);
            return c
                    ?? throw new InvalidOperationException(
            $"\nColor '{key}' not found. \nAvailable: " +
            string.Join(", ", Colors.Keys) + "\n---");
        }

        public static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, IncludeFields = true };

        // TODO: implement file search across various config/cache directories
        public static string? LocateStoragePath(string filename) {
            if (true)
                return "wlk.storage";
            else
                return null;
        }

        public static string GetEmbeddedContent(string name) {
            Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream("wlk.embedded." + name)
            ?? throw new InvalidOperationException(
                $"\nResource '{name}' not found. \nAvailable: " +
                string.Join(", ", asm.GetManifestResourceNames()) + "\n---");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}


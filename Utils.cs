using System.Reflection;
using System.Diagnostics;
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
        public static string? GetStoragePath() {
            string? homeDir = Environment.GetEnvironmentVariable("HOME");
            if (homeDir is null) {
                Console.Error.WriteLine("Error: environment variable '$HOME' not set");
                return null;
            }
            return homeDir + "/.wlk.storage";
        }

        public static bool InitStorage() {
            string? path = GetStoragePath();
            Console.WriteLine("Initializing new storage...");
            if (path == null) {
                Console.Error.WriteLine("Error: failed to fetch storage path");
                return false;
            }
            else {
                try {
                    File.Create(path).Dispose();
                    Console.WriteLine($"New storage has been initialized at: {path}");
                    return true;
                }
                catch {
                    Console.Error.WriteLine("Error: failed to save file to storage path");
                    return false;
                }
            }
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

        // Source - https://stackoverflow.com/a/3378474
        // Posted by Jordão, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-11-06, License - CC BY-SA 3.0
        public sealed class TemporaryFile : IDisposable {
            public TemporaryFile() :
              this(Path.GetTempPath()) { }

            public TemporaryFile(string directory) {
                Create(Path.Combine(directory, Path.GetRandomFileName()));
            }

            ~TemporaryFile() {
                Delete();
            }

            public void Dispose() {
                Delete();
                GC.SuppressFinalize(this);
            }

            public string? FilePath { get; private set; }

            private void Create(string path) {
                FilePath = path;
                using (File.Create(FilePath)) { }
                ;
            }

            private void Delete() {
                if (FilePath == null) return;
                File.Delete(FilePath);
                FilePath = null;
            }
        }

        /// <summary>
        /// Launches user ($EDITOR) editor on a temporary file, with default
        /// file content defined by parameter defaultContent. Returns the
        /// content of the edited file.
        /// </summary>
        public static string? EditInEditor(string defaultContent) {
            string? output = null;
            string? ed = Environment.GetEnvironmentVariable("EDITOR");
            if (ed is null) {
                Console.Error.WriteLine("Error: environment variable '$EDITOR' not set");
                return null;
            }

            using (var tempFile = new TemporaryFile()) {
                if (tempFile.FilePath != null) {
                    File.WriteAllText(tempFile.FilePath, defaultContent);
                    // Console.WriteLine($"tmp: {tempFile.FilePath}");
                    var p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = ed;
                    p.StartInfo.Arguments = tempFile.FilePath;
                    p.Start();
                    p.WaitForExit();
                    output = File.ReadAllText(tempFile.FilePath);
                }
                else
                    return null;
            }

            return output;
        }

    }
}


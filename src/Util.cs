using System.Reflection;
using System.Diagnostics;
using System.Text.Json;

namespace Prj.src {

    static class Msg {
        public static void Error(string msg) {
            Console.Error.WriteLine($"Error: {msg}");
        }

        public static void Info(string msg) {
            Console.Error.WriteLine($"Info: {msg}");
        }

    }

    /// <summary>
    /// Various helpers and global settings.
    /// </summary>
    static class Util {

        public static string UserInput(string? prompt) {
            Console.WriteLine(prompt ?? "");
            Console.Write("=> ");
            return Console.ReadLine() ?? "";
        }

        /// <summary> Concatenates a string if its length reaches a certain threshold.</summary>
        public static string Concat(string str, int threshold) {
            return str.Length <= threshold
                ? str
                : str[..threshold] + "...";
        }

        public static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, IncludeFields = true };

        /// <summary> Takes a DateTimeOffset and formats it nicely for printing
        /// to console. </summary>
        public static string TimeDisplay(DateTimeOffset? t) {
            if (t is DateTimeOffset ta)
                return ta.ToString("MMM dd yyyy, HH:mm");
            return "";
        }

        public static string? GetStoragePath() {
            string? homeDir = Environment.GetEnvironmentVariable("HOME");
            if (homeDir is null) {
                Msg.Error("environment variable '$HOME' not set");
                return null;
            }
            return homeDir + "/.prj.storage";
        }

        public static bool InitStorage() {
            string? path = GetStoragePath();
            Msg.Info("initializing new storage...");
            if (path == null) {
                Msg.Error("failed to fetch storage path");
                return false;
            } else {
                try {
                    File.Create(path).Dispose();
                    Msg.Info($"new storage has been initialized at: {path}");
                    return true;
                }
                catch {
                    Msg.Error("failed to save file to storage path");
                    return false;
                }
            }
        }

        public static string GetEmbeddedContent(string name) {
            Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream("prj.static." + name)
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
                Msg.Error("environment variable '$EDITOR' not set");
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
                } else
                    return null;
            }

            return output;
        }

    }
}


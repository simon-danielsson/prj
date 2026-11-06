using System.Reflection;

namespace Wlk

{
    static class Utils
    {
        public static string GetEmbeddedContent(string name)
        {
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


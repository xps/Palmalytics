using System.IO;
using System.Linq;
using System.Reflection;

namespace Palmalytics.SqlServer.Scripts
{
    public static class ResourceHelpers
    {
        private static Stream GetResourceForPath(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = GetResourceNameForPath(path);
            return assembly.GetManifestResourceStream(resourceName);
        }

        private static string GetResourceNameForPath(string path)
        {
            return "Palmalytics.SqlServer.Scripts." + path.Replace('/', '.');
        }

        public static bool ScriptExists(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = GetResourceNameForPath(filename);
            return assembly.GetManifestResourceNames().Contains(resourceName);
        }

        public static string GetScriptContent(string filename)
        {
            using var inputStream = GetResourceForPath(filename) ??
                throw new FileNotFoundException($"Can't find script with name: {filename}");

            using var reader = new StreamReader(inputStream);

            return reader.ReadToEnd();
        }
    }
}

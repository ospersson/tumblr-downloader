using System;
using System.IO;
using System.Reflection;

namespace TDown
{
    public static class Infrastructure
    {
        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var uriPath = Uri.UnescapeDataString(uri.Path);
                var directoryName = Path.GetDirectoryName(uriPath);
                return directoryName;
            }
        }
    }
}

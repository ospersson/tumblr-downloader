using System;
using System.IO;
using System.Reflection;

namespace TDownCore
{
    public static class Infrastructure
    {
        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = typeof(JsonHandler).GetTypeInfo().Assembly.CodeBase;
                //var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var uriPath = Uri.UnescapeDataString(uri.Path);
                var directoryName = Path.GetDirectoryName(uriPath);
                return directoryName;
            }
        }
    }
}

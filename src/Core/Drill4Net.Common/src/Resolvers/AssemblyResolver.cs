using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Drill4Net.Common
{
    public class AssemblyResolver : BaseResolver
    {
        private static readonly Dictionary<string, Assembly> _cache = new();

        /********************************************************************************/

        public AssemblyResolver(List<string> searchDirs = null) : base(searchDirs)
        {
        }

        /********************************************************************************/

        public Assembly Resolve(string fullName, string requestingAssemblyPath = null)
        {
            if (_cache.ContainsKey(fullName))
                return _cache[fullName];
            Assembly asm;
            var (shortName, version) = CommonUtils.ParseAssemblyVersion(fullName);
            var path = FindAssemblyPath(shortName, version);
            if (path == null)
            {
                //maybe it's resource dll? Strange, but some of resource's searchings were here,
                //not in CurrentDomain.AssemblyResolve event. But it's inaccurate...
                if (shortName.EndsWith(".resources"))
                {
                    var resName = $"{shortName}.dll";
                    //resName = "FxResources.System.Private.Xml.SR.resources"; //TEST!!!
                    var reqAsm = Assembly.LoadFrom(requestingAssemblyPath);
                    using var input = reqAsm.GetManifestResourceStream(resName);
                    if (input != null)
                    {
                        try
                        {
                            var ar = StreamToBytes(input);
                            asm = Assembly.Load(ar);
                            _cache.Add(fullName, asm);
                            return asm;
                        }
                        catch(Exception ex)
                        {
                            //log...
                            throw;
                        }
                    }
                    //else
                        //return Assembly.LoadFrom(requestingAssemblyPath);
                }
                return null;
            }
            if (!File.Exists(path))
                throw new FileNotFoundException($"Resolve failed, file not found: [{path}]");
            asm = Assembly.LoadFrom(path);
            _cache.Add(fullName, asm);
            return asm;
        }

        private static byte[] StreamToBytes(Stream input)
        {
            int capacity = input.CanSeek ? (int)input.Length : 0;
            using MemoryStream output = new MemoryStream(capacity);
            int readLength;
            byte[] buffer = new byte[4096];

            do
            {
                readLength = input.Read(buffer, 0, buffer.Length); //better to use buffer.Length
                output.Write(buffer, 0, readLength);
            }
            while (readLength != 0);

            output.Position = 0;
            return output.ToArray();
        }

        public Assembly ResolveResource(string requestingAssemblyPath, string resource)
        {
            if (!File.Exists(requestingAssemblyPath))
                return null;
            if (string.IsNullOrWhiteSpace(resource) || !resource.Contains("."))
                return null;
            var ar = resource.Split('.');
            var localization = ar[ar.Length - 2];
            var dir = Path.GetDirectoryName(requestingAssemblyPath);
            var dir2 = Path.Combine(new DirectoryInfo(dir).Parent.FullName, localization);
            var path = Path.Combine(dir2, Path.GetFileName(requestingAssemblyPath));
            if (!File.Exists(path))
                path = requestingAssemblyPath; // null;
            return string.IsNullOrWhiteSpace(path) ? null : Assembly.LoadFrom(path);
        }
    }
}

using System;
using System.Linq;
using System.Reflection;

namespace Drill4Net.Common
{
    public class CommonUtils
    {
        public static AssemblyVersioning GetEntryTargetVersioning()
        {
            return new AssemblyVersioning(GetAssemblyVersion(Assembly.GetEntryAssembly()));
        }

        public static string GetAssemblyVersion(Assembly asm)
        {
            var fs = asm.DefinedTypes.FirstOrDefault(a => a.FullName?.Contains("-Target-Common-FSharp") == true);
            string versionS;
            if (fs != null)
            {
                //var ar = fs.FullName?.Split('$'); //hm...
                //versionS = "";
                throw new NotImplementedException($"Processing of FSharp not implemented yet");
            }
            else
            {
                var versionAttr = asm.CustomAttributes
                    .FirstOrDefault(a => a.AttributeType == typeof(System.Runtime.Versioning.TargetFrameworkAttribute));
                versionS = versionAttr?.ConstructorArguments[0].Value?.ToString();
            }
            return versionS;
        }
    }
}

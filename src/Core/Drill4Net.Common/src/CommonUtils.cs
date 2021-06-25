using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Drill4Net.Common
{
    /// <summary>
    /// Common util functions
    /// </summary>
    public static class CommonUtils
    {
        /// <summary>
        /// Get target version for the entry assembly of current process
        /// </summary>
        /// <returns></returns>
        public static AssemblyVersioning GetEntryTargetVersioning()
        {
            return new AssemblyVersioning(GetAssemblyVersion(Assembly.GetEntryAssembly()));
        }

        public static AssemblyVersioning GetCallingTargetVersioning()
        {
            return new AssemblyVersioning(GetAssemblyVersion(Assembly.GetCallingAssembly()));
        }

        public static AssemblyVersioning GetExecutingTargetVersioning()
        {
            return new AssemblyVersioning(GetAssemblyVersion(Assembly.GetExecutingAssembly()));
        }

        /// <summary>
        /// Get string version of specified assembly
        /// </summary>
        /// <param name="asm"></param>
        /// <returns></returns>
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

        public static AssemblyVersioning GetAssemblyVersion(string path)
        {
            using var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters());
            var versionAttr = assembly.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "TargetFrameworkAttribute");
            var versionS = versionAttr?.ConstructorArguments[0].Value?.ToString();
            return new AssemblyVersioning(versionS);
        }

        /// <summary>
        /// Convert string to the HEX representation
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToHexString(string s)
        {
            var ba = Encoding.Default.GetBytes(s);
            var hexString = BitConverter.ToString(ba);
            return hexString.Replace("-", "");
        }
    }
}

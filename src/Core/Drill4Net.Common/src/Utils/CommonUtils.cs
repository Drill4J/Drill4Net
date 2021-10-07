using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Mono.Cecil;

namespace Drill4Net.Common
{
    /// <summary>
    /// Common util functions
    /// </summary>
    public static class CommonUtils
    {
        public static int CurrentProcessId { get; }

        /******************************************************************/

        static CommonUtils()
        {
            CurrentProcessId = Process.GetCurrentProcess().Id;
        }

        /******************************************************************/

        public static string GetAppName()
        {
            return Assembly.GetEntryAssembly().GetName().Name;
        }

        #region TargetVersioning
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
        #endregion
        #region Assembly
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
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));
            //
            var searches = new List<string> { Path.GetDirectoryName(path) };
            using var resolver = new AssemblyDefinitionResolver(searches);
            using var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver = resolver });
            var versionAttr = assembly.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "TargetFrameworkAttribute");
            var versionS = versionAttr?.ConstructorArguments[0].Value?.ToString();
            return new AssemblyVersioning(versionS);
        }

        public static (string ShortName, Version Version) ParseAssemblyVersion(string fullName)
        {
            //'System.Text.Json, Version=5.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentNullException(nameof(fullName));
            if (!fullName.Contains(", ") || !fullName.Contains("="))
                throw new ArgumentException(nameof(fullName));
            var ar = fullName.Split(',');
            var ver = ar[1].Trim().Split('=')[1];
            return (ar[0], new Version(ver));
        }
        #endregion
        #region FirstChanceException & Resolving
        //TODO: replace all File.AppendAllLines on normal writer to file (see ChannelsQueue in Agent.File)!!!

        public static void LogFirstChanceException(string emergencyLogDir, string context, Exception e)
        {
            if (!Directory.Exists(emergencyLogDir))
                Directory.CreateDirectory(emergencyLogDir);
            File.WriteAllLines(Path.Combine(emergencyLogDir, "first_chance_error.log"),
                new List<string> { $"{GetPreciseTime()}|{context}:\n{e}" });
        }

        public static Assembly TryResolveAssembly(string dir, string context, ResolveEventArgs args, AssemblyResolver resolver, ILogger log)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var name = args.Name;
            log?.LogDebug($"{context}: need resolve the assembly: [{name}]");
            var asm = resolver.Resolve(name, args.RequestingAssembly.Location);
            if (asm != null)
                return asm;
            var info = $"{GetPreciseTime()}|{context}: {name} -> request assembly from [{args.RequestingAssembly.FullName}] at [{args.RequestingAssembly.Location}]";
            File.AppendAllLines(Path.Combine(dir, "resolve_failed.log"), new string[] { info });
            log?.LogDebug($"{context}: assembly [{name}] didn't resolve");
            return args.RequestingAssembly; //null
        }

        public static Assembly TryResolveResource(string dir, string context, ResolveEventArgs args, AssemblyResolver resolver, ILogger log)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var name = args.Name;
            var asm = resolver.ResolveResource(args.RequestingAssembly.Location, name);
            if (asm != null)
                return asm;
            var info = $"{GetPreciseTime()}|{context}: {name} -> request resource from [{args.RequestingAssembly.FullName}] at [{args.RequestingAssembly.Location}]";
            log?.LogDebug(info);
            File.AppendAllLines(Path.Combine(dir, "resolve_resource_failed.log"), new string[] { info });
            return null;
        }

        public static Assembly TryResolveType(string dir, string context, ResolveEventArgs args, ILogger log)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var name = args.Name;
            var info = $"{GetPreciseTime()}|{context}: {name} -> request type from [{args.RequestingAssembly.FullName}] at [{args.RequestingAssembly.Location}]";
            log?.LogDebug(info);
            File.AppendAllLines(Path.Combine(dir, "resolve_type_failed.log"), new string[] { info });
            return null;
        }
        #endregion
        #region Exception Desc
        /// <summary>
        /// Get the description of the error (including AggregateException)
        /// </summary>
        public static string GetExceptionDescription(Exception ex)
        {
            var bld = new StringBuilder();
            if (ex is AggregateException aex)
            {
                foreach (var e in aex.Flatten().InnerExceptions)
                    bld.AppendLine(GetDesc(e));
            }
            else
            {
                bld.AppendLine(GetDesc(ex));
            }
            return bld.ToString();
        }

        internal static string GetDesc(Exception ex)
        {
            return ex?.ToString();
        }
        #endregion

        public static string GetTypeByMethod(string methodFullName)
        {
            if (string.IsNullOrWhiteSpace(methodFullName))
                throw new ArgumentNullException(nameof(methodFullName));
            //
            if (!methodFullName.Contains(" ") || !methodFullName.Contains(":"))
                return null;
            var tAr = methodFullName.Split(' ')[1].Split(':');
           return tAr[0];
        }

        public static string GetRootNamespace(string typeFullName)
        {
            if (string.IsNullOrWhiteSpace(typeFullName))
                throw new ArgumentNullException(nameof(typeFullName));
            //
            if (!typeFullName.Contains("."))
                return null;
            var tAr = typeFullName.Split('.');
            return tAr[0];
        }

        public static (string ns, string type) GetNamespaceAndTypeName(string typeFullName)
        {
            string typeName;
            string ns = null;
            if (typeFullName.Contains("."))
            {
                var list = typeFullName.Split('.').ToList();
                typeName = list[list.Count-1];
                list.RemoveAt(list.Count - 1);
                ns = string.Join(".", list);
            }
            else
            {
                typeName = typeFullName;
            }
            return (ns, typeName);
        }

        public static string GetPreciseTime()
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
        }

        public static bool IsStringMachRegexPattern(string s, string pattern)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;
            try
            {
                return Regex.IsMatch(s, pattern);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Convert string to the HEX representation
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToHexString(string s)
        {
            var ba = Encoding.UTF8.GetBytes(s);
            var hexString = BitConverter.ToString(ba);
            return hexString.Replace("-", "");
        }

        public static string ToBase64String(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string FromBase64String(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static long GetCurrentUnixTimeMs()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static DateTime ConvertUnixTime(long ts)
        {
            var offset = DateTimeOffset.FromUnixTimeMilliseconds(ts);
            return offset.DateTime;
        }
    }
}

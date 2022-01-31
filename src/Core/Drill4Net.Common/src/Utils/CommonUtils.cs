using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Drill4Net.Common;

// automatic version tagger including Git info - https://github.com/devlooped/GitInfo
// semVer creates an automatic version number based on the combination of a SemVer-named tag/branches
// the most common format is v0.0 (or just 0.0 is enough)
// to change semVer it is nesseccary to create appropriate tag and push it to remote repository
// patches'(commits) count starts with 0 again after new tag pushing
// For file version format exactly is digit
[assembly: AssemblyFileVersion(CommonUtils.AssemblyFileGitVersion)]
[assembly: AssemblyInformationalVersion(CommonUtils.AssemblyProductVersion)]

namespace Drill4Net.Common
{
    /// <summary>
    /// Common util functions
    /// </summary>
    public static class CommonUtils
    {
        public const string AssemblyFileGitVersion = $"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}.{ThisAssembly.Git.SemVer.Label}";
        public const string AssemblyProductVersion = $"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}/*.{ThisAssembly.Git.SemVer.Label}*/-{ThisAssembly.Git.Branch}+{ThisAssembly.Git.Commit}";
        public static int CurrentProcessId { get; }

        /******************************************************************/

        static CommonUtils()
        {
            CurrentProcessId = Process.GetCurrentProcess().Id;
        }

        /******************************************************************/

        public static string GetAppName()
        {
            return AppDomain.CurrentDomain.FriendlyName;
        }

        public static string GetAppVersion()
        {
            var asm = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly() ?? Assembly.GetExecutingAssembly();
            return FileUtils.GetProductVersion(asm.Location);
        }

        public static string GetFrameworkRawVersion()
        {
            return AppContext.TargetFrameworkName;
        }

        #region TargetVersioning
        /// <summary>
        /// Get target version for the entry assembly of current process
        /// </summary>
        /// <returns></returns>
        public static AssemblyVersioning GetEntryTargetVersioning()
        {
            return GetAssemblyVersioning(Assembly.GetEntryAssembly());
        }

        public static AssemblyVersioning GetCallingTargetVersioning()
        {
            return GetAssemblyVersioning(Assembly.GetCallingAssembly());
        }

        public static AssemblyVersioning GetExecutingTargetVersioning()
        {
            return GetAssemblyVersioning(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Get version for the specified assembly
        /// </summary>
        /// <returns></returns>
        public static AssemblyVersioning GetAssemblyVersioning(Assembly asm)
        {
            return new AssemblyVersioning(GetAssemblyVersion(asm));
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
            if (asm == null)
                throw new ArgumentNullException(nameof(asm));
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
        public static void LogUnhandledException(string emergencyLogDir, string context, string err)
        {
            try
            {
                if (!Directory.Exists(emergencyLogDir))
                    Directory.CreateDirectory(emergencyLogDir);
                File.AppendAllLines(Path.Combine(emergencyLogDir, "unhandled_error.log"),
                    new List<string> { $"{GetPreciseTime()}|{context}|{err}" });
            }
            catch { }
        }

        //TODO: replace all File.AppendAllLines on normal writer to file (see ChannelsQueue in Agent.File) in next methods!!!
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
        #region Type & method name parsing
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

        public static (string ns, string type) DeconstructFullTypeName(string typeFullName)
        {
            return DeconstructForLastGroup(typeFullName);
        }

        public static (string type, string method) DeconstructFullMethodName(string methodFullName)
        {
            return DeconstructForLastGroup(methodFullName);
        }

        private static (string prefix, string lastGroup) DeconstructForLastGroup(string typeFullName)
        {
            string lastGroup;
            string prefix = null;
            if (typeFullName.Contains("."))
            {
                var list = typeFullName.Split('.').ToList();
                lastGroup = list[list.Count-1];
                list.RemoveAt(list.Count - 1);
                prefix = string.Join(".", list);
            }
            else
            {
                lastGroup = typeFullName;
            }
            return (prefix, lastGroup);
        }
        #endregion

        public static bool IsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }

        public static string GetUserDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        public static (bool res, int pid) StartProgram(string subsystem, string path, string args,
            out string error, bool createNoWindow = false)
        {
            error = "";
            path = FileUtils.GetFullPath(path);
            var process = new Process
            {
                StartInfo =
                {
                    FileName = path,
                    Arguments = args,
                    WorkingDirectory = Path.GetDirectoryName(path),
                    CreateNoWindow = createNoWindow, //true for real using
                    //UseShellExecute = false, //false for real using
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true
                }
            };
            var res = process.Start();
            if (!res)
            {
                error = $"Program {subsystem} -> pid={process.Id} is not started";
                return (false, 0);
            }
            return (true, process.Id);
        }

        public static async Task WaitForProcessExit(int pid)
        {
            //yes, it is really simpler then using mutex (or even event of Process)
            while (true)
            {
                try
                {
                    var prc = Process.GetProcessById(pid);
                    if (prc?.HasExited != false)
                        break;
                    await Task.Delay(200);
                }
                catch { return; }
            }
        }

        public static string GetPreciseTime()
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
        }

        //private static readonly object _tmpLogLocker = new();
        //private static bool _errorOnTmpLog;
        //public static void WriteTempLog(string content, string logFile = null)
        //{
        //    if (_errorOnTmpLog)
        //        return;
        //    if (string.IsNullOrWhiteSpace(logFile))
        //        logFile = @"D:\drill_tmpLog.txt"; //Path.Combine(FileUtils.EntryDir, "drill_tmpLog.txt");
        //    if (!Directory.Exists(Path.GetPathRoot(logFile)))
        //        return;
        //    try
        //    {
        //        var dir = Path.GetDirectoryName(logFile);
        //        if (!Directory.Exists(dir))
        //            Directory.CreateDirectory(dir);
        //        lock (_tmpLogLocker)
        //        {
        //            File.AppendAllText(logFile, $"{GetPreciseTime()}|{content}\n");
        //        }
        //    }
        //    catch { _errorOnTmpLog = true; }
        //}

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

        public static DateTime ConvertFromUnixTime(long ts)
        {
            var offset = DateTimeOffset.FromUnixTimeMilliseconds(ts);
            return offset.DateTime;
        }
    }
}

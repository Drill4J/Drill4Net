using System;
using System.Diagnostics;
using Drill4Net.Common;
using System.Reflection;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Messaging.Transport;
using Drill4Net.Repository;
using Drill4Net.BanderLog.Sinks.File;
using Drill4Net.BanderLog.Sinks.Console;

/*** INFO
     automatic version tagger including Git info - https://github.com/devlooped/GitInfo
     semVer creates an automatic version number based on the combination of a SemVer-named tag/branches
     the most common format is v0.0 (or just 0.0 is enough)
     to change semVer it is nesseccary to create appropriate tag and push it to remote repository
     patches'(commits) count starts with 0 again after new tag pushing
     For file version format exactly is digit
***/
[assembly: AssemblyFileVersion(CommonUtils.AssemblyFileGitVersion)]
[assembly: AssemblyInformationalVersion(CommonUtils.AssemblyProductVersion)]

namespace Drill4Net.Agent.Worker
{
    internal class Program
    {
        private static Logger _logger;

        /****************************************************************/

        private static void Main(string[] args)
        {
            try
            {
                Init();
                StartWorker(args);
            }
            catch (Exception ex)
            {
                var mess = ex.ToString();
                Console.WriteLine(mess);
                _logger.Fatal(mess);
            }
            Console.WriteLine("Finished");
            Log.Flush();
        }

        private static void Init()
        {
            PrepareLogger();

            var caption = GetCaption();
            SetCaption(caption);

            var mess = $"Worker is starting: {caption}";
            Console.WriteLine(mess);
            _logger.Debug(mess);
        }

        private static void StartWorker(string[] args)
        {
            var creator = new WorkerCreator(args);
            var worker = creator.CreateWorker();
            worker.ErrorOccured += Receiver_ErrorOccured;
            _logger.Debug("Worker has initialized");
            worker.Start();
        }

        private static void SetCaption(string caption)
        {
            if (!Debugger.IsAttached)
                return;
            Console.Title = $"{caption}: {Environment.ProcessId}";
        }

        private static string GetCaption()
        {
            return $"{typeof(AgentWorker).Assembly.GetName().Name} {CommonUtils.GetAppVersion()}";
        }

        /// <summary>
        /// Prepares the logger for the local file log only (without console as automatic output)
        /// </summary>
        /// <returns>The file path to the emergency log</returns>
        public static void PrepareLogger()
        {
            var path = LoggerHelper.GetCommonFilePath(LoggerHelper.LOG_FOLDER);
            var builder = new LogBuilder()
                .AddSink(new FileSink(path))
                .AddSink(new ConsoleSink()) //TODO: only for Debug or DebuggerAtached
                .Build();
            Log.Configure(builder);
            _logger = new TypedLogger<Program>(CoreConstants.SUBSYSTEM_CONFIGURATOR);
        }

        private static void Receiver_ErrorOccured(IMessageReceiver source, bool isFatal, bool isLocal, string message)
        {
            var mess = $"Source = {source} -> error (local: {isLocal}): {message}";
            if (isFatal)
                _logger.Fatal(mess);
            else
                _logger.Error(mess);
            Console.WriteLine(mess);
        }
    }
}

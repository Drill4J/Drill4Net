using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Abstract;
using Drill4Net.BanderLog.Sinks.File;
using MicrosoftLogging = Microsoft.Extensions.Logging;

[assembly: InternalsVisibleToAttribute("Drill4Net.Agent.Standard.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Drill4Net.Agent.Standard.Utils
{
    /// <summary>
    /// Helper for connector log parameters
    /// </summary>
    public class ConnectorLogHelper
    {
#if DEBUG
        private const MicrosoftLogging.LogLevel DEFAULT_LOG_LEVEL = MicrosoftLogging.LogLevel.Debug;
#endif
#if !DEBUG
        private const MicrosoftLogging.LogLevel DEFAULT_LOG_LEVEL = MicrosoftLogging.LogLevel.Error;
#endif
/********************************************************************************************************/

        /// <summary>
        /// Get logFile full path and log level for connector log.
        /// </summary>
        /// <param name="connOpts">Connector options from config</param>
        /// <param name="logger">Logger</param>
        /// <returns></returns>
        public (string logFile, Microsoft.Extensions.Logging.LogLevel logLevel) GetConnectorLogParameters(ConnectorAuxOptions connOpts, Logger logger)
        {
            var logDir = connOpts?.LogDir;
            var logFile = connOpts?.LogFile;

            //get real log level 
            var logLevel =GetLogLevel(connOpts?.LogLevel);

            //Guanito: just first file sink is bad idea...
            //the last because the firast may be just emergency logger
            var fileSink = logger?.GetManager()?.GetSinks()?.LastOrDefault(s => s is FileSink) as FileSink;

            //dir
            PrepareLogDir(ref logDir, fileSink);

            //file path
            PrepareLogFile(ref logFile, logDir);    

            return (logFile, logLevel);
        }

        internal virtual string GetBaseDir()
        {
            return FileUtils.EntryDir;
        }

        internal void PrepareLogDir( ref string logDir, FileSink fileSink)
        {
            if (string.IsNullOrWhiteSpace(logDir))
            {
                if (fileSink == null)
                {
                    logDir = GetBaseDir();
                }
                else
                {
                    logDir = Path.GetDirectoryName(fileSink.Filepath);
                }
            }
            else
            {
                logDir = FileUtils.GetFullPath(logDir, GetBaseDir());
            }
        }

        internal void PrepareLogFile(ref string logFile, string logDir)
        {
            ////is some check needed?
            //if (string.IsNullOrWhiteSpace(logDir))
            //    logDir = GetBaseDir();

            //no file path
            if (string.IsNullOrWhiteSpace(logFile))
                logFile = AgentConstants.CONNECTOR_LOG_FILE_NAME;

            //is it file path?
            if (logFile.Contains(":") || logFile.Contains("..") || logFile.Contains("/") || logFile.Contains("\\"))
            {
                logFile = FileUtils.GetFullPath(logFile, logDir ?? GetBaseDir()); //maybe it is relative path
            }
            else //it is just file name
            {
                logFile = Path.Combine(logDir, logFile);
            }
        }

        internal MicrosoftLogging.LogLevel GetLogLevel(string logLevelOpt)
        {
            if (string.IsNullOrWhiteSpace(logLevelOpt) || !Enum.TryParse(logLevelOpt, true, out MicrosoftLogging.LogLevel logLevel))
                logLevel = DEFAULT_LOG_LEVEL;
            return logLevel;
        }


    }
}

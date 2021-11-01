using System.Collections.Generic;
using Drill4Net.Agent.Abstract;
using Drill4Net.BanderLog;
using Drill4Net.BanderLog.Sinks;
using Drill4Net.BanderLog.Sinks.File;
using Moq;
using Microsoft.Extensions.Logging;

namespace Drill4Net.Agent.Standard.UnitTests.TestData
{
    public static class GetConnectorLogHelperTestData
    {
        public const string ENTRY_DIR = @"D:\Default\app";

#if DEBUG
        private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Debug;
#endif
#if !DEBUG
        private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Error;
#endif
        private const string LOG_FILE_DEFAULT = AgentConstants.CONNECTOR_LOG_FILE_NAME;
        private const string LOG_DIR = @"C:\logs_temp";
        private const string LOG_FILE = "log.txt";
        private const string LOG_LEVEL = "Debug";
        private const string LOG_FILE_FULL_PATH = @"D:\logger.txt";
        private const string LOG_DIR_RELATIVE = @"..\applogs";
        private const string LOG_PATH_RELATIVE = @"..\app_logs\logs.txt";
        private const string LOG_PATH_RELATIVE2 = @"../app_logs/logs.txt";
        private const string LOG_SINK_DIR = @"D:\app";
        private const string LOG_SINK_FILE = @"sink_log.txt";

        private static List<string> _sinkPathList = new List<string>()
        {
            @"C:\log_file.txt",
            null,
            @"E:\apps\temp\app_log.txt",
            $@"{LOG_SINK_DIR}\{LOG_SINK_FILE}",
            null
        };

        /************************************************************************************************************/

        public static Logger CreateLogger(bool isHasManager, List<string> sinkPaths)
        {
            var sinks = CreateSinks(sinkPaths);
            var mockedLogger = new Mock<Logger>(null, null);

            if (isHasManager)
            {
                mockedLogger.Setup(l => l.GetManager()).Returns(new LogManager());
                if (sinks != null)
                    mockedLogger.Setup(l => l.GetManager().GetSinks()).Returns(sinks);
                else
                    mockedLogger.Setup(l => l.GetManager().GetSinks()).Returns<IList<ILogSink>>(null);
            }
            else
                mockedLogger.Setup(l => l.GetManager()).Returns<LogManager>(null);

            return mockedLogger.Object;
        }

        public static ConnectorAuxOptions CreateConnectorAuxOptions(string logDir, string logFile, string logLevel)
        {
            var options = new ConnectorAuxOptions();
            options.LogDir = logDir;
            options.LogFile = logFile;
            options.LogLevel = logLevel;
            return options;
        }

        public static List<ILogSink> CreateSinks(List<string> sinkPaths)
        {
            if (sinkPaths == null)
                return null;

            var fileSinks = new List<ILogSink>();

            foreach(var path in sinkPaths)
            {
                fileSinks.Add(CreateSink(path));
            }
            return  fileSinks;
        }

        public static ILogSink CreateSink(string sinkPath)
        {
            if (sinkPath == null)
                return null;
            return FileSinkCreator.CreateSink(sinkPath);
        }

        #region TestData
        public static IEnumerable<object[]> GetLogParamsTestParameters
        {
            get
            {
                return new List<object[]>()
                {
                   new object[]
                    {
                        CreateConnectorAuxOptions(LOG_DIR,LOG_FILE,"Debug"),
                        CreateLogger(true, _sinkPathList),
                        $@"{LOG_DIR}\{LOG_FILE}",
                        LogLevel.Debug
                    },
                   new object[]
                   {
                        CreateConnectorAuxOptions(LOG_DIR,LOG_FILE,"error"),
                        null,
                        $@"{LOG_DIR}\{LOG_FILE}",
                        LogLevel.Error
                   },
                   new object[]
                   {
                        CreateConnectorAuxOptions(null,null,"custom"),
                        null,
                        $@"{ENTRY_DIR}\{LOG_FILE_DEFAULT}",
                        DEFAULT_LOG_LEVEL
                   },
                   new object[]
                   {
                        CreateConnectorAuxOptions(null, null, null),
                        CreateLogger(true, _sinkPathList),
                        $@"{LOG_SINK_DIR}\{LOG_FILE_DEFAULT}",
                        DEFAULT_LOG_LEVEL
                   },
                   new object[]
                   {
                        CreateConnectorAuxOptions(LOG_DIR_RELATIVE, LOG_PATH_RELATIVE, "Information"),
                        CreateLogger(true, _sinkPathList),
                        @"D:\Default\app_logs\logs.txt",
                        LogLevel.Information
                   }
                };
            }
        }

        public static IEnumerable<object[]> LogFileNullCheckParameters
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        CreateConnectorAuxOptions(LOG_DIR_RELATIVE, LOG_PATH_RELATIVE, LOG_LEVEL),
                        null
                    },
                    new object[]
                    {
                        null,
                        CreateLogger(true, _sinkPathList),
                    },
                    new object[]
                    {
                        null,
                        null
                    },
                    new object[]
                    {
                        CreateConnectorAuxOptions(null, LOG_PATH_RELATIVE, LOG_LEVEL),
                        CreateLogger(true, _sinkPathList),
                    },
                    new object[]
                    {
                        CreateConnectorAuxOptions(LOG_DIR_RELATIVE, null, LOG_LEVEL),
                        CreateLogger(true, _sinkPathList),
                    },
                    new object[]
                    {
                        CreateConnectorAuxOptions(LOG_DIR_RELATIVE, LOG_PATH_RELATIVE, null),
                        CreateLogger(true, _sinkPathList),
                    },
                    new object[]
                    {
                        CreateConnectorAuxOptions(LOG_DIR_RELATIVE, LOG_PATH_RELATIVE,LOG_LEVEL),
                        CreateLogger(false, _sinkPathList),
                    },
                    new object[]
                    {
                        CreateConnectorAuxOptions(LOG_DIR_RELATIVE, LOG_PATH_RELATIVE, LOG_LEVEL),
                        CreateLogger(true,null),
                    }
                };
            }
        }

        public static IEnumerable<object[]> PrepareLogDirParameters
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        LOG_DIR,
                        CreateSink($@"{LOG_SINK_DIR}\{LOG_SINK_FILE}"),
                        LOG_DIR
                    },
                    new object[]
                    {
                        LOG_DIR_RELATIVE,
                        CreateSink($@"{LOG_SINK_DIR}\{LOG_SINK_FILE}"),
                        @"D:\Default\applogs"
                    },
                    new object[]
                    {
                        LOG_DIR,
                        null,
                        LOG_DIR
                    },
                    new object[]
                    {
                        null,
                        CreateSink($@"{LOG_SINK_DIR}\{LOG_SINK_FILE}"),
                        LOG_SINK_DIR
                    },
                    new object[]
                    {
                        " ",
                        null,
                        ENTRY_DIR
                    }
                };


            }
        }

        public static IEnumerable<object[]> PrepareLogFileParameters
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        LOG_PATH_RELATIVE2,
                        LOG_DIR,
                        $@"C:\app_logs\logs.txt"
                    },
                    new object[]
                    {
                        LOG_PATH_RELATIVE,
                        LOG_DIR,
                        $@"C:\app_logs\logs.txt"
                    },
                    new object[]
                    {
                        LOG_PATH_RELATIVE,
                        null,
                        $@"D:\Default\app_logs\logs.txt"
                    },
                    new object[]
                    {
                        LOG_FILE_FULL_PATH,
                        LOG_DIR,
                        LOG_FILE_FULL_PATH
                    },
                    new object[]
                    {
                        LOG_FILE,
                        LOG_DIR,
                        $@"{LOG_DIR}\{LOG_FILE}"
                    },
                    new object[]
                    {
                        null,
                        LOG_DIR,
                        $@"{LOG_DIR}\{LOG_FILE_DEFAULT}"
                    },
                };
            }
        }

        public static IEnumerable<object[]> GetLogLevelParameters
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        "Debug",
                        LogLevel.Debug
                    },
                    new object[]
                    {
                        "INFORMATION",
                        LogLevel.Information
                    },
                    new object[]
                    {
                        "warning",
                        LogLevel.Warning
                    },
                    new object[]
                    {
                        "Error",
                        LogLevel.Error
                    },
                    new object[]
                    {
                        "CriTical",
                        LogLevel.Critical
                    },
                    new object[]
                    {
                        "Trace",
                        LogLevel.Trace
                    },
                    new object[]
                    {
                        "none",
                        LogLevel.None
                    },
                    new object[]
                    {
                        "Custom",
                        DEFAULT_LOG_LEVEL
                    },
                    new object[]
                    {
                        "",
                        DEFAULT_LOG_LEVEL
                    },
                    new object[]
                    {
                        "  ",
                        DEFAULT_LOG_LEVEL
                    },
                    new object[]
                    {
                        null,
                        DEFAULT_LOG_LEVEL
                    }
                };
            }
        }
        #endregion
    }
}

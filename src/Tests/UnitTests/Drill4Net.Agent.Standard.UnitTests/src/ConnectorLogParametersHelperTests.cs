using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Standard.Utils;
using Drill4Net.Agent.Standard.UnitTests.TestData;
using Drill4Net.BanderLog.Sinks.File;

namespace Drill4Net.Agent.Standard.UnitTests
{
    /// <summary>
    /// Tests for ConnectorLogParametersHelper methods.
    /// </summary>
    public class ConnectorLogParametersHelperTests
    {
        private ConnectorLogHelper _logParamHelper;

        /************************************************************************************************************/
        public ConnectorLogParametersHelperTests()
        {
            var mocked= new Mock<ConnectorLogHelper>();
            mocked.Setup(i => i.GetBaseDir())
                 .Returns(GetConnectorLogHelperTestData.ENTRY_DIR);

            _logParamHelper = mocked.Object;
        }
        /************************************************************************************************************/

        [Theory]
        [MemberData(nameof(GetConnectorLogHelperTestData.GetLogParamsTestParameters), MemberType = typeof(GetConnectorLogHelperTestData))]
        public void Get_Log_Params_Test(ConnectorAuxOptions connOpts, Logger logger, string expectedLogFile, LogLevel expectedLogLevel)
        {
            //Arrange

            //Act
            var result = _logParamHelper.GetConnectorLogParameters(connOpts, logger);

            //Assert
            Assert.Equal(expectedLogFile, result.logFile);
            Assert.Equal( expectedLogLevel, result.logLevel);
        }

        [Theory]
        [MemberData(nameof(GetConnectorLogHelperTestData.LogFileNullCheckParameters), MemberType = typeof(GetConnectorLogHelperTestData))]
        public void Get_Log_Params_Null_Test(ConnectorAuxOptions connOpts, Logger logger)
        {
            //Arrange

            //Act
            var exception = Record.Exception(() => _logParamHelper.GetConnectorLogParameters(connOpts, logger));

            //Assert
            Assert.Null(exception);

        }

        [Theory]
        [MemberData(nameof(GetConnectorLogHelperTestData.PrepareLogDirParameters), MemberType = typeof(GetConnectorLogHelperTestData))]
        public void Prepare_Log_Dir_Test(string logDir, FileSink fileSink, string expectedResult)
        {
            //Arrange

            //Act
            _logParamHelper.PrepareLogDir(ref logDir, fileSink);

            //Assert
            Assert.Equal(expectedResult, logDir);
        }

        [Theory]
        [MemberData(nameof(GetConnectorLogHelperTestData.PrepareLogFileParameters), MemberType = typeof(GetConnectorLogHelperTestData))]
        public void Prepare_Log_File_Test(string logFile, string logDir, string expectedResult)
        {
            //Arrange

            //Act
            _logParamHelper.PrepareLogFile(ref logFile, logDir);

            //Assert
            Assert.Equal(expectedResult, logFile);
        }

        [Theory]
        [MemberData(nameof(GetConnectorLogHelperTestData.GetLogLevelParameters), MemberType = typeof(GetConnectorLogHelperTestData))]
        public void Get_Log_Level_Test(string logLevel, LogLevel expectedResult)
        {
            //Arrange

            //Act
            var result= _logParamHelper.GetLogLevel(logLevel);

            //Assert
            Assert.Equal(expectedResult, result);
        }
    }
}

using System.Collections.Generic;
using Moq;
using Xunit;
using Drill4Net.Configuration;
using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Engine.UnitTests
{
    /// <summary>
    /// Tests for InjectorEngine methods.
    /// </summary>
    public class InjectorEngineTest
    {
        /*****************************************************************************************/

        private InjectorEngine CreateInjectorEngine()
        {
            var mockRepository = new Mock<IInjectorRepository>();
           return new InjectorEngine(mockRepository.Object);
        }

        /*****************************************************************************************/

    #region Moniker
        [Theory]
        [MemberData(nameof(MonikerTestData.MonikerTrueData), MemberType = typeof(MonikerTestData))]
        public void Directory_Moniker_True(Dictionary<string, MonikerData> monikers, string root, string dir)
        {
            //Arrange
            var injectorEngine = CreateInjectorEngine();

            //Act
            var result =injectorEngine.IsDirectoryNeedByMoniker(monikers, root, dir);

            //Assert
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(MonikerTestData.MonikerFalseData), MemberType = typeof(MonikerTestData))]
        public void Directory_Moniker_False(Dictionary<string, MonikerData> monikers, string root, string dir)
        {
            //Arrange
            var injectorEngine = CreateInjectorEngine();

            //Act
            var result = injectorEngine.IsDirectoryNeedByMoniker(monikers, root, dir);

            //Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(MonikerTestData.MonikerNullData), MemberType = typeof(MonikerTestData))]
        public void Directory_Moniker_Null(Dictionary<string, MonikerData> monikers, string root, string dir)
        {
            //Arrange
            var injectorEngine = CreateInjectorEngine();

            //Act
            var exception = Record.Exception(() => injectorEngine.IsDirectoryNeedByMoniker(monikers, root, dir));

            //Assert
            Assert.Null(exception);
        }
#endregion
    #region ProcessDirectory
        [Theory]
        [MemberData(nameof(DirectoryTestData.ProcessDirectoryTrueData), MemberType = typeof(DirectoryTestData))]
        public void Process_Directory_True(SourceFilterOptions flt, string directory, bool isRoot)
        {
            // Arrange
            var injectorEngine = CreateInjectorEngine();

            //Act
            var result = injectorEngine.IsNeedProcessDirectory(flt, directory, isRoot);

            //Assert
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(DirectoryTestData.ProcessDirectoryFalseData), MemberType = typeof(DirectoryTestData))]
        public void Process_Directory_False(SourceFilterOptions flt, string directory, bool isRoot)
        {
            // Arrange
            var injectorEngine = CreateInjectorEngine();

            //Act
            var result = injectorEngine.IsNeedProcessDirectory(flt, directory, isRoot);

            //Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(DirectoryTestData.ProcessDirectoryNullData), MemberType = typeof(DirectoryTestData))]
        public void Process_Directory_Null(SourceFilterOptions flt, string directory, bool isRoot)
        {
            //Arrange
            var injectorEngine = CreateInjectorEngine();

            //Act
            var exception = Record.Exception(() => injectorEngine.IsNeedProcessDirectory(flt, directory, isRoot));

            //Assert
            Assert.Null(exception);
        }
        #endregion
    #region ProcessFile
        [Theory]
        [MemberData(nameof(FileTestData.ProcessFileTrueData), MemberType = typeof(FileTestData))]
        public void Process_File_True(SourceFilterOptions flt, string filePath)
        {
            // Arrange
            var injectorEngine = CreateInjectorEngine();

            //Act
            var result = injectorEngine.IsNeedProcessFile(flt, filePath);

            //Assert
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(FileTestData.ProcessFileFalseData), MemberType = typeof(FileTestData))]
        public void Process_File_False(SourceFilterOptions flt, string filePath)
        {
            // Arrange
            var injectorEngine = CreateInjectorEngine();

            //Act
            var result = injectorEngine.IsNeedProcessFile(flt, filePath);

            //Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(FileTestData.ProcessFileNullData), MemberType = typeof(FileTestData))]
        public void Process_File_Null(SourceFilterOptions flt, string filePath)
        {
            //Arrange
            var injectorEngine = CreateInjectorEngine();

            //Act
            var exception = Record.Exception(() => injectorEngine.IsNeedProcessFile(flt,filePath));

            //Assert
            Assert.Null(exception);
        }
        #endregion
    }
}

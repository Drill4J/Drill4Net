using Drill4Net.Configuration;
using Drill4Net.Injector.Core;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Drill4Net.Injector.Engine.UnitTests
{
    public class InjectorEngineTest
    {
        private InjectorEngine CreateInjectorEngine()
        {
            var mockRepository = new Mock<IInjectorRepository>();
           return new InjectorEngine(mockRepository.Object);
        }
#region Monirer
        [Theory]
        [MemberData(nameof(MonikerTestData.NeedByMonikerTrue), MemberType = typeof(MonikerTestData))]
        public void Directory_Moniker_True(Dictionary<string, MonikerData> monikers, string root, string dir)
        {
            var injectorEngine = CreateInjectorEngine();
            var result=injectorEngine.IsDirectoryNeedByMoniker(monikers, root, dir);
            Assert.True(result);

        }
        [Theory]
        [MemberData(nameof(MonikerTestData.NeedByMonikerFalse), MemberType = typeof(MonikerTestData))]
        public void Directory_Moniker_False(Dictionary<string, MonikerData> monikers, string root, string dir)
        {
            var injectorEngine = CreateInjectorEngine();
            var result = injectorEngine.IsDirectoryNeedByMoniker(monikers, root, dir);
            Assert.False(result);

        }
        [Theory]
        [MemberData(nameof(MonikerTestData.NeedByMonikerNullCheck), MemberType = typeof(MonikerTestData))]
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
        [Theory]
        [MemberData(nameof(DirectoryTestData.NeedProcessDirectoryTrue), MemberType = typeof(DirectoryTestData))]
        public void Process_Directory_True(SourceFilterOptions flt, string directory, bool isRoot)
        {
            var injectorEngine = CreateInjectorEngine();
            var result = injectorEngine.IsNeedProcessDirectory(flt, directory, isRoot);
            Assert.True(result);

        }
    }
}

using System;
using System.Linq;
using Xunit;
using Drill4Net.Common;
using Drill4Net.TestDataHelper;

namespace Drill4Net.Injector.Core.UnitTests
{
    /// <summary>
    /// Tests for filters of directories, folders, namespaces, type names, etc.
    /// </summary>
    public class SourceFilterOptionsTests
    {
        private const string _reg = CoreConstants.REGEX_FILTER_PREFIX;
        private SourceFilterOptionsHelper _dataHelper = new SourceFilterOptionsHelper();

        /*************************************************************************************/

        #region IsDirectoryNeed
        [Theory]
        [InlineData(null, @"C:\bin\Debug\Test.File")]
        [InlineData(new string[] {}, @"C:\bin\Debug\Test.File")]
        [InlineData(new string[] { @"C:\bin\Debug\Test.File\" }, @"C:\bin\Debug\Test.File")]
        [InlineData(new string[] { @"D:\bin\\Debug\Test.File\", _reg+ @"\\bin\\((Debug\\)|(Release\\))" }, @"F:\bin\Release\Test.File")]
        [InlineData(new string[] { _reg + @"^C:.*", _reg + @"\\bin\\((Debug\\)|(Release\\))Test\.\w+\.\w+\\" }, @"F:\bin\Debug\Test.File.Helper")]
        [InlineData(new string[] {_reg + @"C:\\bin\\Release\\ver[147]\\" }, @"C:\bin\Release\ver4")]        
        public void Directory_Include_True(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Directories = filters?.ToList();
                  
            // Act
            var result = options.IsDirectoryNeed(s);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(new string[] { @"C:\\bin\\Debug\\Test.File\\" }, @"C:\\bin\\Release\\Test.File")]
        [InlineData(new string[] { @"D:\\bin\\Debug\\Test.File\\", _reg + @"\\bin\\((Debug\\)|(Release\\))" }, @"F:\bin\x64\Test.File")]
        [InlineData(new string[] { _reg + @"^C:.*", _reg + @"\\bin\\((Debug\\)|(Release\\))Test\.\w+\.\w+\\" }, @"F:\bin\Debug\Test.File")]
        [InlineData(new string[] { _reg + @"\\bin\\Release\\ver[147]\\" }, @"C:\bin\Release\ver10")]
        [InlineData(new string[] { _reg + @"^C:\\bin\\Release\\ver[147]\\$" }, @"C:\bin\Release\ver4\samples")]
        public void Directory_Include_False(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Directories = filters?.ToList();

            // Act
            var result = options.IsDirectoryNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(new string[] { @"C:\bin\Debug\Test.File\" }, @"C:\bin\Debug\Test.File")]
        [InlineData(new string[] { @"D:\bin\\Debug\Test.File\", _reg + @"\\bin\\((Debug\\)|(Release\\))" }, @"F:\bin\Release\Test.File")]
        [InlineData(new string[] { _reg + @"^C:.*", _reg + @"\\bin\\((Debug\\)|(Release\\))Test\.\w+\.\w+\\" }, @"F:\bin\Debug\Test.File.Helper")]
        [InlineData(new string[] { _reg + @"C:\\bin\\Release\\ver[147]\\" }, @"C:\bin\Release\ver4")]
        public void Directory_Include_Exclude_False(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Directories = filters?.ToList();
            options.Excludes.Directories = filters?.ToList();

            // Act
            var result = options.IsDirectoryNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(FilterOptionsNullTestData.DirectoryData), MemberType = typeof(FilterOptionsNullTestData))]
        public void Directory_Include_Exclude_Null(SourceFilterParams include, SourceFilterParams exclude, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions(include, exclude);

            // Act
            var exception = Record.Exception(() => options.IsDirectoryNeed(s));

            // Assert
            Assert.Null(exception);
        }
        #endregion
        #region IsFolderNeed
        [Theory]
        [InlineData(null, "Test")]
        [InlineData(new string[] {}, "Test")]
        [InlineData(new string[] { "Test" }, "Test")]
        [InlineData(new string[] { "Test1",_reg+"test1",_reg+"Tes[A-Za-z]{3}$" }, "MultyTester")]
        [InlineData(new string[] { _reg + @"^Test\.\w+\.\w+$" }, "Test.String.Parsing")]
        [InlineData(new string[] { _reg + @"^Test\.\w+\.\w+" }, "Test.String.Parsing.New")]
        [InlineData(new string[] {_reg+"est$" }, "Test")]
        public void Folder_Include_True(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Folders= filters?.ToList();

            // Act
            var result = options.IsFolderNeed(s);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(new string[] { "Test" }, "Test1")]
        [InlineData(new string[] { "Test1",_reg+"Tester2.*",_reg+"^Tes[A-Za-z]{3}[1-4]$" }, "MultyTester1")]
        [InlineData(new string[] { _reg + "^Tester2.*", _reg + "^Tes[A-Za-z]{3}[1-4]$" }, "Tester14")]
        [InlineData(new string[] { _reg + @"^Test\.\w+\.\w+$" }, "Test.String.Parsing.New")]
        [InlineData(new string[] {_reg+"est$" }, "Test1")]
        public void Folder_Include_False(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Folders = filters?.ToList();

            // Act
            var result = options.IsFolderNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(new string[] { "Test", "*" }, "Test1")]
        [InlineData(new string[] { "Test" }, "Test")]
        [InlineData(new string[] { "Test1", _reg + "test1", _reg + "Tes[A-Za-z]{3}$" }, "MultyTester")]
        [InlineData(new string[] { _reg + @"^Test\.\w+\.\w+$" }, "Test.String.Parsing")]
        [InlineData(new string[] { _reg + @"^Test\.\w+\.\w+" }, "Test.String.Parsing.New")]
        [InlineData(new string[] { _reg + "est$" }, "Test")]
        public void Folder_Include_Exclude_False(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Folders = filters?.ToList();
            options.Excludes.Folders = filters?.ToList();

            // Act
            var result = options.IsFolderNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(FilterOptionsNullTestData.FolderData), MemberType = typeof(FilterOptionsNullTestData))]

        public void Folder_Include_Exclude_Null(SourceFilterParams include, SourceFilterParams exclude, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions(include, exclude);

            // Act
            var exception = Record.Exception(() => options.IsFolderNeed(s));

            // Assert
            Assert.Null(exception);
        }
        #endregion
        #region IsFileNeed
        [Theory]
        [InlineData(null, "Test.cs")]
        [InlineData(new string[] {}, "Test.cs")]
        [InlineData(new string[] { "Test.cs" }, "Test.cs")]
        [InlineData(new string[] { "Test1.cs",_reg+@"test1\w+",_reg+ @"^Test\.([\w-]+\.)+cs$" }, "Test.String.New5.cs")]
        [InlineData(new string[] {_reg+@"\.cs$" }, "Test.String.cs")]
        public void File_Include_True(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Files = filters?.ToList();

            // Act
            var result = options.IsFileNeed(s);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(new string[] { "Test.cs" }, "Test1.cs")]
        [InlineData(new string[] { "Test1.cs", _reg + @"test1\w+", _reg + @"^Test\.([\w-]+\.)+cs$" }, "SomeTest.String.New5.cs")]
        [InlineData(new string[] { _reg + @"\.cs$" }, "Test.String.ts")]
        public void File_Include_False(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Files = filters?.ToList();

            // Act
            var result = options.IsFileNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(new string[] { "Test.cs" }, "Test.cs")]
        [InlineData(new string[] { "Test1.cs", _reg + @"test1\w+", _reg + @"^Test\.([\w-]+\.)+cs$" }, "Test.String.New5.cs")]
        [InlineData(new string[] { _reg + @"\.cs$" }, "Test.String.cs")]
        public void File_Include_Exclude_False(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Files = filters?.ToList();
            options.Excludes.Files = filters?.ToList();

            // Act
            var result = options.IsFileNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(FilterOptionsNullTestData.FileData), MemberType = typeof(FilterOptionsNullTestData))]
        public void File_Include_Exclude_Null(SourceFilterParams include, SourceFilterParams exclude, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions(include, exclude);

            // Act
            var exception = Record.Exception(() => options.IsFileNeed(s));

            // Assert
            Assert.Null(exception);
        }
        #endregion
        #region IsNamespaceNeed
        [Theory]
        [InlineData(null, "Drill4Net")]
        [InlineData(new string[] {}, "Drill4Net")]
        [InlineData(new string[] { "Drill4Net" }, "Drill4Net")]
        [InlineData(new string[] { "Benchmark", "Drill4Net.Common" }, "Drill4Net.Common.Utils")]
        [InlineData(new string[] { _reg + @"^Common", _reg + @"Injector\.Strategies\." }, "Drill4Net.Injector.Strategies.Blocks")]
        [InlineData(new string[] { _reg + @"^Common", _reg + @"Injector\.Strategies\." }, "CommonData.Utils")]
        [InlineData(new string[] { _reg + @"^Drill4Net\.([\w-]+\.)+[\w]*Tests$" }, "Drill4Net.BanderLog.CommonTests")]
        [InlineData(new string[] { _reg + @"^Drill4Net\.([\w-]+\.)+[\w]*Tests$" }, "Drill4Net.BanderLog.Tests")]
        [InlineData(new string[] { _reg + @"Constants$" }, "Drills4Net.CommonConstants")]
        public void Namespace_Include_True(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Namespaces = filters?.ToList();

            // Act
            var result = options.IsNamespaceNeed(s);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(new string[] { "Drill4Net" }, "")]
        [InlineData(new string[] { "Drill4Net" }, null)]
        [InlineData(new string[] { "Drill4Net" }, "Drill4J")]
        [InlineData(new string[] { "Drill4Net" }, "EPAM.Drill4Net")] //in simple string we must use only the start of the namespace
        [InlineData(new string[] { _reg + @"^Common", _reg + @"Injector\.Strategies\." }, "Drill4Net.Injector.New.Strategies.Blocks")]
        [InlineData(new string[] { _reg + @"^Drill4Net\.([\w-]+\.)+[\w]*Tests$" }, "Drill4Net.BanderLog.CommonTest")]
        [InlineData(new string[] { _reg + @"^Drill4Net\.([\w-]+\.)+[\w]*Tests$" }, "Drill.Drill4Net.BanderLog.Tests")]
        [InlineData(new string[] { _reg + @"Constants$" }, "Drills4Net.CommonConstantsUtil")]
        public void Namespace_Include_False(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Namespaces = filters?.ToList();


            // Act
            var result = options.IsNamespaceNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(new string[] { "Drill4Net" }, "Drill4Net")]
        [InlineData(new string[] { "Benchmark", "Drill4Net.Common" }, "Drill4Net.Common.Utils")]
        [InlineData(new string[] { _reg + @"^Common", _reg + @"Injector\.Strategies\." }, "Drill4Net.Injector.Strategies.Blocks")]
        [InlineData(new string[] { _reg + @"^Common", _reg + @"Injector\.Strategies\." }, "CommonData.Utils")]
        [InlineData(new string[] { _reg + @"^Drill4Net\.([\w-]+\.)+[\w]*Tests$" }, "Drill4Net.BanderLog.CommonTests")]
        [InlineData(new string[] { _reg + @"^Drill4Net\.([\w-]+\.)+[\w]*Tests$" }, "Drill4Net.BanderLog.Tests")]
        [InlineData(new string[] { _reg + @"Constants$" }, "Drills4Net.CommonConstants")]
        public void Namespace_Include_Exclude_False(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Namespaces = filters?.ToList();
            options.Excludes.Namespaces = filters?.ToList();
            // Act
            var result = options.IsNamespaceNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(FilterOptionsNullTestData.NamespaceData), MemberType = typeof(FilterOptionsNullTestData))]
        public void Namespace_Include_Exclude_Null(SourceFilterParams include, SourceFilterParams exclude, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions(include, exclude);

            // Act
            var exception = Record.Exception(() => options.IsNamespaceNeed(s));

            // Assert
            Assert.Null(exception);
        }
        #endregion
        #region IsClassNeed
        [Theory]
        [InlineData(null, "InjectorCoreConstants")]
        [InlineData(new string[] {}, "InjectorCoreConstants")]
        [InlineData(new string[] { "InjectorCoreConstants" }, "InjectorCoreConstants")]
        [InlineData(new string[] { "InjectorCore", _reg + "InjectorConstants", _reg + "CoreConstants$" }, "InjectorCoreConstants")]
        [InlineData(new string[] { _reg + @"Injector\w+Constants" }, "InjectorCoreConstants")]
        [InlineData(new string[] { _reg + @"Injector\w+Constants$" }, "InjectorCommon5Constants")]
        public void Class_Include_True(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Classes = filters?.ToList();

            // Act
            var result = options.IsClassNeed(s);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(new string[] { "InjectorCoreConstants" }, "InjectorConstants")]
        [InlineData(new string[] { "InjectorCore", _reg + "InjectorConstants", _reg + "CoreConstants$" }, "InjectorCommonConstants")]
        [InlineData(new string[] { _reg + @"Injector\w+Constants" }, "InjectorCoreConst")]
        [InlineData(new string[] { _reg + @"^Injector\w+Constants" }, "NewInjectorCommon5Constants")]
        public void Class_Include_False(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Classes = filters?.ToList();

            // Act
            var result = options.IsClassNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(new string[] { "InjectorCoreConstants" }, "InjectorCoreConstants")]
        [InlineData(new string[] { "InjectorCore", _reg + "InjectorConstants", _reg + "CoreConstants$" }, "InjectorCoreConstants")]
        [InlineData(new string[] { _reg + @"Injector\w+Constants" }, "InjectorCoreConstants")]
        [InlineData(new string[] { _reg + @"Injector\w+Constants$" }, "InjectorCommon5Constants")]
        public void Class_Include_Exclude_False(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Classes = filters?.ToList();
            options.Excludes.Classes = filters?.ToList();

            // Act
            var result = options.IsClassNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(FilterOptionsNullTestData.ClassData), MemberType = typeof(FilterOptionsNullTestData))]
        public void Class_Include_Exclude_Null(SourceFilterParams include, SourceFilterParams exclude, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions(include, exclude);

            // Act
            var exception = Record.Exception(() => options.IsClassNeed(s));

            // Assert
            Assert.Null(exception);
        }
        #endregion
        #region IsAttributeNeed
        [Theory]
        [InlineData(null, "CustomAttribute")]
        [InlineData(new string[] {}, "CustomAttribute")]
        [InlineData(new string[] { "CustomAttribute" }, "CustomAttribute")]
        [InlineData(new string[] { "CustomAttribute", _reg + "^Core", _reg + "Core" }, "CustomCoreAttribute")]
        [InlineData(new string[] { "CustomAttribute", _reg + "^Core" }, "CoreAttribute")]
        [InlineData(new string[] { _reg + @"Custom\w+Attribute$" }, "CustomCoreAttribute")]
        public void Attribute_Include_True(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Attributes = filters?.ToList();

            // Act
            var result = options.IsAttributeNeed(s);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(new string[] { "CustomAttribute" }, "CoreAttribute")]
        [InlineData(new string[] { "CustomAttribute", _reg + "^Core", _reg + "Core" }, "CustAttribute")]
        [InlineData(new string[] { "CustomAttribute", _reg + "^Core" }, "NewCoreAttribute")]
        [InlineData(new string[] { _reg + @"Custom\w+Attribute$" }, "CustomCoreAttributeNew")]
        public void Attribute_Include_False(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Attributes = filters?.ToList();

            // Act
            var result = options.IsAttributeNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(new string[] { "CustomAttribute" }, "CustomAttribute")]
        [InlineData(new string[] { "CustomAttribute", _reg + "^Core", _reg + "Core" }, "CustomCoreAttribute")]
        [InlineData(new string[] { "CustomAttribute", _reg + "^Core" }, "CoreAttribute")]
        [InlineData(new string[] { _reg + @"Custom\w+Attribute$" }, "CustomCoreAttribute")]
        public void Attribute_Include_Exclude_False(string[] filters, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions();
            options.Includes.Attributes = filters?.ToList();
            options.Excludes.Attributes = filters?.ToList();

            // Act
            var result = options.IsAttributeNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(FilterOptionsNullTestData.AttributeData), MemberType = typeof(FilterOptionsNullTestData))]
        public void Attribute_Include_Exclude_Null(SourceFilterParams include, SourceFilterParams exclude, string s)
        {
            // Arrange
            var options = _dataHelper.CreateSourceFilterOptions(include, exclude);

            // Act
            var exception = Record.Exception(() => options.IsAttributeNeed(s));

            // Assert
            Assert.Null(exception);
        }
        #endregion
    }
}

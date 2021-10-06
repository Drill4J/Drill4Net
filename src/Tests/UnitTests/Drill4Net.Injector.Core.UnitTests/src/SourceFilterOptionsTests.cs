﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;
using Drill4Net.Common;

namespace Drill4Net.Injector.Core.UnitTests
{
    /// <summary>
    /// Tests for filters of directories, folders, namespaces, type names, etc.
    /// </summary>
    public class SourceFilterOptionsTests
    {
        private const string _reg= Drill4Net.Common.CoreConstants.REGEX_FILTER_PPREFIX;

        /*************************************************************************************/

        #region IsDirectoryNeed
        [Theory]
        [InlineData(new string[] {}, @"C:\bin\Debug\Test.File")]
        [InlineData(new string[] { @"C:\bin\Debug\Test.File\" }, @"C:\bin\Debug\Test.File")]
        [InlineData(new string[] { @"D:\bin\\Debug\Test.File\", _reg+ @"\\bin\\((Debug\\)|(Release\\))" }, @"F:\bin\Release\Test.File")]
        [InlineData(new string[] { _reg + @"^C:.*", _reg + @"\\bin\\((Debug\\)|(Release\\))Test\.\w+\.\w+\\" }, @"F:\bin\Debug\Test.File.Helper")]
        [InlineData(new string[] {_reg+ @"C:\\bin\\Release\\ver[147]\\" }, @"C:\bin\Release\ver4")]        
        public void IsDirectoryNeedTrueIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Directories = filters.ToList();


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
        public void IsDirectoryNeedFalseIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Directories = filters.ToList();

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
        public void IsDirectoryNeedFalseExcludeIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Excludes.Directories = filters.ToList();
            options.Includes.Directories = filters.ToList();

            // Act
            var result = options.IsDirectoryNeed(s);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsFolderNeed
        [Theory]
        [InlineData(new string[] {}, "Test")]
        [InlineData(new string[] { "Test" }, "Test")]
        [InlineData(new string[] { "Test1",_reg+"test1",_reg+"Tes[A-Za-z]{3}$" }, "MultyTester")]
        [InlineData(new string[] { _reg + @"^Test\.\w+\.\w+$" }, "Test.String.Parsing")]
        [InlineData(new string[] { _reg + @"^Test\.\w+\.\w+" }, "Test.String.Parsing.New")]
        [InlineData(new string[] {_reg+"est$" }, "Test")]
        public void IsFolderNeedTrueIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Folders = filters.ToList();

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
        public void IsFolderNeedFalseIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Folders = filters.ToList();

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
        public void IsFolderNeedFalseExcludeIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Folders = filters.ToList();
            options.Excludes.Folders = filters.ToList();

            // Act
            var result = options.IsFolderNeed(s);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region IsFileNeed
        [Theory]
        [InlineData(new string[] {}, "Test.cs")]
        [InlineData(new string[] { "Test.cs" }, "Test.cs")]
        [InlineData(new string[] { "Test1.cs",_reg+@"test1\w+",_reg+ @"^Test\.([\w-]+\.)+cs$" }, "Test.String.New5.cs")]
        [InlineData(new string[] {_reg+@"\.cs$" }, "Test.String.cs")]
        public void IsFileNeedTrueIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Files = filters.ToList();

            // Act
            var result = options.IsFileNeed(s);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(new string[] { "Test.cs" }, "Test1.cs")]
        [InlineData(new string[] { "Test1.cs", _reg + @"test1\w+", _reg + @"^Test\.([\w-]+\.)+cs$" }, "SomeTest.String.New5.cs")]
        [InlineData(new string[] { _reg + @"\.cs$" }, "Test.String.ts")]
        public void IsFileNeedFalseIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Files = filters.ToList();

            // Act
            var result = options.IsFileNeed(s);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(new string[] { "Test.cs" }, "Test.cs")]
        [InlineData(new string[] { "Test1.cs", _reg + @"test1\w+", _reg + @"^Test\.([\w-]+\.)+cs$" }, "Test.String.New5.cs")]
        [InlineData(new string[] { _reg + @"\.cs$" }, "Test.String.cs")]
        public void IsFileNeedFalseExcludeIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Files = filters.ToList();
            options.Excludes.Files = filters.ToList();

            // Act
            var result = options.IsFileNeed(s);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region IsNamespaceNeed
        [Theory]
        [InlineData(new string[] {}, "Drill4Net")]
        [InlineData(new string[] { "Drill4Net" }, "Drill4Net")]
        [InlineData(new string[] { "Benchmark", "Drill4Net.Common" }, "Drill4Net.Common.Utils")]
        [InlineData(new string[] { _reg + @"^Common", _reg + @"Injector\.Strategies\." }, "Drill4Net.Injector.Strategies.Blocks")]
        [InlineData(new string[] { _reg + @"^Common", _reg + @"Injector\.Strategies\." }, "CommonData.Utils")]
        [InlineData(new string[] { _reg + @"^Drill4Net\.([\w-]+\.)+[\w]*Tests$" }, "Drill4Net.BanderLog.CommonTests")]
        [InlineData(new string[] { _reg + @"^Drill4Net\.([\w-]+\.)+[\w]*Tests$" }, "Drill4Net.BanderLog.Tests")]
        [InlineData(new string[] { _reg + @"Constants$" }, "Drills4Net.CommonConstants")]
        public void IsNamespaceNeedTrueIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Namespaces = filters.ToList();

            // Act
            var result = options.IsNamespaceNeed(s);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(new string[] { "Drill4Net" }, "")]
        [InlineData(new string[] { "Drill4Net" }, null)]
        [InlineData(new string[] { "Drill4Net" }, "Drill4J")]
        [InlineData(new string[] { _reg + @"^Common", _reg + @"Injector\.Strategies\." }, "Drill4Net.Injector.New.Strategies.Blocks")]
        [InlineData(new string[] { _reg + @"^Drill4Net\.([\w-]+\.)+[\w]*Tests$" }, "Drill4Net.BanderLog.CommonTest")]
        [InlineData(new string[] { _reg + @"^Drill4Net\.([\w-]+\.)+[\w]*Tests$" }, "Drill.Drill4Net.BanderLog.Tests")]
        [InlineData(new string[] { _reg + @"Constants$" }, "Drills4Net.CommonConstantsUtil")]
        public void IsNamespaceNeedFalseIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Namespaces = filters.ToList();

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
        public void IsNamespaceNeedFalseExcludeIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Namespaces = filters.ToList();
            options.Excludes.Namespaces = filters.ToList();

            // Act
            var result = options.IsNamespaceNeed(s);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region IsClassNeed
        [Theory]
        [InlineData(new string[] {}, "InjectorCoreConstants")]
        [InlineData(new string[] { "InjectorCoreConstants" }, "InjectorCoreConstants")]
        [InlineData(new string[] { "InjectorCore", _reg + "InjectorConstants", _reg + "CoreConstants$" }, "InjectorCoreConstants")]
        [InlineData(new string[] { _reg + @"Injector\w+Constants" }, "InjectorCoreConstants")]
        [InlineData(new string[] { _reg + @"Injector\w+Constants$" }, "InjectorCommon5Constants")]
        public void IsClassNeedTrueIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Classes = filters.ToList();

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
        public void IsClassNeedFalseIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Classes = filters.ToList();

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
        public void IsClassNeedFalseExcludeIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Classes = filters.ToList();
            options.Excludes.Classes = filters.ToList();

            // Act
            var result = options.IsClassNeed(s);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region IsAttributeNeed
        [Theory]
        [InlineData(new string[] {}, "CustomAttribute")]
        [InlineData(new string[] { "CustomAttribute" }, "CustomAttribute")]
        [InlineData(new string[] { "CustomAttribute", _reg + "^Core", _reg + "Core" }, "CustomCoreAttribute")]
        [InlineData(new string[] { "CustomAttribute", _reg + "^Core" }, "CoreAttribute")]
        [InlineData(new string[] { _reg + @"Custom\w+Attribute$" }, "CustomCoreAttribute")]
        public void IsAttributeNeedTrueIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Attributes = filters.ToList();

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
        public void IsAttributeNeedFalseIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Attributes = filters.ToList();

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
        public void IsAttributeNeedFalseExcludeIncludeFilter(string[] filters, string s)
        {
            // Arrange
            var options = new SourceFilterOptions();
            options.Includes = new SourceFilterParams();
            options.Excludes = new SourceFilterParams();
            options.Includes.Attributes = filters.ToList();
            options.Excludes.Attributes = filters.ToList();

            // Act
            var result = options.IsAttributeNeed(s);

            // Assert
            Assert.False(result);
        }
        #endregion
    }
}

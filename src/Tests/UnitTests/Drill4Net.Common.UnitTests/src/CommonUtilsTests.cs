using System;
using Xunit;

namespace Drill4Net.Common.UnitTests
{
    public class CommonUtilsTests
    {
        private const string module = "<Module>";

        #region DeconstructFullTypeName
        [Theory]
        [InlineData("Drill4Net.Target.Common.Another.AnotherTarget", "Drill4Net.Target.Common.Another", "AnotherTarget")]
        [InlineData(module, null, module)]
        [InlineData("", null, "")]
        public void Deconstruct_Full_TypeName_Test(string typeFullName, string expectedNs, string expectedType)
        {
            // Arrange       
            // Act
            var result = CommonUtils.DeconstructFullTypeName(typeFullName);

            // Assert
            Assert.Equal(expectedNs, result.ns);
            Assert.Equal(expectedType, result.type);
        }
        #endregion
        #region GetRootNamespace
        [Theory]
        [InlineData("Drill4Net.Target.Common.Another.AnotherTarget", "Drill4Net")]
        [InlineData(module,  null)]
        public void Get_Root_Namespace_Test(string typeFullName, string expectedResult)
        {
            // Arrange       
            // Act
            var result = CommonUtils.GetRootNamespace(typeFullName);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Get_Root_Namespace_Throws_ArgumentNullException(string typeFullName)
        {
            // Arrange       
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => CommonUtils.GetRootNamespace(typeFullName));

        }
        #endregion
        #region GetTypeByMethod
        [Theory]
        [InlineData("Drill4Net.Target.Common.Another.AnotherTarget", "Drill4Net")]
        [InlineData(module, null)]
        public void Get_Type_By_Method_Test(string typeFullName, string expectedResult)
        {
            // Arrange       
            // Act
        

            // Assert

        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Get_Type_By_Method_Throws_ArgumentNullException(string typeFullName)
        {
            // Arrange       
            // Act
            // Assert

        }
        #endregion
    }
}

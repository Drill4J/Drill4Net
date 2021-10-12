using System;
using Xunit;

namespace Drill4Net.Common.UnitTests
{
    public class CommonUtilsTests
    {
        #region DeconstructFullTypeName
        [Theory]
        [InlineData("Drill4Net.Target.Common.Another.AnotherTarget", "Drill4Net.Target.Common.Another", "AnotherTarget")]
        [InlineData("Drill4Net.Common", "Drill4Net", "Common")]
        public void Deconstruct_Full_TypeName_Test(string typeFullName, string expectedNs, string expectedType)
        {
            // Arrange       
            // Act
            var result = CommonUtils.DeconstructFullTypeName(typeFullName);

            // Assert
            Assert.Equal(expectedNs, result.ns);
            Assert.Equal(expectedType, result.type);
        }

        [Theory]
        [InlineData("<Module>")]
        [InlineData("")]
        public void Deconstruct_Full_TypeName_Null(string typeFullName)
        {
            // Arrange       
            // Act
            var result = CommonUtils.DeconstructFullTypeName(typeFullName);

            // Assert
            Assert.Null(result.ns);
            Assert.Equal(typeFullName, result.type);
        }
        #endregion
        #region GetRootNamespace
        [Theory]
        [InlineData("Drill4Net.Target.Common.Another.AnotherTarget", "Drill4Net")]
        public void Get_Root_Namespace_Test(string typeFullName, string expectedResult)
        {
            // Arrange       
            // Act
            var result = CommonUtils.GetRootNamespace(typeFullName);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("Drill4Net")]
        public void Get_Root_Namespace_Null(string typeFullName)
        {
            // Arrange       
            // Act
            var result = CommonUtils.GetRootNamespace(typeFullName);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
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
        [InlineData("System.Int32 Drill4Net.Target.Common.NotEmptyStringEnumerator::GetPosition()", "Drill4Net.Target.Common.NotEmptyStringEnumerator")]
        [InlineData("System.Int32 Drill4Net.Target.Common.NotEmptyStringEnumerator:GetPosition()", "Drill4Net.Target.Common.NotEmptyStringEnumerator")]
        [InlineData("System.Int32 Drill4Net.Target:Common.NotEmptyStringEnumerator::GetPosition()", "Drill4Net.Target")]
        [InlineData(" :","")]
        public void Get_Type_By_Method_Test(string typeFullName, string expectedResult)
        {
            // Arrange       
            // Act    
            var result = CommonUtils.GetTypeByMethod(typeFullName);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("System.Int32 GetPosition()")]
        [InlineData("Drill4Net.Target.Common.NotEmptyStringEnumerator::GetPosition()")]
        public void Get_Type_By_Method_Null(string typeFullName)
        {
            // Arrange       
            // Act
            var result = CommonUtils.GetTypeByMethod(typeFullName);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Get_Type_By_Method_Throws_ArgumentNullException(string methodFullName)
        {
            // Arrange       
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => CommonUtils.GetTypeByMethod(methodFullName));
        }
        #endregion
    }
}
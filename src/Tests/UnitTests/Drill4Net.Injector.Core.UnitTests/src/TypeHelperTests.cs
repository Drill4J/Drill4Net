﻿using System.Collections.Generic;
using Xunit;
using Drill4Net.Common;

namespace Drill4Net.Injector.Core.UnitTests
{
    public class TypeHelperTests
    {
        [Theory]
        [MemberData(nameof(TypeHelperTestData.TypeHelperTrueData), MemberType = typeof(TypeHelperTestData))]
        public void Type_Need_True(SourceFilterOptions flt, string typeFullName, IEnumerable<string> attributes)
        {
            // Arrange
            // Act
            var result = TypeHelper.IsTypeNeed(flt, typeFullName, attributes);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(TypeHelperTestData.TypeHelperFalseData), MemberType = typeof(TypeHelperTestData))]
        public void Type_Need_False(SourceFilterOptions flt, string typeFullName, IEnumerable<string> attributes)
        {
            // Arrange
            // Act
            var result = TypeHelper.IsTypeNeed(flt,typeFullName, attributes);

            // Assert
            Assert.False(result);
        }
    }
}

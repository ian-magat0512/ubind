// <copyright file="EnumTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Helpers
{
    using UBind.Domain.Helpers;
    using UBind.Domain.Permissions;
    using Xunit;

    public class EnumTests
    {
        [Fact]
        public void GetValues_ReturnsAllValuesInEnum()
        {
            // Act
            var values = Enum<RoleType>.GetValues();

            // Assert
            // Proves applicability of Linq methods to return value of GetValues().
            Assert.Contains(values, v => v > RoleType.Master);
        }
    }
}

// <copyright file="MoqTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1649 // File name should match first type name

namespace UBind.Application.Tests.Fakes
{
    using System;
    using Moq;
    using Xunit;

    public interface ITestInterface
    {
        void TestMethod(Guid optionalParameter = default);
    }

    public class MoqTests
    {
        [Fact]
        public void Moq_SupportsMethodsWithOptionalStructParameters_WhenDefaultValueIsDefault()
        {
            // Arrange
            var mock = new Mock<ITestInterface>();

            // Act
            var obj = mock.Object;

            // Assert
            Assert.NotNull(obj);
        }
    }
}

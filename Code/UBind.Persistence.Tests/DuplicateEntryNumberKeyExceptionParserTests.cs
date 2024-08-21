// <copyright file="DuplicateEntryNumberKeyExceptionParserTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using UBind.Domain;
    using Xunit;

    public class DuplicateEntryNumberKeyExceptionParserTests
    {
        [Fact]
        public void ParsingSucceedsWhenMessageIsAsExpected()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var message = $"Cannot insert duplicate key row in object 'dbo.PolicyNumbers' with unique index 'AK_PolicyNumberProductEnvironmentAndNumberIndex'. The duplicate key value is ({tenantId.ToString()}, {productId.ToString()}, 1, AAAAAA).\r\nThe statement has been terminated.";

            // Act
            var sut = new DuplicateEntryNumberKeyExceptionParser(tenantId, productId, environment, message);

            // Assert
            Assert.True(sut.Succeeded);
            Assert.Equal("AAAAAA", sut.Number);
        }
    }
}

// <copyright file="FileContentTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.ReadWriteModel
{
    using System;
    using System.Text;
    using FluentAssertions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;
    using Xunit;

    public class FileContentTests
    {
        [Fact]
        public void CreateFromContent_GeneratesHashCode_WhenCalled()
        {
            // Arrange
            var content = "Hello World!";
            var contentInBytes = Encoding.ASCII.GetBytes("Hello World!");

            // Act
            var fileContent = FileContent.CreateFromBytes(Guid.NewGuid(), Guid.NewGuid(), contentInBytes);

            // Assert
            fileContent.HashCode.Should().Be(content.GetHashString());
        }
    }
}

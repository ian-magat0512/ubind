// <copyright file="AssetTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests
{
    using System;
    using System.Text;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;
    using Xunit;

    public class AssetTests
    {
        [Fact]
        public void Constructor_SerializesStringContentsToByteArrayUsingUTF8()
        {
            // Arrange
            var filename = "foo.txt";
            var contents = "blah";
            var expectedByteArray = Encoding.UTF8.GetBytes(contents);
            var fileContent = FileContent.CreateFromBytes(Guid.NewGuid(), Guid.NewGuid(), expectedByteArray);

            // Act
            var asset = new Asset(Guid.NewGuid(), filename, SystemClock.Instance.Now(), fileContent, SystemClock.Instance.Now());

            // Assert
            asset.FileContent.Content.Should().Equal(expectedByteArray);
        }
    }
}

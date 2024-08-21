// <copyright file="CryptographyHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Helpers
{
    using System.Security.Cryptography;
    using System.Text;
    using FluentAssertions;
    using UBind.Domain.Helpers;
    using Xunit;

    public class CryptographyHelperTests
    {
        [Theory]
        [InlineData("Hello World!", "7F83B1657FF1FC53B92DC18148A1D65DFC2D4B1FA3D677284ADDD200126D9069")]
        [InlineData("uBind Australia", "2337354C1BBF2D8BB964B55B75AB5DE901C9F38284EB245B55CF4EE31AA4C6DB")]
        public void ComputeHashString_ReturnsCorrectHashString_WhenByteArrayIsPassedAsParameter(
            string content, string expectedHashCode)
        {
            // Arrange
            var contentInBytes = Encoding.ASCII.GetBytes(content);

            // Act
            var hashCode = CryptographyHelper.ComputeHashString(contentInBytes);

            // Assert
            hashCode.Should().Be(expectedHashCode);
        }

        [Theory]
        [InlineData("The quick brown fox jumps over the lazy dog.")]
        [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit")]
        public void ComputeHashString_ReturnsTheSameHash_WhenComparedToDefaultSha256Algorithm(string content)
        {
            // Arrange
            var contentInBytes = Encoding.ASCII.GetBytes(content);

            // Act
            var hashCode = CryptographyHelper.ComputeHashString(contentInBytes);
            var hashCodeSha256 = CryptographyHelper.ComputeHashString(contentInBytes, HashAlgorithmName.SHA256);

            // Assert
            hashCode.Should().Be(hashCodeSha256);
        }
    }
}

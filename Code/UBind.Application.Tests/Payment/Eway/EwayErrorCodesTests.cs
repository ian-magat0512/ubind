// <copyright file="EwayErrorCodesTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Payment.Eway
{
    using UBind.Application.Payment.Eway;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="EwayErrorCodesTests" />.
    /// </summary>
    public class EwayErrorCodesTests
    {
        /// <summary>
        /// The Indexer_ReturnCorrectMessage_ForKnownCode.
        /// </summary>
        [Fact]
        public void Indexer_ReturnCorrectMessage_ForKnownCode()
        {
            // Arrange
            var sut = new EwayErrorCodes();

            // Act
            var message = sut["V6033"];

            // Assert
            Assert.Equal("Invalid Expiry Date", message);
        }

        /// <summary>
        /// The Indexer_ReturnGenericMessage_ForUnknownCode.
        /// </summary>
        [Fact]
        public void Indexer_ReturnGenericMessage_ForUnknownCode()
        {
            // Arrange
            var sut = new EwayErrorCodes();

            // Act
            var message = sut["foobar"];

            // Assert
            Assert.Equal("Payment Error (foobar)", message);
        }
    }
}

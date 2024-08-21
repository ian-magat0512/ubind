// <copyright file="InternetAddressHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Helpers
{
    using System.Collections.Generic;
    using UBind.Domain.Helpers;
    using Xunit;

    public class InternetAddressHelperTests
    {
        [Fact]
        public void ConvertEmailAddressesToMailBoxAddresses_ConvertsEmail_WithSemicolons()
        {
            // Arrange
            IEnumerable<string> emailAddresses = new List<string>
        {
            "email1@domain.com;email2@domain.com",
            "email3@domain.com ; email4@domain.com",
        };
            int expectedCount = 4;

            // Act
            var result = InternetAddressHelper.ConvertEmailAddressesToMailBoxAddresses(emailAddresses);

            // Assert
            Assert.Equal(expectedCount, result.Count());
        }

        [Fact]
        public void ConvertEmailAddressesToMailBoxAddresses_ConvertsEmail_WithOutSemicolon()
        {
            // Arrange
            IEnumerable<string> emailAddresses = new List<string>
        {
            "email1@domain.com",
            "email2@domain.com",
        };
            int expectedCount = 2;

            // Act
            var result = InternetAddressHelper.ConvertEmailAddressesToMailBoxAddresses(emailAddresses);

            // Assert
            Assert.Equal(expectedCount, result.Count());
        }
    }
}

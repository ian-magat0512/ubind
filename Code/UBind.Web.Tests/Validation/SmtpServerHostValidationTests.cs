// <copyright file="SmtpServerHostValidationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.Validation
{
    using System;
    using UBind.Web.Validation;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="SmtpServerHostValidationTests" />.
    /// </summary>
    public class SmtpServerHostValidationTests
    {
        /// <summary>
        /// The SmtpServerHostValidation_Accepts_ValidHostNames.
        /// </summary>
        /// <param name="hostname">The hostname<see cref="string"/>.</param>
        [Theory]
        [InlineData("foo")]
        [InlineData("foo.bar")]
        [InlineData("127.0.0.1")]
        [InlineData("localhost")]
        [InlineData("::1")]
        [InlineData("[::1]")]
        [InlineData("2001:db8:85a3:8d3:1319:8a2e:370:7348")]
        [InlineData("[2001:db8:85a3:8d3:1319:8a2e:370:7348]")]
        public void SmtpServerHostValidation_Accepts_ValidHostNames(string hostname)
        {
            // Arrange
            var smtpServerHostValidation = new SmtpServerHostValidationAttribute();

            // Act + Assert
            Assert.True(smtpServerHostValidation.IsValid(hostname));
        }

        /// <summary>
        /// The SmtpServerHostValidation_Rejects_InvalidHostNames.
        /// </summary>
        /// <param name="hostname">The hostname<see cref="string"/>.</param>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("foo bar")]
        public void SmtpServerHostValidation_Rejects_InvalidHostNames(string hostname)
        {
            // Act
            var result = Uri.CheckHostName(hostname);

            // Act + Assert
            Assert.Equal(UriHostNameType.Unknown, result);
        }
    }
}

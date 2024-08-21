// <copyright file="TenantHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Helpers
{
    using System;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Helpers;
    using Xunit;

    public class TenantHelperTests
    {
        [Theory]
        [InlineData("Policy Title", "Policy Message", "Protection Title", "Protection Message", true)]
        [InlineData("policy Title", "policy Message", "protection Title", "protection Message", true)]
        [InlineData("Premium Title", "Premium Message", "Contribution Title", "Contribution Message", true)]
        [InlineData("premium Title", "premium Message", "contribution Title", "contribution Message", true)]
        [InlineData("Insurance Title", "Insurance Message", "Risk Protection Title", "Risk Protection Message", true)]
        [InlineData("insurance Title", "insurance Message", "risk protection Title", "risk protection Message", true)]
        [InlineData("Indemnity Title", "Indemnity Message", "Protection Title", "Protection Message", true)]
        [InlineData("indemnity Title", "indemnity Message", "protection Title", "protection Message", true)]
        [InlineData("Policy Title", "Policy Message", "Policy Title", "Policy Message", false)]
        [InlineData("Insurer Insured Insure", "Insurer Insured Insure", "Product issuer Member Cover", "Product issuer Member Cover", true)]
        public void TenantHelper_CheckAndChangeTextToMutualForErrorObject_ReturnsCorrectTextIfMutualOrNot(string title, string message, string expectedTitle, string expectedMessage, bool isMutual)
        {
            // Act
            TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);

            // Assert
            title.Should().Be(expectedTitle);
            message.Should().Be(expectedMessage);
        }

        [Theory]
        [InlineData("Policy Text", "Protection Text", true)]
        [InlineData("policy Text", "protection Text", true)]
        [InlineData("Premium Text", "Contribution Text", true)]
        [InlineData("premium Text", "contribution Text", true)]
        [InlineData("Insurance Text", "Risk Protection Text", true)]
        [InlineData("insurance Text", "risk protection Text", true)]
        [InlineData("Indemnity Text", "Protection Text", true)]
        [InlineData("indemnity Text", "protection Text", true)]
        [InlineData("Insurer Insured Insure", "Product issuer Member Cover", true)]
        [InlineData("insurer insured insure", "product issuer member cover", true)]
        [InlineData("Policy Text", "Policy Text", false)]
        public void TenantHelper_CheckAndChangeTextToMutual_ReturnsCorrectTextIfMutualOrNot(string text, string expectedText, bool isMutual)
        {
            // Act
            text = TenantHelper.CheckAndChangeTextToMutual(text, isMutual);

            // Assert
            text.Should().Be(expectedText);
        }

        [Fact]
        public void TenantHelper_CheckAndChangeTextToMutualForErrorObject_CreateErrorObject_ReturnsWithMutualTexts()
        {
            var clock = SystemClock.Instance;

            // Act
            // test selected error object functions
            var error1 = Errors.Policy.Adjustment.AdjustmentQuoteExists(Guid.NewGuid(), clock.GetCurrentInstant(), "PO111", true);
            var error2 = Errors.Policy.Renewal.NoPolicyExists(true);

            // Assert
            error1.Title.Should().Be("Protection adjustment in progress");
            error1.Message.Should().Be($"An adjustment quote for the protection PO111 has already been started, but never completed. "
                        + "You may resume the existing adjustment quote, or if you would like to start again, you must cancel the existing adjustment quote first.");
            error2.Title.Should().Be($"There's no protection to renew");
            error2.Message.Should().Be($"You requested to start a renewal quote but there's no protection to renew.");
        }

        [Fact]
        public void TenantHelper_CheckAndChangeTextToMutualForErrorObject_CreateErrorObject_ReturnsWitNonhMutualTexts()
        {
            var clock = SystemClock.Instance;

            // Act
            // test selected error object functions
            var error1 = Errors.Policy.Adjustment.AdjustmentQuoteExists(Guid.NewGuid(), clock.GetCurrentInstant(), "PO111", false);
            var error2 = Errors.Policy.Renewal.NoPolicyExists(false);

            // Assert
            error1.Title.Should().Be("Policy adjustment in progress");
            error1.Message.Should().Be($"An adjustment quote for the policy PO111 has already been started, but never completed. "
                        + "You may resume the existing adjustment quote, or if you would like to start again, you must cancel the existing adjustment quote first.");
            error2.Title.Should().Be($"There's no policy to renew");
            error2.Message.Should().Be($"You requested to start a renewal quote but there's no policy to renew.");
        }
    }
}

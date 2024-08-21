// <copyright file="ValidationExpressionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests
{
    using FluentAssertions;
    using UBind.Web.Validation;
    using Xunit;

    /// <summary>
    /// Tests for validation expressions.
    /// </summary>
    public class ValidationExpressionsTests
    {
        [Theory]
        [InlineData("alias123-abc", true)]
        [InlineData("alias-abc", true)]
        [InlineData("alias-23", true)]
        [InlineData("alias123", true)]
        [InlineData("-alias123", false)]
        [InlineData("alias123-", false)]
        public void AliasRegex_MatchesOrDoesNotMatch_TestString(
            string testString, bool shouldMatch, string reason = null)
        {
            // Act
            var pattern = @ValidationExpressions.Alias;

            // Assert
            if (shouldMatch)
            {
                testString.Should().MatchRegex(pattern, reason);
            }
            else
            {
                testString.Should().NotMatchRegex(pattern, reason);
            }
        }

        [Theory]
        [InlineData("uT3*aaaaaaaa", true)]
        [InlineData("uT3*aaaaaaa", false, "must have a minimum of 12 characters")]
        [InlineData("uT3aaaaaaaaa", false, "must have a special characater")]
        [InlineData("uo3*aaaaaaaa", false, "must have a capital letter")]
        [InlineData("uao*aaaaaaaa", false, "must have a number")]
        [InlineData("PT3*PPPPPPPP", false, "must have a lowercase letter")]
        public void StrongPasswordRegex_MatchesOrDoesNotMatch_TestString(
            string testString, bool shouldMatch, string reason = null)
        {
            // Act
            var pattern = @ValidationExpressions.StrongPassword;

            // Assert
            if (shouldMatch)
            {
                testString.Should().MatchRegex(pattern, reason);
            }
            else
            {
                testString.Should().NotMatchRegex(pattern, reason);
            }
        }

        [Theory]
        [InlineData("04 1234 1234", true)]
        [InlineData("05 1234 1234", true)]
        [InlineData("+61 4 1234 1234", true)]
        [InlineData("+61 5 1234 1234", true)]
        [InlineData("+61 4 1234 123", false, "absolute Australian mobile number must have 11 digits")]
        [InlineData("04 1234 12345", false, "relative Australian mobile number must have 10 digits")]
        [InlineData("03 1234 12345", false, "relative Australian landline number must have 10 digits")]
        [InlineData("0x 1234 1234", false, "phone number must not have letters")]
        [InlineData("(03) 1234 1234", false, "Australian mobile number must not have parenthesis")]
        [InlineData("03 1234 1234", false, "03 rpresents a landline number")]
        [InlineData("+61 3 1234 1234", false, "+61 3 represents a landline number")]
        [InlineData("+61 3 1234 123", false, "must be 11 digits when it starts with +61")]
        public void AustralianMobileNumberRegex_MatchesOrDoesNotMatch_TestString(
            string testString, bool shouldMatch, string reason = null)
        {
            // Act
            var pattern = @ValidationExpressions.AustralianMobileNumber;

            // Assert
            if (shouldMatch)
            {
                testString.Should().MatchRegex(pattern, reason);
            }
            else
            {
                testString.Should().NotMatchRegex(pattern, reason);
            }
        }

        [Theory]
        [InlineData("jeo talavera III'.,", true, "separators are allowed")]
        [InlineData("o.,'", true, "certain special characters are allowed")]
        [InlineData("itmatchesThisString@@'.,", false, "certain special characters are not allowed")]
        [InlineData("jeo talavera III`.,/", false, "slash is not allowed")]
        [InlineData("!@#@)))~~~/!@#", false, "certain special characters are not allowed")]
        public void NameRegex_MatchesOrDoesNotMatch_TestString(
            string testString, bool shouldMatch, string reason = null)
        {
            // Act
            var pattern = @ValidationExpressions.Name;

            // Assert
            if (shouldMatch)
            {
                testString.Should().MatchRegex(pattern, reason);
            }
            else
            {
                testString.Should().NotMatchRegex(pattern, reason);
            }
        }

        [Theory]
        [InlineData("PortalOrg2-1", true, "dashes and numbers are allowed")]
        public void CustomLabelRegex_MatchesOrDoesNotMatch_TestString(
            string testString, bool shouldMatch, string reason = null)
        {
            // Act
            var pattern = @ValidationExpressions.CustomLabel;

            // Assert
            if (shouldMatch)
            {
                testString.Should().MatchRegex(pattern, reason);
            }
            else
            {
                testString.Should().NotMatchRegex(pattern, reason);
            }
        }
    }
}

// <copyright file="TextIsEqualToFilterProviderConfigModelIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests.Application.Automations.Filters
{
    using System.Threading.Tasks;
    using Xunit;

    [Collection("Database collection")]
    [SystemEventTypeExtensionInitialize]
    public class TextIsEqualToFilterProviderConfigModelIntegrationTests : TextFilterProviderConfigModelIntegrationTests
    {
        [Theory]
        [InlineData("", "", true)]
        [InlineData("abcdef", "abcdef", true)]
        [InlineData(" abcdef", " abcdef", true)]
        [InlineData("abcdef ", "abcdef ", true)]
        [InlineData("abc def", "abc def", true)]
        [InlineData(" ", " ", true)]
        [InlineData("  ", "  ", true)]
        [InlineData("!@#$%^&*()", "!@#$%^&*()", true)]
        [InlineData("%", "%", true)]
        [InlineData("*", "*", true)]
        [InlineData("abc def ", "c d", false)]
        [InlineData("abcdef", "abcdef ", true)]
        [InlineData("abcdef", "a", false)]
        [InlineData("abcdef", "", false)]
        [InlineData("%", "a", false)]
        [InlineData("*", "a", false)]
        [InlineData("%", "", false)]
        [InlineData("*", "", false)]
        [InlineData("%", " ", false)]
        [InlineData("*", " ", false)]
        public async Task Subject_CorrectlyFiltersAsync(string text, string substring, bool expectedResult)
        {
            await this.VerifyTextFilterBehaviourAsync("textIsEqualToCondition", text, "isEqualTo", substring, expectedResult);
        }
    }
}

// <copyright file="TextEndsWithFilterProviderConfigModelIntegrationTests.cs" company="uBind">
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
    public class TextEndsWithFilterProviderConfigModelIntegrationTests : TextFilterProviderConfigModelIntegrationTests
    {
        [Theory]
        [InlineData("abcdef", "f", true)]
        [InlineData("abcdef", "ef", true)]
        [InlineData("abcdef", "def", true)]
        [InlineData("abcdef", "", true)]
        [InlineData("abcdef ", " ", true)]
        [InlineData("abcdef ", "ef ", true)]
        [InlineData("abcdef  ", "ef  ", true)]
        [InlineData("abc def", "abc def", true)]
        [InlineData("!@#$%^&*()", "()", true)]
        [InlineData("abcdef", "g", false)]
        [InlineData("abcdef", "gf", false)]
        [InlineData("abcdef", "%", false)]
        [InlineData("abcdef", "*", false)]
        [InlineData("abcdef", " ", false)]
        [InlineData("abcdef ", "  ", false)]
        [InlineData("abc def ", "c", false)]
        public async Task Subject_CorrectlyFiltersAsync(string text, string substring, bool expectedResult)
        {
            await this.VerifyTextFilterBehaviourAsync("textEndsWithCondition", text, "endsWith", substring, expectedResult);
        }
    }
}

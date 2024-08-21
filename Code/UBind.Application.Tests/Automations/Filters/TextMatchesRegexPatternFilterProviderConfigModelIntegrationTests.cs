// <copyright file="TextMatchesRegexPatternFilterProviderConfigModelIntegrationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Filters
{
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class TextMatchesRegexPatternFilterProviderConfigModelIntegrationTests
        : BinaryExpressionFilterProviderConfigModelIntegrationTests<string>
    {
        [Theory]
        [InlineData("apple", "\\\\w*ap\\\\w*", true)]
        [InlineData("staple", "\\\\w*ap\\\\w*", true)]
        [InlineData("flap", "\\\\w*ap\\\\w*", true)]
        [InlineData("butter", "\\\\w*ap\\\\w*", false)]
        [InlineData("13a3b", "\\\\w*ap\\\\w*", false)]
        public async Task Build_CreatesConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(string item, string pattern, bool inclusionExpected)
        {
            // Arrange
            var json = $@"
            {{
                ""textMatchesRegexPatternCondition"": {{
                    ""text"": {{
                        ""objectPathLookupText"": ""#""
                    }},
                    ""regexPattern"": ""{pattern}""
                }}
            }}";

            // Act + Assert
            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Fact]
        public async Task Build_CreatesConditionThatThrowsError_WhenSutIsDeserializedWithAnInvalidPattern()
        {
            var json = $@"
            {{
                ""textMatchesRegexPatternCondition"": {{
                    ""text"": ""Alpha"",
                    ""regexPattern"": ""([A-Z]+\\""
                }}
            }}";

            // Act
            Exception ex = await Record.ExceptionAsync(async () => await this.VerifyConditionResolution(json, "Alpha", false));

            // Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ErrorException>();
        }

        [Fact]
        public async Task Build_CreateConditionThatThrowsError_IfInputIsNotAText()
        {
            var json = $@"
            {{
                ""textMatchesRegexPatternCondition"": {{
                    ""text"": 27,
                    ""regexPattern"": ""([A-Z]+\\""
                }}
            }}";
            var dependencyProvider = new Mock<IServiceProvider>();
            var automationData = MockAutomationData.CreateWithEventTrigger();
            var sut = JsonConvert.DeserializeObject<IBuilder<IFilterProvider>>(json, AutomationDeserializationConfiguration.ModelSettings);
            var items = new List<object> { 27 };

            // Act
            var provider = sut!.Build(dependencyProvider.Object);
            Exception ex = await Record.ExceptionAsync(async () => await provider.Resolve<object>(
                new ProviderContext(automationData),
                new ExpressionScope("foo", Expression.Parameter(typeof(object)))));

            // Assert
            ex.Should().BeOfType<ErrorException>();
            ex.Data.Should().NotBeNull();
        }
    }
}

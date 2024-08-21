// <copyright file="NumberComparisonFilterProviderConfigModelIntegrationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Filters
{
    using System.Linq.Expressions;
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

    /// <summary>
    /// Integration tests for number-related conditions, e.g. numberIsEqualToCondition, numberIsAfterCondition, etc.
    /// </summary>
    [SystemEventTypeExtensionInitialize]
    public class NumberComparisonFilterProviderConfigModelIntegrationTests : BinaryExpressionFilterProviderConfigModelIntegrationTests<decimal>
    {
        [Theory]
        [InlineData("0", "0", true)]
        [InlineData("1.5", "1.5", true)]
        [InlineData("85", "85", true)]
        [InlineData("0.3", "1.9", false)]
        [InlineData("15", "0.8", false)]
        [InlineData("1.5", "-1.5", false)]
        public async Task Build_CreatesNumberIsEqualToConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJsonAsync(
        string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
                {{
                    ""numberIsEqualToCondition"": {{
                        ""number"": {{
                            ""objectPathLookupNumber"": ""#""
                        }},
                        ""isEqualTo"": {decimal.Parse(comparand)}
                    }}
                }}";

            await this.VerifyConditionResolution(json, decimal.Parse(item), inclusionExpected);
        }

        [Fact]
        public async Task Build_CreatesNumberIsEqualToConditionThatThrowsAnError_WhenInputTypeDoesNotMatchAsExpected()
        {
            // Arrange
            var json = @"
                {
                    ""numberIsEqualToCondition"": {
                        ""number"": ""test"",
                        ""isEqualTo"": 1.5
                    }
                }";
            var dependencyProvider = new Mock<IServiceProvider>();
            var automationData = MockAutomationData.CreateWithEventTrigger();
            var sut = JsonConvert.DeserializeObject<IBuilder<IFilterProvider>>(json, AutomationDeserializationConfiguration.ModelSettings);
            var items = new List<object> { "test" };

            // Act
            var provider = sut!.Build(dependencyProvider.Object);
            var filter = await provider.Resolve<object>(new ProviderContext(automationData), new ExpressionScope("foo", Expression.Parameter(typeof(object))));
            var filteredItems = items.AsQueryable().Where(filter);

            // Assert
            Action executeQuery = () => filteredItems.ToList();
            executeQuery.Should().Throw<ErrorException>();
        }

        [Theory]
        [InlineData("1.5", "0.3", true)]
        [InlineData("0.9", "-1.5", true)]
        [InlineData("1.8", "-1.1", true)]
        [InlineData("0", "0", false)]
        [InlineData("1", "4", false)]
        [InlineData("-1", "-2.2", true)]
        [InlineData("0", "1.8", false)]
        [InlineData("-1", "1.9", false)]
        public async Task Build_CreatesNumberIsGreaterThanConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJsonAsync(
            string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""numberIsGreaterThanCondition"": {{
                    ""number"": {{
                        ""objectPathLookupNumber"": ""#""
                    }},
                    ""isGreaterThan"": {decimal.Parse(comparand)}
                }}
            }}";

            await this.VerifyConditionResolution(json, decimal.Parse(item), inclusionExpected);
        }

        [Theory]
        [InlineData("1.5", "0.3", true)]
        [InlineData("0.9", "-1.5", true)]
        [InlineData("1.8", "-1.1", true)]
        [InlineData("1.8", "1.8", true)]
        [InlineData("0", "0", true)]
        [InlineData("-8", "-8.0", true)]
        [InlineData("1", "4", false)]
        [InlineData("-1", "-2.2", true)]
        [InlineData("0", "1.8", false)]
        [InlineData("-1", "1.9", false)]
        public async Task Build_CreatesNumberIsGreaterThanOrEqualToConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJsonAsync(
            string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""numberIsGreaterThanOrEqualToCondition"": {{
                    ""number"": {{
                        ""objectPathLookupNumber"": ""#""
                    }},
                    ""isGreaterThanOrEqualTo"": {decimal.Parse(comparand)}
                }}
            }}";

            await this.VerifyConditionResolution(json, decimal.Parse(item), inclusionExpected);
        }

        [Theory]
        [InlineData("1.5", "3.0", true)]
        [InlineData("4.2", "10.40", true)]
        [InlineData("0.5", "0.56", true)]
        [InlineData("1", "0.1", false)]
        [InlineData("2.4", "2.3", false)]
        [InlineData("10.2", "10.3", true)]
        [InlineData("0.3", "0.4", true)]
        [InlineData("0.3", "0.3", false)]
        [InlineData("-0.4", "-0.2", true)]
        public async Task Build_CreatesNumberIsLessThanConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJsonAsync(
            string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""numberIsLessThanCondition"": {{
                    ""number"": {{
                        ""objectPathLookupNumber"": ""#""
                    }},
                    ""isLessThan"": {decimal.Parse(comparand)}
                }}
            }}";

            await this.VerifyConditionResolution(json, decimal.Parse(item), inclusionExpected);
        }

        [Theory]
        [InlineData("1.5", "3.0", true)]
        [InlineData("-4.2", "4.9", true)]
        [InlineData("0.5", "0.5", true)]
        [InlineData("1", "0.1", false)]
        [InlineData("2.4", "2.3", false)]
        [InlineData("10.2", "10.3", true)]
        [InlineData("0.3", "0.4", true)]
        [InlineData("0.3", "0.3", true)]
        [InlineData("-0.4", "-0.2", true)]
        public async Task Build_CreatesNumberIsLessThanOrEqualToConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJsonAsync(
            string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""numberIsLessThanOrEqualToCondition"": {{
                    ""number"": {{
                        ""objectPathLookupNumber"": ""#""
                    }},
                    ""isLessThanOrEqualTo"": {decimal.Parse(comparand)}
                }}
            }}";

            await this.VerifyConditionResolution(json, decimal.Parse(item), inclusionExpected);
        }
    }
}

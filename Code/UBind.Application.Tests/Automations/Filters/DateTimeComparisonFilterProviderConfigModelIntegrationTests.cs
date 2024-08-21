// <copyright file="DateTimeComparisonFilterProviderConfigModelIntegrationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Filters
{
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class DateTimeComparisonFilterProviderConfigModelIntegrationTests
        : BinaryExpressionFilterProviderConfigModelIntegrationTests<object>
    {
        [Theory]
        [InlineData("2023-01-02T00:00:01Z", "2023-01-02T00:00:01Z", true)]
        [InlineData("2000-01-02T12:30:01Z", "2000-01-02T12:30:01Z", true)]
        [InlineData("2023-01-02T00:00:01Z", "2023-01-02T23:00:01Z", false)]
        public async Task Build_CreatesDateTimeIsEqualToConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""dateTimeIsEqualToCondition"": {{
                    ""dateTime"": {{
                        ""objectPathLookupDateTime"": ""#""
                    }},
                    ""isEqualTo"": ""{comparand}""
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }
    }
}

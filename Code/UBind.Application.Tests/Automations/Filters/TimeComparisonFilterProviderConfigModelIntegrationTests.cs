// <copyright file="TimeComparisonFilterProviderConfigModelIntegrationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Filters
{
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class TimeComparisonFilterProviderConfigModelIntegrationTests
        : BinaryExpressionFilterProviderConfigModelIntegrationTests<object>
    {
        [Theory]
        [InlineData("1:50 PM", "1:50 PM", true)]
        [InlineData("13:56:00", "01:56 PM", true)]
        [InlineData("1:00 AM", "01:00:00", true)]
        [InlineData("1:00 AM", "01:00:59.1234567 AM", false)]
        [InlineData("00:00:00", "12:00 PM", false)]
        [InlineData("01:45:00", "01:45:45", false)]
        public async Task Build_CreatesTimeIsEqualToConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""timeIsEqualToCondition"": {{
                    ""time"": {{
                        ""objectPathLookupTime"": ""#""
                    }},
                    ""isEqualTo"": ""{comparand}""
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Theory]
        [InlineData("1:50 PM", "1:49 PM", true)]
        [InlineData("13:56:00", "01:55:59 PM", true)]
        [InlineData("1:00 AM", "00:59:00", true)]
        [InlineData("1:00 AM", "01:00:00.1234567 AM", false)]
        [InlineData("00:00:00", "12:00 AM", false)]
        [InlineData("01:45:45", "01:45:00", true)]
        public async Task Build_CreatesTimeIsAfterConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""timeIsAfterCondition"": {{
                    ""time"": {{
                        ""objectPathLookupTime"": ""#""
                    }},
                    ""isAfter"": ""{comparand}""
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Theory]
        [InlineData("1:50 PM", "1:50 PM", true)]
        [InlineData("13:56:00", "01:56:00 PM", true)]
        [InlineData("1:00 AM", "01:00:01", false)]
        [InlineData("1:00 AM", "12:59:00.1234567 AM", true)]
        [InlineData("00:00:00", "12:00 AM", true)]
        [InlineData("01:45:00", "01:45:59", false)]
        public async Task Build_CreatesTimeIsAfterOrEqualToConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""timeIsAfterOrEqualToCondition"": {{
                    ""time"": {{
                        ""objectPathLookupTime"": ""#""
                    }},
                    ""isAfterOrEqualTo"": ""{comparand}""
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Theory]
        [InlineData("1:50 PM", "1:50 PM", false)]
        [InlineData("13:56:00", "01:56:00 PM", false)]
        [InlineData("1:00 AM", "01:00:01", true)]
        [InlineData("1:00 AM", "01:00:00.1234567 AM", true)]
        [InlineData("15:00:00", "4:00 PM", true)]
        [InlineData("01:45:00", "01:45:59", true)]
        public async Task Build_CreatesTimeIsBeforeConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""timeIsBeforeCondition"": {{
                    ""time"": {{
                        ""objectPathLookupTime"": ""#""
                    }},
                    ""isBefore"": ""{comparand}""
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Theory]
        [InlineData("1:50 PM", "1:50 PM", true)]
        [InlineData("13:56:00", "01:56:00 PM", true)]
        [InlineData("1:50 PM", "1:49 PM", false)]
        [InlineData("13:56:00", "01:55:59 PM", false)]
        [InlineData("1:00 AM", "01:00:00.1234567 AM", true)]
        [InlineData("15:00:00", "4:00 PM", true)]
        [InlineData("01:45:00", "01:45:59", true)]
        public async Task Build_CreatesTimeIsBeforeOrEqualToConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""timeIsBeforeOrEqualToCondition"": {{
                    ""time"": {{
                        ""objectPathLookupTime"": ""#""
                    }},
                    ""isBeforeOrEqualTo"": ""{comparand}""
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }
    }
}

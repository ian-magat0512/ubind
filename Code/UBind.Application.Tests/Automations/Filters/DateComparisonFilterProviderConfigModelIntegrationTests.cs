// <copyright file="DateComparisonFilterProviderConfigModelIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Filters
{
    using Xunit;

    /// <summary>
    /// Integration tests for building date-related comparisons, e.g. dateIsEqualToCondition, dateIsAfterCondtiion, etc.
    /// </summary>
    public class DateComparisonFilterProviderConfigModelIntegrationTests
        : BinaryExpressionFilterProviderConfigModelIntegrationTests<object>
    {
        [Theory]
        [InlineData("2020-01-01", "2020-01-01", true)]
        [InlineData("2023-08-01", "2023-08-01", true)]
        [InlineData("2023-08-02", "2023-08-02T08:00:00Z", true)]
        [InlineData("2023-08-02", "2023-08-04", false)]
        [InlineData("2023-08-07", "2023-08-06", false)]
        public async Task BuildDateIsEqualToCondition_CreatesConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""dateIsEqualToCondition"": {{
                    ""date"": {{
                        ""objectPathLookupDate"": ""#""
                    }},
                    ""isEqualTo"": ""{comparand}""
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Theory]
        [InlineData("2020-01-01", "2019-12-31", true)]
        [InlineData("2023-08-01", "2023-07-31T12:59:59Z", true)]
        [InlineData("2023-08-02", "2023-08-01T08:00:00Z", true)]
        [InlineData("2023-08-02", "2023-08-02", false)]
        [InlineData("2023-08-07", "2023-08-08", false)]
        public async Task BuildDateIsAfterCondition_CreatesConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""dateIsAfterCondition"": {{
                    ""date"": {{
                        ""objectPathLookupDate"": ""#""
                    }},
                    ""isAfter"": ""{comparand}""
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Theory]
        [InlineData("2020-01-01", "2019-12-31", true)]
        [InlineData("2020-01-01", "2020-01-01", true)]
        [InlineData("2023-08-01", "2023-08-01T00:00:00Z", true)]
        [InlineData("2023-08-02", "2023-08-01T08:00:00Z", true)]
        [InlineData("2023-08-02", "2023-08-03", false)]
        [InlineData("2023-08-07", "2023-08-08", false)]
        public async Task BuildDateIsAfterOrEqualToCondition_CreatesConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""dateIsAfterOrEqualToCondition"": {{
                    ""date"": {{
                        ""objectPathLookupDate"": ""#""
                    }},
                    ""isAfterOrEqualTo"": ""{comparand}""
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Theory]
        [InlineData("2020-01-01", "2020-01-02", true)]
        [InlineData("2020-01-01", "2020-01-01", false)]
        [InlineData("2023-08-01", "2023-08-02T00:00:00Z", true)]
        [InlineData("2023-08-02", "2023-08-03T08:00:00Z", true)]
        [InlineData("2023-08-03", "2023-08-02", false)]
        [InlineData("2023-08-07", "2023-08-06", false)]
        public async Task BuildDateIsBeforeCondition_CreatesConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""dateIsBeforeCondition"": {{
                    ""date"": {{
                        ""objectPathLookupDate"": ""#""
                    }},
                    ""isBefore"": ""{comparand}""
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Theory]
        [InlineData("2020-01-01", "2020-01-02", true)]
        [InlineData("2020-01-01", "2020-01-01", true)]
        [InlineData("2023-08-01", "2023-08-02T00:00:00Z", true)]
        [InlineData("2023-08-02", "2023-08-02T08:00:00Z", true)]
        [InlineData("2023-08-03", "2023-08-02", false)]
        [InlineData("2023-08-07", "2023-08-06", false)]
        public async Task BuildDateIsBeforeOrEqualToCondition_CreatesConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(string item, string comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""dateIsBeforeOrEqualToCondition"": {{
                    ""date"": {{
                        ""objectPathLookupDate"": ""#""
                    }},
                    ""isBeforeOrEqualTo"": ""{comparand}""
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }
    }
}

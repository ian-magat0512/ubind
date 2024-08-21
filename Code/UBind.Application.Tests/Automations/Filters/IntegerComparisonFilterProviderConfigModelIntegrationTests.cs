// <copyright file="IntegerComparisonFilterProviderConfigModelIntegrationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Filters
{
    using Xunit;

    /// <summary>
    /// Integration tests for building integer-related comparisons, e.g. integerIsEqualToCondition, integerIsAfterCondtiion, etc.
    /// </summary>
    [SystemEventTypeExtensionInitialize]
    public class IntegerComparisonFilterProviderConfigModelIntegrationTests
        : BinaryExpressionFilterProviderConfigModelIntegrationTests<long>
    {
        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(1, 1, true)]
        [InlineData(long.MaxValue, long.MaxValue, true)]
        [InlineData(-1, -1, true)]
        [InlineData(long.MinValue, long.MinValue, true)]
        [InlineData(0, 1, false)]
        [InlineData(1, 0, false)]
        [InlineData(long.MaxValue, long.MinValue, false)]
        [InlineData(1, -1, false)]
        [InlineData(long.MinValue, long.MinValue + 1, false)]
        public async Task Build_CreatesIntegerIsEqualToConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJsonAsync(
    long item, long comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""integerIsEqualToCondition"": {{
                    ""integer"": {{
                        ""objectPathLookupInteger"": ""#""
                    }},
                    ""isEqualTo"": {comparand}
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Theory]
        [InlineData(1, 0, true)]
        [InlineData(0, -1, true)]
        [InlineData(1, -1, true)]
        [InlineData(long.MaxValue, long.MinValue, true)]
        [InlineData(long.MaxValue, long.MaxValue - 1, true)]
        [InlineData(long.MinValue + 1, long.MinValue, true)]
        [InlineData(0, 0, false)]
        [InlineData(1, 1, false)]
        [InlineData(long.MinValue, long.MinValue, false)]
        [InlineData(-1, -1, false)]
        [InlineData(long.MaxValue, long.MaxValue, false)]
        [InlineData(0, 1, false)]
        [InlineData(long.MinValue, long.MaxValue, false)]
        [InlineData(-1, 1, false)]
        public async Task Build_CreatesIntegerIsGreaterThanConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJsonAsync(
    long item, long comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""integerIsGreaterThanCondition"": {{
                    ""integer"": {{
                        ""objectPathLookupInteger"": ""#""
                    }},
                    ""isGreaterThan"": {comparand}
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Theory]
        [InlineData(1, 0, true)]
        [InlineData(0, -1, true)]
        [InlineData(1, -1, true)]
        [InlineData(long.MaxValue, long.MinValue, true)]
        [InlineData(long.MaxValue, long.MaxValue - 1, true)]
        [InlineData(long.MinValue + 1, long.MinValue, true)]
        [InlineData(0, 0, true)]
        [InlineData(1, 1, true)]
        [InlineData(long.MinValue, long.MinValue, true)]
        [InlineData(-1, -1, true)]
        [InlineData(long.MaxValue, long.MaxValue, true)]
        [InlineData(0, 1, false)]
        [InlineData(long.MinValue, long.MaxValue, false)]
        [InlineData(-1, 1, false)]
        public async Task Build_CreatesIntegerIsGreaterThanOrEqualToConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJsonAsync(
    long item, long comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""integerIsGreaterThanOrEqualToCondition"": {{
                    ""integer"": {{
                        ""objectPathLookupInteger"": ""#""
                    }},
                    ""isGreaterThanOrEqualTo"": {comparand}
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Theory]
        [InlineData(0, 1, true)]
        [InlineData(-1, 0, true)]
        [InlineData(-1, 1, true)]
        [InlineData(long.MinValue, long.MaxValue, true)]
        [InlineData(long.MaxValue - 1, long.MaxValue, true)]
        [InlineData(long.MinValue, long.MinValue + 1, true)]
        [InlineData(0, 0, false)]
        [InlineData(1, 1, false)]
        [InlineData(long.MaxValue, long.MaxValue, false)]
        [InlineData(-1, -1, false)]
        [InlineData(long.MinValue, long.MinValue, false)]
        [InlineData(1, 0, false)]
        [InlineData(long.MaxValue, long.MinValue, false)]
        [InlineData(1, -1, false)]
        public async Task Build_CreatesIntegerIsLessThanConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJsonAsync(
    long item, long comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""integerIsLessThanCondition"": {{
                    ""integer"": {{
                        ""objectPathLookupInteger"": ""#""
                    }},
                    ""isLessThan"": {comparand}
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }

        [Theory]
        [InlineData(0, 1, true)]
        [InlineData(-1, 0, true)]
        [InlineData(-1, 1, true)]
        [InlineData(long.MinValue, long.MaxValue, true)]
        [InlineData(long.MaxValue - 1, long.MaxValue, true)]
        [InlineData(long.MinValue, long.MinValue + 1, true)]
        [InlineData(0, 0, true)]
        [InlineData(1, 1, true)]
        [InlineData(long.MinValue, long.MinValue, true)]
        [InlineData(-1, -1, true)]
        [InlineData(long.MaxValue, long.MaxValue, true)]
        [InlineData(1, 0, false)]
        [InlineData(long.MaxValue, long.MinValue, false)]
        [InlineData(1, -1, false)]
        public async Task Build_CreatesIntegerIsLessThanOrEqualToConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJsonAsync(
    long item, long comparand, bool inclusionExpected)
        {
            var json = $@"
            {{
                ""integerIsLessThanOrEqualToCondition"": {{
                    ""integer"": {{
                        ""objectPathLookupInteger"": ""#""
                    }},
                    ""isLessThanOrEqualTo"": {comparand}
                }}
            }}";

            await this.VerifyConditionResolution(json, item, inclusionExpected);
        }
    }
}

// <copyright file="IntegerIsGreaterThanConditionConfigModelIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Conditions
{
    using System.Threading.Tasks;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class IntegerIsGreaterThanConditionConfigModelIntegrationTests : ConditionConfigModelIntegrationTests
    {
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
        public async Task Build_CreatesConditionThatResolvesCorrectResult_WhenSutIsDeserializedFromJsonAsync(
            long first, long second, bool expectedResult)
        {
            var json = $@"
{{
    ""integerIsGreaterThanCondition"": {{
        ""integer"": {first},
        ""isGreaterThan"": {second}
    }}
}}";

            await this.VerifyConditionResolutionAsync(json, expectedResult);
        }
    }
}

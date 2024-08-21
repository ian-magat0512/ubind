// <copyright file="DateTimeIsInPeriodConditionConfigModelIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Conditions
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class DateTimeIsInPeriodConditionConfigModelIntegrationTests
    {
        [Theory]
        [InlineData("1999-12-31T23:59:59Z", false)]
        [InlineData("2000-01-01T00:00:00Z", true)]
        [InlineData("2000-01-01T12:00:00Z", true)]
        [InlineData("2000-01-02T00:00:00Z", false)]
        [InlineData("2000-01-02T00:00:01Z", false)]
        public async Task Build_CreatesProviderThatResolvesCorrectResult_WhenSutIsDeserializedFromJson(
            string dateTime, bool expectedResult)
        {
            // Arrange
            var fakeClock = new TestClock();
            var fakeNow = Instant.FromUtc(2000, 1, 2, 0, 0);
            fakeClock.SetToInstant(fakeNow);
            var dependencyProvider = new Mock<IServiceProvider>();
            dependencyProvider
                .Setup(dp => dp.GetService(typeof(IClock))) // Underlying method called by GetRequiredService(type).
                .Returns(fakeClock);
            var automationData = MockAutomationData.CreateWithEventTrigger();
            var json = $@"
{{
    ""dateTimeIsInPeriodCondition"": {{
        ""dateTime"": ""{dateTime}"",
        ""isInPeriod"": {{
            ""lastPeriod"": {{
                ""periodTypeValueDuration"": {{
                    ""value"": 1,
                    ""periodType"": ""day""
                }}               
            }}
        }}
    }}
}}";

            // Act
            var sut = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(
                json, AutomationDeserializationConfiguration.ModelSettings);

            var provider = sut.Build(dependencyProvider.Object);
            bool result = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}

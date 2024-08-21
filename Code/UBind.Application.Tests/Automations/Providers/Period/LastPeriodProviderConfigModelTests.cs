// <copyright file="LastPeriodProviderConfigModelTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Period
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
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class LastPeriodProviderConfigModelTests
    {
        [Fact]
        public async Task Resolve_ReturnsExpectedIntervall()
        {
            // Arrange
            var json = @"
{
    ""lastPeriod"": {
        ""periodTypeValueDuration"": {
            ""value"": 60,
            ""periodType"": ""second""
        }
    }
}";

            var sut = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<Interval>>>>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var fakeClock = new TestClock();
            var fakeNow = Instant.FromUtc(2000, 1, 1, 0, 0);
            fakeClock.SetToInstant(fakeNow);
            var dependencyProvider = new Mock<IServiceProvider>();
            dependencyProvider
                .Setup(dp => dp.GetService(typeof(IClock))) // Underlying method called by GetRequiredService(type).
                .Returns(fakeClock);
            var periodProvider = sut.Build(dependencyProvider.Object);
            var fakeAutomationData = MockAutomationData.CreateWithEventTrigger();
            var expectedStart = Instant.FromUtc(1999, 12, 31, 23, 59);

            // Act
            Interval period = (await periodProvider.Resolve(new ProviderContext(fakeAutomationData))).GetValueOrThrowIfFailed();

            // Assert
            period.End.Should().Be(fakeNow);
            period.Start.Should().Be(expectedStart);
        }
    }
}

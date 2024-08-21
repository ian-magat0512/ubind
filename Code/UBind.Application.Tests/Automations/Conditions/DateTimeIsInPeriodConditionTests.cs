// <copyright file="DateTimeIsInPeriodConditionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Conditions
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Conditions;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class DateTimeIsInPeriodConditionTests
    {
        [Fact]
        public async Task Resolve_ReturnsTrue_WhenDateTimeIsInPeriod()
        {
            // Arrange
            var interval = new Interval(
                Instant.FromUtc(2000, 1, 1, 0, 0),
                Instant.FromUtc(2000, 1, 2, 0, 0));
            var dateTime = Instant.FromUtc(2000, 1, 1, 12, 0);
            var dateTimeProvider = new StaticProvider<Data<Instant>>(dateTime);
            var intervalProvider = new StaticProvider<Data<Interval>>(interval);
            var sut = new DateTimeIsInPeriodCondition(dateTimeProvider, intervalProvider);
            var automationData = MockAutomationData.CreateWithEventTrigger();

            // Act
            bool result = (await sut.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            result.Should().Be(true);
        }

        // TODO: Confirm desired boundary behaviour (currently following analagous NodaTime behaviour).
        [Fact]
        public async Task Resolve_ReturnsWhat_WhenDateTimeIsOnStartOfPeriod()
        {
            // Arrange
            var start = Instant.FromUtc(2000, 1, 1, 0, 0);
            var end = Instant.FromUtc(2000, 1, 2, 0, 0);
            var interval = new Interval(start, end);
            var dateTimeProvider = new StaticProvider<Data<Instant>>(start);
            var intervalProvider = new StaticProvider<Data<Interval>>(interval);
            var sut = new DateTimeIsInPeriodCondition(dateTimeProvider, intervalProvider);
            var automationData = MockAutomationData.CreateWithEventTrigger();

            // Act
            bool result = (await sut.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            result.Should().Be(true);
        }

        // TODO: Confirm desired boundary behaviour (currently following analagous NodaTime behaviour).
        [Fact]
        public async Task Resolve_ReturnsFalse_WhenDateTimeIsOnEndOfPeriod()
        {
            // Arrange
            var start = Instant.FromUtc(2000, 1, 1, 0, 0);
            var end = Instant.FromUtc(2000, 1, 2, 0, 0);
            var interval = new Interval(start, end);
            var dateTimeProvider = new StaticProvider<Data<Instant>>(end);
            var intervalProvider = new StaticProvider<Data<Interval>>(interval);
            var sut = new DateTimeIsInPeriodCondition(dateTimeProvider, intervalProvider);
            var automationData = MockAutomationData.CreateWithEventTrigger();

            // Act
            bool result = (await sut.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            result.Should().Be(false);
        }
    }
}

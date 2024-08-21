// <copyright file="PeriodTypeValueDurationProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Duration
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Duration;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class PeriodTypeValueDurationProviderTests
    {
        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfPeriodTypeValueDurationYears()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = new PeriodTypeValueDurationProvider(new StaticProvider<Data<long>>(2), Application.Automation.Enums.PeriodType.Year);

            // Act
            NodaTime.Period duration = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            duration.Years.Should().Be(2);
        }

        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfPeriodTypeValueDurationQuaters()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = new PeriodTypeValueDurationProvider(new StaticProvider<Data<long>>(2), Application.Automation.Enums.PeriodType.Quarter);

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Months.Should().Be(6);
        }

        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfPeriodTypeValueDurationMonths()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = new PeriodTypeValueDurationProvider(new StaticProvider<Data<long>>(2), Application.Automation.Enums.PeriodType.Month);

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Months.Should().Be(2);
        }

        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfPeriodTypeValueDurationWeeks()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = new PeriodTypeValueDurationProvider(new StaticProvider<Data<long>>(2), Application.Automation.Enums.PeriodType.Week);

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Weeks.Should().Be(2);
        }

        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfPeriodTypeValueDurationDays()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = new PeriodTypeValueDurationProvider(new StaticProvider<Data<long>>(2), Application.Automation.Enums.PeriodType.Day);

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Days.Should().Be(2);
        }

        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfPeriodTypeValueDurationHours()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = new PeriodTypeValueDurationProvider(new StaticProvider<Data<long>>(2), Application.Automation.Enums.PeriodType.Hour);

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Hours.Should().Be(2);
        }

        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfPeriodTypeValueDurationMinutes()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = new PeriodTypeValueDurationProvider(new StaticProvider<Data<long>>(2), Application.Automation.Enums.PeriodType.Minute);

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Minutes.Should().Be(2);
        }

        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfPeriodTypeValueDurationSeconds()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = new PeriodTypeValueDurationProvider(new StaticProvider<Data<long>>(2), Application.Automation.Enums.PeriodType.Second);

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Seconds.Should().Be(2);
        }

        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfPeriodTypeValueDurationMilliseconds()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = new PeriodTypeValueDurationProvider(new StaticProvider<Data<long>>(2), Application.Automation.Enums.PeriodType.Millisecond);

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Milliseconds.Should().Be(2);
        }
    }
}

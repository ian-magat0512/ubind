// <copyright file="DurationProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Duration
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Duration;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class DurationProviderTests
    {
        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfTextDurationYear()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = this.CreateDurationProvider("P1Y");

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Years.Should().Be(1);
        }

        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfTextDurationMonth()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = this.CreateDurationProvider("P1M");

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Months.Should().Be(1);
        }

        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfTextDurationCombination()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = this.CreateDurationProvider("P3Y6M1W4DT12H30M5S");

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Years.Should().Be(3);
            period.Months.Should().Be(6);
            period.Weeks.Should().Be(1);
            period.Days.Should().Be(4);
            period.Hours.Should().Be(12);
            period.Minutes.Should().Be(30);
            period.Seconds.Should().Be(5);
        }

        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfTextDurationHourMinuteCombination()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = this.CreateDurationProvider("PT1H30M");

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Hours.Should().Be(1);
            period.Minutes.Should().Be(30);
        }

        [Fact]
        public async Task ResolveContent_HasCorrectConversion_IfTextDurationDay()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = this.CreateDurationProvider("P1D");

            // Act
            NodaTime.Period period = (await durationProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            period.Days.Should().Be(1);
        }

        [Fact]
        public void ResolveContent_ThrowsAnErrorException_IfTextIsNull()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = this.CreateDurationProvider(null);

            // Act + Assert
            Func<Task> action = async () => await durationProvider.Resolve(new ProviderContext(data));
            action.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task ResolveContent_ThrowsAnErrorException_IfTextDurationHasEmptyStringDurationAsync()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = this.CreateDurationProvider(string.Empty);

            // Act
            Func<Task> func = async () => await durationProvider.Resolve(new ProviderContext(data));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>())
                .Which.Error.Data["errorMessage"].ToString().Should().Be("The value string is empty.");
        }

        [Fact]
        public async Task ResolveContent_ThrowsAnErrorException_IfTextDurationHasWrongFormatDurationAsync()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = this.CreateDurationProvider("Pz");

            // Act
            Func<Task> func = async () => await durationProvider.Resolve(new ProviderContext(data));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Data["errorMessage"].ToString().Should().Be("The value string does not include a number in the expected position. Value being parsed: 'P^z'. (^ indicates error position.)");
        }

        [Fact]
        public async Task ResolveContent_ThrowsAnErrorException_IfTextDurationHasP1Y1SValueAsync()
        {
            // Arrange
            var data = MockAutomationData.CreateWithEventTrigger();
            IProvider<Data<NodaTime.Period>> durationProvider = this.CreateDurationProvider("P1Y1S");

            // Act
            Func<Task> func = async () => await durationProvider.Resolve(new ProviderContext(data));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>())
                .Which.Error.Data["errorMessage"].ToString().ToString().Should().Be("The period unit specifier 'S' appears at the wrong place in the input string. Value being parsed: 'P1Y1^S'. (^ indicates error position.)");
        }

        private IProvider<Data<NodaTime.Period>> CreateDurationProvider(string isoDuration)
        {
            IProvider<Data<string>> textProvider = new StaticProvider<Data<string>>(isoDuration);
            return new StaticDurationProvider(textProvider);
        }
    }
}

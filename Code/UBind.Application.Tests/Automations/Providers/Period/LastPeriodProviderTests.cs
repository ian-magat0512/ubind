// <copyright file="LastPeriodProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Period
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Period;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class LastPeriodProviderTests
    {
        [Fact]
        public async Task Resolve_ReturnsCorrectInterval_WhenDurationIsExpressedInDays()
        {
            // Arrange
            var fakeNow = Instant.FromUtc(2021, 1, 2, 0, 0);
            var fakeOneDayAgo = Instant.FromUtc(2021, 1, 1, 0, 0);
            var clock = new TestClock();
            clock.SetToInstant(fakeNow);
            var period = Period.FromDays(1);
            var durationProvider = new StaticProvider<Data<Period>>(period);
            var sut = new LastPeriodProvider(durationProvider, clock);
            AutomationData automationData = null;

            // Act
            Interval result = (await sut.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            result.Start.Should().Be(fakeOneDayAgo);
            result.End.Should().Be(fakeNow);
        }
    }
}

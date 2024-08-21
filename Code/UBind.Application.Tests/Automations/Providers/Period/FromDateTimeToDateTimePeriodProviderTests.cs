// <copyright file="FromDateTimeToDateTimePeriodProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Period
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Period;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using Xunit;

    public class FromDateTimeToDateTimePeriodProviderTests
    {
        [Fact]
        public async Task Resolve_ReturnsCorrectInterval_WhenFromAndToInstantsFormValidInterval()
        {
            // Arrange
            var from = Instant.FromUtc(2021, 1, 14, 0, 0);
            var to = Instant.FromUtc(2022, 1, 14, 0, 0);
            var fromProvider = new StaticProvider<Data<Instant>>(from);
            var toProvider = new StaticProvider<Data<Instant>>(to);
            var sut = new FromDateTimeToDateTimePeriodProvider(fromProvider, toProvider);
            AutomationData automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Interval result = (await sut.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            var timezone = automationData.System.TimeZone;
            var fromAdjustedToAet = from.InUtc().LocalDateTime.InZoneLeniently(timezone).ToInstant();
            var toAdjustedToAet = to.InUtc().LocalDateTime.InZoneLeniently(timezone).ToInstant();
            result.Start.Should().Be(fromAdjustedToAet);
            result.End.Should().Be(toAdjustedToAet);
        }

        [Fact]
        public async Task Resolve_ThrowsErrorException_WhenFromisAfterTo()
        {
            // Arrange
            var from = Instant.FromUtc(2022, 1, 14, 0, 0);
            var to = Instant.FromUtc(2021, 1, 14, 0, 0);
            var fromProvider = new StaticProvider<Data<Instant>>(from);
            var toProvider = new StaticProvider<Data<Instant>>(to);
            var sut = new FromDateTimeToDateTimePeriodProvider(fromProvider, toProvider);
            AutomationData automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);
            var errorData = await providerContext.GetDebugContextForProviders(sut.SchemaReferenceKey);
            var expectedError = Errors.Automation.ParameterValueTypeInvalid(
                "fromDateTimeToDateTimePeriod",
                "toDateTime",
                to.ToString(),
                errorData);

            // Act
            Func<Task> func = async () => await sut.Resolve(providerContext);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Title.Should().Be(expectedError.Title);
        }
    }
}

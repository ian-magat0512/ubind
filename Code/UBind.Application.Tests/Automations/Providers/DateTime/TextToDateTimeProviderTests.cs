// <copyright file="TextToDateTimeProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.DateTime
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.DateTime;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class TextToDateTimeProviderTests
    {
        [Fact]
        public async Task Resolve_ThrowsErrorException_WhenTextProviderIsMissing()
        {
            // Arrange
            var sut = new TextToDateTimeProvider(null);
            AutomationData automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();

            // Act
            Func<Task> func = async () => await sut.Resolve(new ProviderContext(automationData));

            // Assert
            await func.Should().ThrowAsync<Exception>();
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("12pm")]
        [InlineData("12:23")]
        [InlineData("2000-01-01")]
        public async Task Resolve_ThrowsErrorException_WhenTextProviderGivesInvalidString(string providedText)
        {
            // Arrange
            var textProvider = new StaticProvider<Data<string>>(providedText);
            var sut = new TextToDateTimeProvider(textProvider);
            AutomationData automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();

            // Act
            Func<Task> func = async () => await sut.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Title.Should().Be("We couldn't recognise that as a valid dateTime");
        }

        [Theory]
        [InlineData("2000-01-01T00:00:00Z", 2000, 1, 1, 0, 0, 0)]
        [InlineData("2021-01-14T21:41:45Z", 2021, 1, 14, 21, 41, 45)]
        public async Task Resolve_ReturnsCorrectInstant_WhenTextProviderGivesValidString(
            string providedText, int year, int month, int day, int hour, int minute, int second)
        {
            // Arrange
            var textProvider = new StaticProvider<Data<string>>(providedText);
            var sut = new TextToDateTimeProvider(textProvider);
            AutomationData automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();

            // Act
            Instant result = (await sut.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            result.Should().Be(Instant.FromDateTimeUtc(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc)));
        }
    }
}

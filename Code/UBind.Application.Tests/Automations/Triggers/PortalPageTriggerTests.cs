// <copyright file="PortalPageTriggerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Triggers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class PortalPageTriggerTests
    {
        [Theory]
        [InlineData(EntityType.Quote, PageType.List, true)]
        [InlineData(EntityType.Quote, PageType.Display, false)]
        public void DoesMatch_ReturnsExpectedResult_WhenAutomationDataIncludesPortalPageTrigger(
            EntityType entityType, PageType pageType, bool expectedResult)
        {
            // Arrange
            var automationData = MockAutomationData.CreateWithPortalPageTrigger();
            var sut = this.CreateTrigger(entityType, pageType);

            // Act
            var doesMatch = sut.DoesMatch(automationData);

            // Assert
            doesMatch.Equals(expectedResult);
        }

        [Fact]
        public async Task DoesMatch_ReturnsFalse_WhenAutomationDataDoesNotIncludePortalPageTrigger()
        {
            // Arrange
            var automationData = MockAutomationData.CreateWithEventTrigger();
            var sut = this.CreateTrigger(EntityType.Quote, PageType.List);

            // Act
            var doesMatch = await sut.DoesMatch(automationData);

            // Assert
            doesMatch.Should().BeFalse();
        }

        private PortalPageTrigger CreateTrigger(EntityType entityType, PageType pageType)
        {
            var pages = new List<Page>
            {
                new Page(entityType, pageType),
            };

            return new PortalPageTrigger(
                "Test Portal Page Trigger",
                "testPortalPageTrigger",
                "This is a test portal page trigger",
                pages,
                "download",
                null,
                "Download page data",
                string.Empty,
                false,
                "Preparing page data for download...",
                new StaticProvider<Data<string>>("Page data made available for download"),
                new TextFileProvider(
                    new StaticProvider<Data<string>>("quote_sample.txt"),
                    new StaticProvider<Data<string>>("Hello world!")),
                true);
        }
    }
}

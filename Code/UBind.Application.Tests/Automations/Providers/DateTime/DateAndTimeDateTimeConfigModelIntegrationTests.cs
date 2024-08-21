// <copyright file="DateAndTimeDateTimeConfigModelIntegrationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Providers.DateTime
{
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.DateTime;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Extensions;
    using Xunit;

    public class DateAndTimeDateTimeConfigModelIntegrationTests
    {
        [Fact]
        [SystemEventTypeExtensionInitialize]
        public async void Build_ShouldBeAbleToBuildProvider_FromJsonAndResolveToDateTime()
        {
            // Arrange
            var json = $@"
                {{
                    ""dateAndTimeDateTime"": {{
                        ""date"": ""2023-08-01"",
                        ""time"": ""8:30 AM""
                    }}
                }}";
            var expectedDateTime = new LocalDateTime(2023, 8, 1, 8, 30).ToExtendedIso8601() + "Z";
            var expected = InstantPattern.ExtendedIso.Parse(expectedDateTime).Value;
            var dependencyProvider = new Mock<IServiceProvider>();
            var automationData = MockAutomationData.CreateWithEventTrigger();

            // Act
            var builder = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<Instant>>>>(json, AutomationDeserializationConfiguration.ModelSettings);
            var provider = builder!.Build(dependencyProvider.Object);
            var result = await provider.Resolve(new ProviderContext(automationData));

            // Assert
            builder.Should().NotBeNull();
            builder.Should().BeOfType<DateAndTimeDateTimeProviderConfigModel>();
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(expected);
        }
    }
}

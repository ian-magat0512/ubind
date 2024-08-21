// <copyright file="DateTimeToTextProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.DateTime
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
    using UBind.Application.Automation.Providers.Text;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using Xunit;

    public class DateTimeToTextProviderTests
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;
        private IClock clock;

        public DateTimeToTextProviderTests()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
            this.clock = SystemClock.Instance;
        }

        [Fact]
        public async Task DateTimeToText_ShouldParseDateTimeFromTenantEntityCreatedDate_WhenTextIsValidISO8601Format()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            var json = @"{
                ""dateTimeToText"": {
                    ""objectPathLookupDateTime"": {
                        ""path"": ""/createdDateTime"",
                        ""dataObject"": {
                                ""entityObject"": {
                                    ""entity"": {
                                        ""contextEntity"": ""/tenant""
                                         },
                                        ""includeOptionalProperties"": []
                                 }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<string>>>>(json, this.settings);
            var dateTimeToText = model.Build(this.dependencyProvider) as DateTimeToTextProvider;

            // Assert
            var result = await dateTimeToText.Resolve(new ProviderContext(automationData));
            var createdTimestamp = Instant.FromUnixTimeTicks(automationData.ContextManager.Tenant.CreatedTicksSinceEpoch);
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(createdTimestamp.ToString());
        }

        [Fact]
        public async Task DateTimeToText_ShouldThrowError_WhenDateTimeIsNotValidISOFormat()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            var json = @"{
                ""dateTimeToText"": {
                    ""objectPathLookupDateTime"": {
                        ""path"": ""/createdLocalDate"",
                        ""dataObject"": {
                                ""entityObject"": {
                                    ""entity"": {
                                        ""contextEntity"": ""/tenant""
                                         },
                                        ""includeOptionalProperties"": []
                                 }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<string>>>>(json, this.settings);
            var dateTimeToText = model.Build(this.dependencyProvider) as DateTimeToTextProvider;

            // Assert
            Func<Task> act = async () => await dateTimeToText.Resolve(new ProviderContext(automationData));
            await act.Should().ThrowAsync<ErrorException>();
        }
    }
}

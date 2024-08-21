// <copyright file="DateToTextProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Date
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

    public class DateToTextProviderTests
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;
        private IClock clock;

        public DateToTextProviderTests()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
            this.clock = SystemClock.Instance;
        }

        [Fact]
        public async Task DateToText_ShouldParseDateFromTenantEntityCreatedDate_WhenTheDateIsValidISOFormat()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            var json = @"{
                ""dateToText"": {
                    ""objectPathLookupDate"": {
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
            var dateToText = model.Build(this.dependencyProvider) as DateToTextProvider;

            // Assert
            var result = await dateToText.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(automationData.ContextManager.Tenant.CreatedDate);
        }

        [Fact]
        public async Task DateToText_ShouldThrowError_WhenDateIsNotValidISOFormat()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            var json = @"{
                ""dateToText"": {
                    ""objectPathLookupDate"": {
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
            var dateToText = model.Build(this.dependencyProvider) as DateToTextProvider;

            // Assert
            Func<Task> act = async () => await dateToText.Resolve(new ProviderContext(automationData));
            await act.Should().ThrowAsync<ErrorException>();
        }
    }
}

// <copyright file="TimeToTextProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Text
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Text;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using Xunit;

    public class TimeToTextProviderTests
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public TimeToTextProviderTests()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task TimeToText_ShouldParseTimeFromTenantEntityCreatedTime_WhenTheTimeIsValidISOFormat()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            var json = @"{
                ""timeToText"": {
                    ""objectPathLookupTime"": {
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
            var timeToText = model.Build(this.dependencyProvider) as TimeToTextProvider;

            // Assert
            var result = await timeToText.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(automationData.ContextManager.Tenant.CreatedTime);
        }

        [Fact]
        public async Task TimeToText_ShouldThrowError_WhenTimeIsNotValidISOFormat()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            var json = @"{
                ""timeToText"": {
                    ""objectPathLookupTime"": {
                        ""path"": ""/createdLocalTime"",
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
            var timeToText = model.Build(this.dependencyProvider) as TimeToTextProvider;

            // Assert
            Func<Task> act = async () => await timeToText.Resolve(new ProviderContext(automationData));
            await act.Should().ThrowAsync<ErrorException>();
        }
    }
}

// <copyright file="IntegerToTextProviderTests.cs" company="uBind">
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
    using Xunit;

    public class IntegerToTextProviderTests
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public IntegerToTextProviderTests()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task IntegerToText_ShouldParseTheIntegerValue_WhenIntegerValueIsValid()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            var json = @"{
                ""integerToText"": {
                    ""objectPathLookupInteger"": {
                        ""path"": ""/createdTicksSinceEpoch"",
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
            var integerToText = model.Build(this.dependencyProvider) as IntegerToTextProvider;

            // Assert
            var result = await integerToText.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(automationData.ContextManager.Tenant.CreatedTicksSinceEpoch.ToString());
        }

        [Fact]
        public async Task IntegerToText_ShouldThrowError_WhenIntegerValueIsInvalid()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            var json = @"{
                ""integerToText"": {
                    ""objectPathLookupInteger"": {
                        ""path"": ""/alias"",
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
            var integerToText = model.Build(this.dependencyProvider) as IntegerToTextProvider;

            // Assert
            Func<Task> act = async () => await integerToText.Resolve(new ProviderContext(automationData));
            await act.Should().ThrowAsync<ErrorException>();
        }
    }
}

// <copyright file="TextToIntegerProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Integer
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Integer;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class TextToIntegerProviderTests
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public TextToIntegerProviderTests()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task TextToInteger_ShouldBeParseAsInteger_WhenValidIntegerValueAsString()
        {
            // Arrange
            var pathLookUp = @"{""integer"": ""15""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToInteger"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/integer"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<long>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToIntegerProvider;

            // Assert
            var result = await condition.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(15);
        }

        [Fact]
        public async Task TextToInteger_ShouldThrowErrorException_WhenIntegerValueIsInvalid()
        {
            // Arrange
            var pathLookUp = @"{""integer"": ""15.12""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToInteger"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/integer"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<long>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToIntegerProvider;

            // Assert
            Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Title.Should().Be("An exception was raised when trying to resolve the value from a provider");
        }
    }
}

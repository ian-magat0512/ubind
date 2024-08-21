// <copyright file="TextToNumberProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Number
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Number;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class TextToNumberProviderTests
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public TextToNumberProviderTests()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task TextToNumber_ShouldBeParseAsNumber_WhenValidNumberValueAsString()
        {
            // Arrange
            var pathLookUp = @"{""number"": ""15.15""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToNumber"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/number"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<decimal>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToNumberProvider;

            // Assert
            var result = await condition.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(15.15M);
        }

        [Fact]
        public async Task TextToNumber_ShouldThrowError_WhenInvalidNumberValue()
        {
            // Arrange
            var pathLookUp = @"{""number"": ""notAnumber""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToNumber"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/number"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<decimal>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToNumberProvider;

            // Assert
            Func<Task> act = async () => await condition.Resolve(new ProviderContext(automationData));
            await act.Should().ThrowAsync<ErrorException>();
        }
    }
}

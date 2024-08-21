// <copyright file="NumberToTextProviderTests.cs" company="uBind">
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

    public class NumberToTextProviderTests
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public NumberToTextProviderTests()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task NumberToText_ShouldParseTheNumberValueToString_WhenTheNumberValueIsValid()
        {
            // Arrange
            var pathLookUp = @"{""number"": ""15.15""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""numberToText"": {
                    ""objectPathLookupNumber"": {
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
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<string>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as NumberToTextProvider;

            // Assert
            var result = await condition.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be("15.15");
        }

        [Fact]
        public async Task NumberToText_ShouldThrowError_WhenNumberValueIsInvalid()
        {
            // Arrange
            var pathLookUp = @"{""number"": ""notANumber""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""numberToText"": {
                    ""objectPathLookupNumber"": {
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
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<string>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as NumberToTextProvider;

            // Assert
            Func<Task> act = async () => await condition.Resolve(new ProviderContext(automationData));
            await act.Should().ThrowAsync<ErrorException>();
        }
    }
}

// <copyright file="TextToTimeProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Time
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
    using UBind.Application.Automation.Providers.Time;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class TextToTimeProviderTests
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public TextToTimeProviderTests()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task TextToTime_ShouldParseTime_WhenTimeISValidISO()
        {
            // Arrange
            var pathLookUp = @"{""time"": ""12:12:12""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToTime"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/time"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalTime>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToTimeProvider;

            // Assert
            var result = await condition.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(new LocalTime(12, 12, 12));
        }

        [Fact]
        public async Task TextToTime_ShouldParseTime_WhenTimeIsValidFormathmmtt()
        {
            // Arrange
            var pathLookUp = @"{""time"": ""5:10 AM""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToTime"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/time"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalTime>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToTimeProvider;

            // Assert
            var result = await condition.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(new LocalTime(5, 10));
        }

        [Fact]
        public async Task TextToTime_ShouldParseTime_WhenTimeIsValidFormathmmsstt()
        {
            // Arrange
            var pathLookUp = @"{""time"": ""5:10:10 AM""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToTime"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/time"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalTime>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToTimeProvider;

            // Assert
            var result = await condition.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(new LocalTime(5, 10, 10));
        }

        [Fact]
        public async Task TextToTime_ShouldParseTime_WhenTimeIsValidFormathhmmtt()
        {
            // Arrange
            var pathLookUp = @"{""time"": ""01:10 AM""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToTime"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/time"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalTime>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToTimeProvider;

            // Assert
            var result = await condition.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(new LocalTime(01, 10));
        }

        [Fact]
        public async Task TextToTime_ShouldReturnTrue_FromValidJsonWithFormathhmmsstt()
        {
            // Arrange
            var pathLookUp = @"{""time"": ""02:10:10 AM""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToTime"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/time"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalTime>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToTimeProvider;

            // Assert
            var result = await condition.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(new LocalTime(02, 10, 10));
        }

        [Fact]
        public async Task TextToTime_ShouldReturnError_FromValidJsonWithInvalidValue()
        {
            // Arrange
            var pathLookUp = @"{""time"": ""24:10:101 AM""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToTime"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/time"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalTime>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToTimeProvider;

            // Assert
            Func<Task> act = async () => await condition.Resolve(new ProviderContext(automationData));
            await act.Should().ThrowAsync<ErrorException>();
        }
    }
}

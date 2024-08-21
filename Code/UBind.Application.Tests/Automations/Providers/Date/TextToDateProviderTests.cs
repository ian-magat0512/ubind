// <copyright file="TextToDateProviderTests.cs" company="uBind">
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
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Date;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class TextToDateProviderTests
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public TextToDateProviderTests()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task Resolve_ThrowsErrorException_WhenTextProviderIsMissing()
        {
            // Arrange
            var sut = new TextToDateProvider(null);
            AutomationData automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();

            // Act
            Func<Task> action = async () => await sut.Resolve(new ProviderContext(automationData));

            // Assert
            await action.Should().ThrowAsync<ErrorException>();
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("12pm")]
        [InlineData("12:23")]
        [InlineData("4 Jan '07")]
        public async Task Resolve_ThrowsErrorException_WhenTextProviderGivesInvalidString(string providedText)
        {
            // Arrange
            var textProvider = new StaticProvider<Data<string>>(providedText);
            var sut = new TextToDateProvider(textProvider);
            AutomationData automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();

            // Act
            Func<Task> action = async () => await sut.Resolve(new ProviderContext(automationData));

            // Assert
            await action.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task TextToDate_ShouldBeParseAsDate_WhenDateISOFormatIsValid()
        {
            // Arrange
            var pathLookUp = @"{""date"": ""2021-11-24""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToDate"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/date"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToDateProvider;

            // Assert
            var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            result.DataValue.Should().Be(new LocalDate(2021, 11, 24));
        }

        [Fact]
        public async Task TextToDate_ShouldBeParseAsDate_WhenDateFormatIsValidddMMyyyyWithSlashDelimiter()
        {
            // Arrange
            var pathLookUp = @"{""date"": ""11/12/2022""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToDate"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/date"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToDateProvider;

            // Assert
            var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            result.DataValue.Should().Be(new LocalDate(2022, 12, 11));
        }

        [Fact]
        public async Task TextToDate_ShouldBeParseAsDate_WhenDateIsValidddMMyyWithSlashDelimiter()
        {
            // Arrange
            var pathLookUp = @"{""date"": ""11/12/25""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToDate"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/date"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToDateProvider;

            // Assert
            var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            result.DataValue.Should().Be(new LocalDate(2025, 12, 11));
        }

        [Fact]
        public async Task TextToDate_ShouldBeParseAsDate_WhenDateFormatIsddMMyyyyWithDashDelimiter()
        {
            // Arrange
            var pathLookUp = @"{""date"": ""11-12-2023""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToDate"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/date"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToDateProvider;

            // Assert
            var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            result.DataValue.Should().Be(new LocalDate(2023, 12, 11));
        }

        [Fact]
        public async Task TextToDate_ShouldBeParseAsDate_WhenDateFormatIsddMMyyWithDashDelimiter()
        {
            // Arrange
            var pathLookUp = @"{""date"": ""11-12-21""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToDate"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/date"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToDateProvider;

            // Assert
            var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            result.DataValue.Should().Be(new LocalDate(2021, 12, 11));
        }

        [Fact]
        public async Task TextToDate_ShouldThrowErrorException_WhenDateIsInvalid()
        {
            // Arrange
            var pathLookUp = @"{""date"": ""11-JAN-22""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                ""textToDate"": {
                    ""objectPathLookupText"": {
                        ""path"": ""/date"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextToDateProvider;

            // Assert
            Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which
                .Error.Title.Should().Be("We couldn't recognise that as a valid date");
        }
    }
}

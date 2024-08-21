// <copyright file="TextConditionTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Conditions
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Conditions;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    public class TextConditionTest
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public TextConditionTest()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task TextIsEqualToCondition_ShouldReturnTrue_FromValidJsonWithoutIgnorecase()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""textIsEqualToCondition"": {
                            ""text"": ""ken"",
                            ""isEqualTo"": ""ken"",

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextIsEqualToCondition;

            // Assert
            var textIsEqual = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textIsEqual.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TextIsEqualToCondition_ShouldReturnTrue_FromValidJsonWithIgnorecase()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""textIsEqualToCondition"": {
                            ""text"": ""kengurow"",
                            ""isEqualTo"": ""KenGurow"",
                            ""ignoreCase"": true

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextIsEqualToCondition;

            // Assert
            var textIsEqual = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textIsEqual.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TextIsEqualToCondition_ShouldReturnFalse_FromValidJsonWithIgnorecase()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""textIsEqualToCondition"": {
                            ""text"": ""ken"",
                            ""isEqualTo"": ""test""

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextIsEqualToCondition;

            // Assert
            var textIsEqual = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textIsEqual.DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task TextContainsCondition_ShouldReturnTrue_FromValidJsonWithoutIgnorecase()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""textContainsCondition"": {
                            ""text"": ""KenGurow"",
                            ""contains"": ""Ken""

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextContainsCondition;

            // Assert
            var textContains = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textContains.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TextContainsCondition_ShouldReturnTrue_FromValidJsonWithIgnorecase()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""textContainsCondition"": {
                            ""text"": ""KenGurow"",
                            ""contains"": ""keng"",
                            ""ignoreCase"": true

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextContainsCondition;

            // Assert
            var textContains = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textContains.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TextContainsCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""textContainsCondition"": {
                            ""text"": ""KenGurow"",
                            ""contains"": ""test1""

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextContainsCondition;

            // Assert
            var textContains = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textContains.DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task TextStartsWithCondition_ShouldReturnTrue_FromValidJsonWithoutIgnoreCase()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""textStartsWithCondition"": {
                            ""text"": ""Hero"",
                            ""startsWith"": ""H""

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextStartsWithCondition;

            // Assert
            var textStarts = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textStarts.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TextStartsWithCondition_ShouldReturnTrue_FromValidJsonWithIgnoreCase()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""textStartsWithCondition"": {
                            ""text"": ""Hero"",
                            ""startsWith"": ""h"",
                            ""ignoreCase"": true

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextStartsWithCondition;

            // Assert
            var textStarts = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textStarts.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TextStartsWithCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""textStartsWithCondition"": {
                            ""text"": ""Hero"",
                            ""startsWith"": ""t""

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextStartsWithCondition;

            // Assert
            var textStarts = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textStarts.DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task TextEndsWithCondition_ShouldReturnTrue_FromValidJsonWithoutIgnoreCase()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""textEndsWithCondition"": {
                            ""text"": ""Hero"",
                            ""endsWith"": ""o""

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextEndsWithCondition;

            // Assert
            var textEnds = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textEnds.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TextEndsWithCondition_ShouldReturnTrue_FromValidJsonWithIgnoreCase()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""textEndsWithCondition"": {
                            ""text"": ""Hero"",
                            ""endsWith"": ""Ro"",
                            ""ignoreCase"": true

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextEndsWithCondition;

            // Assert
            var textEnds = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textEnds.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TextEndsWithCondition_ShouldReturnFalse_FromValidJsonWithIgnoreCase()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""textEndsWithCondition"": {
                            ""text"": ""Hero"",
                            ""endsWith"": ""to""

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as TextEndsWithCondition;

            // Assert
            var textEnds = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textEnds.DataValue.Should().BeFalse();
        }
    }
}

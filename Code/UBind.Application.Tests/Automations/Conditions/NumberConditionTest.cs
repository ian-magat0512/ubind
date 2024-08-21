// <copyright file="NumberConditionTest.cs" company="uBind">
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
    using UBind.Domain.Exceptions;
    using Xunit;

    public class NumberConditionTest
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public NumberConditionTest()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task NumberIsEqualToCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""numberIsEqualToCondition"": {
                            ""number"": 12.12,
                            ""isEqualTo"": 12.12,

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<decimal>;

            // Assert
            var numberIsEqual = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            numberIsEqual.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task NumberIsEqualToCondition_ShouldReturnErrorException_FromValidJsonInvalidData()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""numberIsEqualToCondition"": {
                            ""number"": ""asdasd"",
                            ""isEqualTo"": ""12.12"",

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<decimal>;
            Func<Task> act = async () => await condition.Resolve(new ProviderContext(automationData));

            // Assert
            await act.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task NumberIsEqualToCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""numberIsEqualToCondition"": {
                            ""number"": 12.12,
                            ""isEqualTo"": 12.123,

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<decimal>;

            // Assert
            var numberIsEqual = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            numberIsEqual.DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task NumberIsGreaterThanCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""numberIsGreaterThanCondition"": {
                            ""number"": 20.12,
                            ""isGreaterThan"": 12.12,

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<decimal>;

            // Assert
            var numberIsGreaterThan = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            numberIsGreaterThan.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task NumberIsGreaterThanCondition_ShouldReturnFalse_FromValidJsonWithWholeNumberValue()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""numberIsGreaterThanCondition"": {
                            ""number"": 10,
                            ""isGreaterThan"": 12.12,

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<decimal>;

            // Assert
            var numberIsGreaterThan = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            numberIsGreaterThan.DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task NumberIsGreaterThaOrEqualTonCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""numberIsGreaterThanOrEqualToCondition"": {
                            ""number"": 20.50,
                            ""isGreaterThanOrEqualTo"": 10.50,

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<decimal>;

            // Assert
            var numberIsGreaterThan = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            numberIsGreaterThan.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task NumberIsGreaterThaOrEqualToCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""numberIsGreaterThanOrEqualToCondition"": {
                            ""number"": 20.50,
                            ""isGreaterThanOrEqualTo"": 100.50,

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<decimal>;

            // Assert
            var numberIsGreaterThan = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            numberIsGreaterThan.DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task NumberIsLessThanCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""numberIsLessThanCondition"": {
                            ""number"": 20.50,
                            ""isLessThan"": 100.50,

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<decimal>;

            // Assert
            var numberIsLessThan = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            numberIsLessThan.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task NumberIsLessThanCondition_ShouldReturnFalse_WhenNumberIsHigherThanExpected()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""numberIsLessThanCondition"": {
                            ""number"": 200.50,
                            ""isLessThan"": 100.50,

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<decimal>;

            // Assert
            var numberIsLessThan = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            numberIsLessThan.DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task NumberIsLessThanOrEqualToCondition_ShouldReturnTrue_SameValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""numberIsLessThanOrEqualToCondition"": {
                            ""number"": 200.50,
                            ""isLessThanOrEqualTo"": 200.50,

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<decimal>;

            // Assert
            var numberIsLessThan = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            numberIsLessThan.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task NumberIsLessThanOrEqualToCondition_ShouldReturnTrue_IsNotMatchTheValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""numberIsLessThanOrEqualToCondition"": {
                            ""number"": 200.50,
                            ""isLessThanOrEqualTo"": 250.50,

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<decimal>;

            // Assert
            var numberIsLessThan = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            numberIsLessThan.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task NumberIsLessThanOrEqualToCondition_ShouldReturnFalse_TheValuesIsHigherThanToExpected()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""numberIsLessThanOrEqualToCondition"": {
                            ""number"": 300.50,
                            ""isLessThanOrEqualTo"": 200.50,

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<decimal>;

            // Assert
            var numberIsLessThan = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            numberIsLessThan.DataValue.Should().BeFalse();
        }
    }
}

// <copyright file="DatesAndTimeConditionTest.cs" company="uBind">
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
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Conditions;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    public class DatesAndTimeConditionTest
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public DatesAndTimeConditionTest()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task DateIsEqualToCondition_ShouldReturnTrue_FromValidJsonWithSameValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsEqualToCondition"": {
                            ""date"": ""2021-11-23"",
                            ""isEqualTo"": ""2021-11-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsEqual = await condition.Resolve(new ProviderContext(automationData));
            dateIsEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateIsEqualToCondition_ShouldReturnTFalse_FromValidJsonWithNotSameValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsEqualToCondition"": {
                            ""date"": ""2021-11-23"",
                            ""isEqualTo"": ""2021-12-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsEqual = await condition.Resolve(new ProviderContext(automationData));
            dateIsEqual.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task DateIsAfterCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsAfterCondition"": {
                            ""date"": ""2021-11-23"",
                            ""isAfter"": ""2021-05-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsAfter = await condition.Resolve(new ProviderContext(automationData));
            dateIsAfter.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateIsAfterCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsAfterCondition"": {
                            ""date"": ""2021-01-23"",
                            ""isAfter"": ""2021-05-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsAfter = await condition.Resolve(new ProviderContext(automationData));
            dateIsAfter.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task DateIsAfterOrEqualToCondition_ShouldReturnTrue_FromValidJsonWithSameValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsAfterOrEqualToCondition"": {
                            ""date"": ""2021-01-23"",
                            ""isAfterOrEqualTo"": ""2021-01-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsAfterOrEqual = await condition.Resolve(new ProviderContext(automationData));
            dateIsAfterOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateIsAfterOrEqualToCondition_ShouldReturnTrue_FromValidJsonWithAfterValue()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsAfterOrEqualToCondition"": {
                            ""date"": ""2021-03-23"",
                            ""isAfterOrEqualTo"": ""2021-01-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsAfterOrEqual = await condition.Resolve(new ProviderContext(automationData));
            dateIsAfterOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateIsAfterOrEqualToCondition_ShouldReturnFalse_FromValidJsonWithAfterValueIsNotMatch()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsAfterOrEqualToCondition"": {
                            ""date"": ""2021-03-23"",
                            ""isAfterOrEqualTo"": ""2021-11-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsAfterOrEqual = await condition.Resolve(new ProviderContext(automationData));
            dateIsAfterOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task DateIsBeforeCondition_ShouldReturnTrue_TrueFromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsBeforeCondition"": {
                            ""date"": ""2021-03-23"",
                            ""isBefore"": ""2021-11-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsBefore = await condition.Resolve(new ProviderContext(automationData));
            dateIsBefore.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateIsBeforeCondition_ShouldReturnFalse_FromValidJsonWithSameValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsBeforeCondition"": {
                            ""date"": ""2021-11-23"",
                            ""isBefore"": ""2021-11-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsBefore = await condition.Resolve(new ProviderContext(automationData));
            dateIsBefore.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task DateIsBeforeCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsBeforeCondition"": {
                            ""date"": ""2021-11-23"",
                            ""isBefore"": ""2021-05-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsBefore = await condition.Resolve(new ProviderContext(automationData));
            dateIsBefore.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task DateIsBeforeOrEqualToCondition_ShouldReturnTrue_FromValidJsonWithSameValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsBeforeOrEqualToCondition"": {
                            ""date"": ""2021-11-23"",
                            ""isBeforeOrEqualTo"": ""2021-11-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsBeforeOrEqual = await condition.Resolve(new ProviderContext(automationData));
            dateIsBeforeOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateIsBeforeOrEqualToCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsBeforeOrEqualToCondition"": {
                            ""date"": ""2021-05-23"",
                            ""isBeforeOrEqualTo"": ""2021-11-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsBeforeOrEqual = await condition.Resolve(new ProviderContext(automationData));
            dateIsBeforeOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateIsBeforeOrEqualToCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateIsBeforeOrEqualToCondition"": {
                            ""date"": ""2021-05-23"",
                            ""isBeforeOrEqualTo"": ""2021-01-23""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalDate>;

            // Assert
            var dateIsBeforeOrEqual = await condition.Resolve(new ProviderContext(automationData));
            dateIsBeforeOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task TimeIsEqualToCondition_ShouldReturnTrue_FromValidJsonWithSameValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""timeIsEqualToCondition"": {
                            ""time"": ""20:30:00"",
                            ""isEqualTo"": ""20:30:00""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalTime>;

            // Assert
            var timeIsEqual = await condition.Resolve(new ProviderContext(automationData));
            timeIsEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TimeIsEqualToCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""timeIsEqualToCondition"": {
                            ""time"": ""10:30:00"",
                            ""isEqualTo"": ""20:30:00""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalTime>;

            // Assert
            var timeIsEqual = await condition.Resolve(new ProviderContext(automationData));
            timeIsEqual.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task TimeIsAfterCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""timeIsAfterCondition"": {
                            ""time"": ""10:30:00"",
                            ""isAfter"": ""05:30:00""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalTime>;

            // Assert
            var timeIsAfter = await condition.Resolve(new ProviderContext(automationData));
            timeIsAfter.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TimeIsAfterCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""timeIsAfterCondition"": {
                            ""time"": ""10:30:00"",
                            ""isAfter"": ""15:30:00""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalTime>;

            // Assert
            var timeIsAfter = await condition.Resolve(new ProviderContext(automationData));
            timeIsAfter.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task TimeIsAfterOrEqualToCondition_ShouldReturnTrue_FromValidJsonWithSameValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""timeIsAfterOrEqualToCondition"": {
                            ""time"": ""15:30:00"",
                            ""isAfterOrEqualTo"": ""15:30:00""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalTime>;

            // Assert
            var timeIsAfterOrEqual = await condition.Resolve(new ProviderContext(automationData));
            timeIsAfterOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TimeIsAfterOrEqualToCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""timeIsAfterOrEqualToCondition"": {
                            ""time"": ""20:30:00"",
                            ""isAfterOrEqualTo"": ""15:30:00""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalTime>;

            // Assert
            var timeIsAfterOrEqual = await condition.Resolve(new ProviderContext(automationData));
            timeIsAfterOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TimeIsAfterOrEqualToCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""timeIsAfterOrEqualToCondition"": {
                            ""time"": ""05:30:00"",
                            ""isAfterOrEqualTo"": ""15:30:00""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalTime>;

            // Assert
            var timeIsAfterOrEqual = await condition.Resolve(new ProviderContext(automationData));
            timeIsAfterOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task TimeIsBeforeCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""timeIsBeforeCondition"": {
                            ""time"": ""05:30:00"",
                            ""isBefore"": ""15:30:00""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalTime>;

            // Assert
            var timeIsBefore = await condition.Resolve(new ProviderContext(automationData));
            timeIsBefore.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TimeIsBeforeCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""timeIsBeforeCondition"": {
                            ""time"": ""22:30:00"",
                            ""isBefore"": ""15:30:00""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalTime>;

            // Assert
            var timeIsBefore = await condition.Resolve(new ProviderContext(automationData));
            timeIsBefore.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task TimeIsBeforeOrEqualToCondition_ShouldReturnTrue_FromValidJsonWithSameValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""timeIsBeforeOrEqualToCondition"": {
                            ""time"": ""15:30:00"",
                            ""isBeforeOrEqualTo"": ""15:30:00""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalTime>;

            // Assert
            var timeIsBeforeOrEqual = await condition.Resolve(new ProviderContext(automationData));
            timeIsBeforeOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TimeIsBeforeOrEqualToCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""timeIsBeforeOrEqualToCondition"": {
                            ""time"": ""01:30:00"",
                            ""isBeforeOrEqualTo"": ""05:30:00""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalTime>;

            // Assert
            var timeIsBeforeOrEqual = await condition.Resolve(new ProviderContext(automationData));
            timeIsBeforeOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task TimeIsBeforeOrEqualToCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""timeIsBeforeOrEqualToCondition"": {
                            ""time"": ""11:30:00"",
                            ""isBeforeOrEqualTo"": ""05:30:00""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<LocalTime>;

            // Assert
            var timeIsBeforeOrEqual = await condition.Resolve(new ProviderContext(automationData));
            timeIsBeforeOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task DateTimeIsEqualToCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsEqualToCondition"": {
                            ""dateTime"": ""2021-11-24T21:00:00Z"",
                            ""isEqualTo"": ""2021-11-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsEqual = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateTimeIsEqualToCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsEqualToCondition"": {
                            ""dateTime"": ""2021-10-24T21:00:00Z"",
                            ""isEqualTo"": ""2021-11-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsEqual = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsEqual.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task DateTimeIsAfterCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsAfterCondition"": {
                            ""dateTime"": ""2021-10-24T21:00:00Z"",
                            ""isAfter"": ""2021-05-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsAfter = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsAfter.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateTimeIsAfterCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsAfterCondition"": {
                            ""dateTime"": ""2021-01-24T21:00:00Z"",
                            ""isAfter"": ""2021-05-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsAfter = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsAfter.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task DateTimeIsAfterOrEqualToCondition_ShouldReturnTrue_FromValidJsonWithSameValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsAfterOrEqualToCondition"": {
                            ""dateTime"": ""2021-10-24T21:00:00Z"",
                            ""isAfterOrEqualTo"": ""2021-10-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsAfterOrEqual = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsAfterOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateTimeIsAfterOrEqualToCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsAfterOrEqualToCondition"": {
                            ""dateTime"": ""2021-10-24T21:00:00Z"",
                            ""isAfterOrEqualTo"": ""2021-05-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsAfterOrEqual = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsAfterOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateTimeIsAfterOrEqualToCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsAfterOrEqualToCondition"": {
                            ""dateTime"": ""2021-10-24T21:00:00Z"",
                            ""isAfterOrEqualTo"": ""2021-12-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsAfterOrEqual = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsAfterOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task DateTimeIsBeforeCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsBeforeCondition"": {
                            ""dateTime"": ""2021-10-24T21:00:00Z"",
                            ""isBefore"": ""2021-12-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsBefore = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsBefore.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateTimeIsBeforeCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsBeforeCondition"": {
                            ""dateTime"": ""2021-10-24T21:00:00Z"",
                            ""isBefore"": ""2021-05-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsBefore = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsBefore.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task DateTimeIsBeforeOrEqualToCondition_ShouldReturnTrue_FromValidJsonWithSameValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsBeforeOrEqualToCondition"": {
                            ""dateTime"": ""2021-10-24T21:00:00Z"",
                            ""isBeforeOrEqualTo"": ""2021-10-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsBeforeOrEqual = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsBeforeOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateTimeIsBeforeOrEqualToCondition_ShouldReturnTrue_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsBeforeOrEqualToCondition"": {
                            ""dateTime"": ""2021-05-24T21:00:00Z"",
                            ""isBeforeOrEqualTo"": ""2021-10-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsBeforeOrEqual = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsBeforeOrEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task DateTimeIsBeforeOrEqualToCondition_ShouldReturnFalse_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsBeforeOrEqualToCondition"": {
                            ""dateTime"": ""2021-05-24T21:00:00Z"",
                            ""isBeforeOrEqualTo"": ""2021-01-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsBefore = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsBefore.GetValueOrThrowIfFailed().DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task DateTimeIsEqualToCondition_ShouldReturnTrue_FromValidJsonWithDateAndTimeDateTime()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""dateTimeIsEqualToCondition"": {
                            ""dateTime"": {
                                ""dateAndTimeDateTime"": {
                                    ""date"": ""2021-01-24"",
                                    ""time"": ""21:00:00""
                                }
                            },
                            ""isEqualTo"": ""2021-01-24T21:00:00Z""
               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ComparisonCondition<Instant>;

            // Assert
            var dateTimeIsEqual = await condition.Resolve(new ProviderContext(automationData));
            dateTimeIsEqual.GetValueOrThrowIfFailed().DataValue.Should().BeTrue();
        }
    }
}

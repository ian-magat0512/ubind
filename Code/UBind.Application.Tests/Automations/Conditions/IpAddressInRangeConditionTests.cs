// <copyright file="IpAddressInRangeConditionTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class IpAddressInRangeConditionTests
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public IpAddressInRangeConditionTests()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task IpAddressInRangeCondition_ShouldReturnTrue_WhenIpAddressIsWithinRange()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""ipAddressInRangeCondition"": {
                            ""ipAddress"": ""192.168.33.1"",
                            ""isInRange"": ""192.168.33.0/13""

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = (IpAddressInRangeCondition)model!.Build(this.dependencyProvider);

            // Assert
            var ipIsInRange = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            ipIsInRange.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task IpAddressInRangeCondition_ShouldReturnFalse_WhenIpAddressIsNotInRange()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""ipAddressInRangeCondition"": {
                            ""ipAddress"": ""11.15.5.2"",
                            ""isInRange"": ""10.15.5.1/10""

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = (IpAddressInRangeCondition)model!.Build(this.dependencyProvider);

            // Assert
            var ipIsInRange = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            ipIsInRange.DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task IpAddressInRangeCondition_ShouldThrowError_WhenIpAddressPropertyIsInValid()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""ipAddressInRangeCondition"": {
                            ""ipAddress"": ""invalid ip address"",
                            ""isInRange"": ""192.168.33.3""

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = (IpAddressInRangeCondition)model!.Build(this.dependencyProvider);
            var errorThrown = Errors.Automation.IpAddressInRangeCondition.IpAddressFormatError("invalid ip address");

            // Assert
            Func<Task> act = async () => await condition.Resolve(new ProviderContext(automationData));
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be(errorThrown.Title);
            exception.Which.Error.Message.Should().Contain($"IP address \"invalid ip address\"");
        }

        [Fact]
        public async Task IpAddressInRangeCondition_ShouldThrowError_WhenIsInRangePropertyIsInValid()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""ipAddressInRangeCondition"": {
                            ""ipAddress"": ""192.168.33.3"",
                            ""isInRange"": ""invalid ip address""

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = (IpAddressInRangeCondition)model!.Build(this.dependencyProvider);
            var errorThrown = Errors.Automation.IpAddressInRangeCondition.IpAddressRangeFormatError("invalid ip address");

            // Assert
            Func<Task> act = async () => await condition.Resolve(new ProviderContext(automationData));
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be(errorThrown.Title);
            exception.Which.Error.Message.Should().Contain($"IP address range \"invalid ip address\"");
        }

        [Fact]
        public async Task IpAddressInRangeCondition_ShouldThrowError_WhenSubnetMaskLengthIsInValid()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""ipAddressInRangeCondition"": {
                            ""ipAddress"": ""192.168.33.3"",
                            ""isInRange"": ""192.168.33.0/test""

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = (IpAddressInRangeCondition)model!.Build(this.dependencyProvider);
            var errorThrown = Errors.Automation.IpAddressInRangeCondition.SubnetMaskInvalid("test");

            // Assert
            Func<Task> act = async () => await condition.Resolve(new ProviderContext(automationData));
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be(errorThrown.Title);
            exception.Which.Error.Message.Should().Contain($"subnet mask length");
        }
    }
}

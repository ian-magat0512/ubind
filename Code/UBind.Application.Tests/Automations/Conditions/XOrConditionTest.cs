// <copyright file="XOrConditionTest.cs" company="uBind">
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

    public class XOrConditionTest
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public XOrConditionTest()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task XOrCondition_ShouldReturnTrue_FromValidJsonInOtherCondition()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""xorCondition"": {
                            ""firstCondition"": {
                                ""textIsEqualToCondition"":{
                                    ""text"": ""sample1"",
                                    ""isEqualTo"": ""sample1""
                                }
                            },
                            ""secondCondition"": {
                                ""textIsEqualToCondition"":{
                                    ""text"": ""sample1"",
                                    ""isEqualTo"": ""sample2""
                                }
                            },

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as XOrCondition;

            // Assert
            var xor = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            xor.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task XOrCondition_ShouldReturnFalse_FromValidJsonWithStaticValue()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""xorCondition"": {
                            ""firstCondition"": true,
                            ""secondCondition"": true,

               }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as XOrCondition;

            // Assert
            var xor = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            xor.DataValue.Should().BeFalse();
        }
    }
}

// <copyright file="SetAdditionalPropertyValueActionIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Actions
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions.AdditionalPropetyValueActions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Unit tests for the SetAdditionalPropertyValue actions.
    /// </summary>
    public class SetAdditionalPropertyValueActionIntegrationTests
    {
        [Theory]
        [InlineData("quoteVersion")]
        [InlineData("quote")]
        [InlineData("policy")]
        [InlineData("policyTransaction")]
        [InlineData("claimVersion")]
        [InlineData("claim")]
        [InlineData("user")]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("customer")]

        public async Task SetAdditionalPropertyValueAction_Should_Update_Entity_Additional_Property(string entityType)
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var json = @"{
                        ""name"": ""Set AdditionalProperty Value Action"",
                        ""alias"": ""setAdditionalPropertyValueAction"",
                        ""description"": ""A test of setting additional property value"",
                        ""asynchronous"": false,
                        ""entity"": {
                            ""dynamicEntity"": 
                            {
                                ""entityType"": """ + entityType + @""",
                                ""entityId"": """ + entityId.ToString() + @"""
                            }
                        },
                        ""propertyAlias"": ""test"",
                        ""value"": ""testing"",
                    }";
            var mockServiceProvider = MockAutomationData.GetServiceProviderForAdditionalProperties(entityId, entityType);
            var setAdditionalPropertyValueActionBuilder
                = JsonConvert.DeserializeObject<SetAdditionalPropertyValueActionConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);
            var setAdditionalPropertyValueAction = setAdditionalPropertyValueActionBuilder.Build(mockServiceProvider);
            var actionData = new SetAdditionalPropertyValueActionData(
                setAdditionalPropertyValueAction.Name, setAdditionalPropertyValueAction.Alias, new TestClock());
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            await setAdditionalPropertyValueAction.Execute(
                new ProviderContext(automationData),
                actionData);

            // Assert
            actionData.EntityId.Should().Be(entityId.ToString());
            actionData.EntityType.ToLowerInvariant().Should().Be(entityType.ToLowerInvariant());
            actionData.ResultingValue.Should().Be("testing");
        }
    }
}

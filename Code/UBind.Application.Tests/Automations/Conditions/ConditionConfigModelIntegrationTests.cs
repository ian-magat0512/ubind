// <copyright file="ConditionConfigModelIntegrationTests.cs" company="uBind">
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
    using UBind.Application.Tests.Automations.Fakes;

    public abstract class ConditionConfigModelIntegrationTests
    {
        protected async Task VerifyConditionResolutionAsync(string json, bool expectedResult)
        {
            // Arrange
            var dependencyProvider = new Mock<IServiceProvider>();
            var automationData = MockAutomationData.CreateWithEventTrigger();

            // Act
            var sut = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var provider = sut.Build(dependencyProvider.Object);
            bool result = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}

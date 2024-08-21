// <copyright file="EnvironmentTextProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Text
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Text;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class EnvironmentTextProviderTests
    {
        [Fact]
        public async Task EnvironmentTextProvider_ShouldBeAbleToProvideText_WhenConfiguredForDevelopment()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var developmentTextProvider = new StaticProvider<Data<string>>("text for dev");
            var defaultProvider = new StaticProvider<Data<string>>("default text");
            var environemtnTextProvider
                = new EnvironmentTextProvider(defaultProvider, developmentTextProvider, null, null);

            // Act
            string providedString = (await environemtnTextProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            providedString.Should().Be("text for dev");
        }

        [Fact]
        public void EnvironmentTextProvider_ShouldRaiseException_WhenThereIsNoDefaultProviderConfigured()
        {
            // Arrange
            var stagingTextProvider = new StaticProvider<Data<string>>("text for staging");

            // Act
            Func<EnvironmentTextProvider> func = () => new EnvironmentTextProvider(null, null, stagingTextProvider, null);

            // Assert
            func.Should().Throw<ErrorException>();
        }

        [Fact]
        public void EnvironmentTextProvider_ShouldRaiseException_WhenEnvironmentRequestedIsNotConfiguredAndDefaultIsNull()
        {
            // Arrange
            var developmentTextProvider = new StaticProvider<Data<string>>("text for dev");
            var stagingTextProvider = new StaticProvider<Data<string>>("text for staging");
            var productionTextProvider = new StaticProvider<Data<string>>("default text");
            var environmentTextProvider = new EnvironmentTextProvider(null, developmentTextProvider, stagingTextProvider, productionTextProvider);
            var automationData = AutomationData.CreateFromHttpRequest(
                TenantFactory.DefaultId,
                default,
                ProductFactory.DefaultId,
                Guid.NewGuid(),
                Domain.DeploymentEnvironment.None,
                null,
                MockAutomationData.GetDefaultServiceProvider());

            // Act + Assert
            Func<Task> action = async () => await environmentTextProvider.Resolve(new ProviderContext(automationData));
            var result = action.Should().ThrowAsync<ErrorException>();
        }
    }
}

// <copyright file="RaiseErrorActionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    public class RaiseErrorActionTests
    {
        [Fact]
        public async Task RaiseErrorAction_ShouldReturn_RaisedErrorInProperFormat()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);
            var raiseErrorAction = this.CreateAction(automationData.ServiceProvider);
            var actionData = raiseErrorAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await raiseErrorAction.Execute(providerContext, actionData, false);

            // Assert
            automationData.Actions.TryGetValue("actionTest", out ActionData data);
            var retrievedActionData = data as RaiseErrorActionData;
            var raisedError = JsonConvert.SerializeObject(retrievedActionData);
            retrievedActionData.RaisedError.Should().NotBeNull();
            raisedError.Should().NotBeNull();
            raisedError.Should().Contain("code");
            raisedError.Should().Contain("title");
            raisedError.Should().Contain("message");
            raisedError.Should().Contain("httpStatusCode");
            raisedError.Should().Contain("additionalDetails");
            raisedError.Should().Contain("data");
        }

        private RaiseErrorAction CreateAction(IServiceProvider serviceProvider)
        {
            var additionalDetailsList = new List<IProvider<Data<string>>>()
            {
                new StaticProvider<Data<string>>("Error message 1"),
                new StaticProvider<Data<string>>("Error message 2"),
            };
            var errorDataProperties = new Dictionary<IProvider<Data<string>>, IProvider<IData>>()
            {
                { new StaticProvider<Data<string>>("tenantId"), (IProvider<IData>)new StaticProvider<Data<string>>("foo") },
                { new StaticProvider<Data<string>>("productId"), (IProvider<IData>)new StaticProvider<Data<string>>("bar") },
            };
            var errorDataProvider = new DynamicObjectProvider(
                errorDataProperties,
                new Mock<ILogger<DynamicObjectProvider>>().Object,
                serviceProvider);
            var errorProvider = new ErrorProvider(
                new StaticProvider<Data<string>>("test.error"),
                new StaticProvider<Data<string>>("Test Error"),
                new StaticProvider<Data<string>>("Test messages"),
                new StaticProvider<Data<long>>(400),
                additionalDetailsList,
                errorDataProvider);
            var action = new RaiseErrorAction(
                "test",
                "actionTest",
                string.Empty,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Application.Automation.Actions.Action>(),
                errorProvider,
                SystemClock.Instance);
            return action;
        }
    }
}

// <copyright file="ActionRunnerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Flurl.Http.Testing;
    using Moq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Conditions;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using Action = UBind.Application.Automation.Actions.Action;
    using ConfiguredError = UBind.Application.Automation.Error.ConfiguredError;

    public class ActionRunnerTests : IAsyncLifetime
    {
        private IActionRunner runner;
        private AutomationData mockData;
        private HttpRequestConfiguration httpRequest;
        private IProvider<Data<bool>> trueCondition;
        private IProvider<Data<bool>> falseCondition;
        private IProvider<ConfiguredError> errorProvider;

        public async Task InitializeAsync()
        {
            var jobClient = new Mock<IJobClient>();
            var cachingResolver = new Mock<ICachingResolver>();
            var mockMediator = new Mock<ICqrsMediator>();
            this.runner = new ActionRunner(jobClient.Object, cachingResolver.Object, mockMediator.Object);
            this.mockData = await MockAutomationData.CreateWithHttpTrigger();
            var emptyHeader = Enumerable.Empty<IProvider<KeyValuePair<string, IEnumerable<string>>>>();
            this.httpRequest = new HttpRequestConfiguration(
                new StaticProvider<Data<string>>("https://app.ubind.com.au/admin"),
                new StaticProvider<Data<string>>("get"),
                emptyHeader,
                null,
                null,
                null,
                null);
            this.trueCondition = new TextStartsWithCondition(
                new StaticProvider<Data<string>>("foo"),
                new StaticProvider<Data<string>>("f"),
                new StaticProvider<Data<bool>>(false));
            this.falseCondition = new TextEndsWithCondition(new StaticProvider<Data<string>>("bar"), new StaticProvider<Data<string>>("z"), new StaticProvider<Data<bool>>(false));

            var staticString = new StaticProvider<Data<string>>(string.Empty);
            var staticInt = new StaticProvider<Data<long>>(400);
            var staticStringList = new List<IProvider<Data<string>>>();
            staticStringList.Add(staticString);
            this.errorProvider = new ErrorProvider(staticString, staticString, staticString, staticInt, staticStringList, null);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Action_ShouldRun_IfThereIsNoRunConditionSpecified()
        {
            // Arrange
            var action = this.CreateBasicAction();
            var actionData = action.CreateActionData();
            this.mockData.AddActionData(actionData);

            // Act
            using (var httpTest = new HttpTest())
            {
                await this.runner.HandleAction(this.mockData, action, actionData, CancellationToken.None);
            }

            // Assert
            var httpActionData = this.mockData.Actions.FirstOrDefault().Value as HttpRequestActionData;
            httpActionData.HttpResponse.Should().NotBeNull();
            httpActionData.Started.Should().BeTrue();
            httpActionData.StartedDateTime.Should().NotBeNull();
            httpActionData.Finished.Should().BeTrue();
        }

        [Fact]
        public async Task Action_ShouldRun_IfRunConditionExistsAndIsSatisfied()
        {
            // Arrange
            var action = this.CreateActionWithPassingRunCondition();
            var actionData = action.CreateActionData();
            this.mockData.AddActionData(actionData);

            // Act
            using (var httpTest = new HttpTest())
            {
                await this.runner.HandleAction(this.mockData, action, actionData, CancellationToken.None);
            }

            // Assert
            var httpActionData = this.mockData.Actions.FirstOrDefault().Value as HttpRequestActionData;
            httpActionData.HttpResponse.Should().NotBeNull();
            httpActionData.Started.Should().BeTrue();
            httpActionData.Succeeded.Should().BeTrue();
            httpActionData.Finished.Should().BeTrue();
        }

        [Fact]
        public async Task Action_ShouldNotRun_IfRunConditionExistsAndIsNotSatisfied()
        {
            // Arrange
            var action = this.CreateActionWithFailingRunCondition();
            var actionData = action.CreateActionData();
            this.mockData.AddActionData(actionData);

            // Act
            using (var httpTest = new HttpTest())
            {
                await this.runner.HandleAction(this.mockData, action, actionData, CancellationToken.None);
            }

            // Assert
            var httpActionData = this.mockData.Actions.FirstOrDefault().Value as HttpRequestActionData;
            httpActionData.HttpResponse.Should().BeNull();
            actionData.Started.Should().BeTrue();
            actionData.Ran.Should().BeFalse();
            actionData.Finished.Should().BeFalse();
        }

        [Fact]
        public async Task Action_ShouldSucceed_IfBeforeRunValidationIsConfiguredAndReturnedFalse()
        {
            // Arrange
            var action = this.CreateActionWithPassingBeforeRunErrorCondition();
            var actionData = action.CreateActionData();
            this.mockData.AddActionData(actionData);

            // Act
            using (var httpTest = new HttpTest())
            {
                await this.runner.HandleAction(this.mockData, action, actionData, CancellationToken.None);
            }

            // Assert
            var httpActionData = this.mockData.Actions.FirstOrDefault().Value as HttpRequestActionData;
            httpActionData.HttpResponse.Should().NotBeNull();
            actionData.Error.Should().BeNull();
            actionData.Started.Should().BeTrue();
            actionData.Ran.Should().BeTrue();
            actionData.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task Action_ShouldNotRun_IfBeforeRunValidationIsConfiguredAndReturnedTrue()
        {
            // Arrange
            var action = this.CreateActionWithFailingBeforeRunErrorCondition();
            var actionData = action.CreateActionData();
            this.mockData.AddActionData(actionData);

            // Act
            using (var httpTest = new HttpTest())
            {
                await this.runner.HandleAction(this.mockData, action, actionData, CancellationToken.None);
            }

            // Assert
            var httpActionData = this.mockData.Actions.FirstOrDefault().Value as HttpRequestActionData;
            httpActionData.HttpResponse.Should().BeNull();
            actionData.Error.Should().NotBeNull();
            actionData.Started.Should().BeTrue();
            actionData.Ran.Should().BeFalse();
            actionData.Succeeded.Should().BeFalse();
        }

        [Fact]
        public async Task Action_ShouldSucceed_IfAfterRunConditionIsConfiguredAndReturnedFalse()
        {
            // Arrange
            var action = this.CreateActionWithPassingAfterRunErrorCondition();
            var actionData = action.CreateActionData();
            this.mockData.AddActionData(actionData);

            // Act
            using (var httpTest = new HttpTest())
            {
                await this.runner.HandleAction(this.mockData, action, actionData, CancellationToken.None);
            }

            // Assert
            var httpActionData = this.mockData.Actions.FirstOrDefault().Value as HttpRequestActionData;
            httpActionData.HttpResponse.Should().NotBeNull();
            actionData.Error.Should().BeNull();
            actionData.Started.Should().BeTrue();
            actionData.Ran.Should().BeTrue();
            actionData.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task Action_ShouldNotSucceed_IfAfterRunConditionIsConfiguredAndReturnedTrue()
        {
            // Arrange
            var action = this.CreateActionWithFailingAfterRunErrorCondition();
            var actionData = action.CreateActionData();
            this.mockData.AddActionData(actionData);

            // Act
            using (var httpTest = new HttpTest())
            {
                await this.runner.HandleAction(this.mockData, action, actionData, CancellationToken.None);
            }

            // Assert
            var httpActionData = this.mockData.Actions.FirstOrDefault().Value as HttpRequestActionData;
            httpActionData.HttpResponse.Should().NotBeNull();
            actionData.Error.Should().NotBeNull();
            actionData.Started.Should().BeTrue();
            actionData.Ran.Should().BeTrue();
            actionData.Succeeded.Should().BeFalse();
        }

        private Action CreateBasicAction(IProvider<Data<bool>> runCondition = null, List<ErrorCondition> beforeRunErrorCondition = null, List<ErrorCondition> afterRunErrorCondition = null)
        {
            return new HttpRequestAction(
                string.Empty,
                "actionRunnerTest",
                string.Empty,
                false,
                runCondition ?? null,
                beforeRunErrorCondition ?? Enumerable.Empty<ErrorCondition>(),
                afterRunErrorCondition ?? Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                this.httpRequest,
                new TestClock());
        }

        private Action CreateActionWithPassingRunCondition()
        {
            return this.CreateBasicAction(this.trueCondition);
        }

        private Action CreateActionWithFailingRunCondition()
        {
            return this.CreateBasicAction(this.falseCondition);
        }

        private Action CreateActionWithPassingBeforeRunErrorCondition()
        {
            var beforeRunConditionList = new List<ErrorCondition>();
            beforeRunConditionList.Add(new ErrorCondition(this.falseCondition, this.errorProvider));
            return this.CreateBasicAction(beforeRunErrorCondition: beforeRunConditionList);
        }

        private Action CreateActionWithFailingBeforeRunErrorCondition()
        {
            var beforeRunConditionList = new List<ErrorCondition>();
            beforeRunConditionList.Add(new ErrorCondition(this.trueCondition, this.errorProvider));
            return this.CreateBasicAction(beforeRunErrorCondition: beforeRunConditionList);
        }

        private Action CreateActionWithPassingAfterRunErrorCondition()
        {
            var afterRunErrorConditionList = new List<ErrorCondition>();
            afterRunErrorConditionList.Add(new ErrorCondition(this.falseCondition, this.errorProvider));
            return this.CreateBasicAction(afterRunErrorCondition: afterRunErrorConditionList);
        }

        private Action CreateActionWithFailingAfterRunErrorCondition()
        {
            var afterRunErrorConditionList = new List<ErrorCondition>();
            afterRunErrorConditionList.Add(new ErrorCondition(this.trueCondition, this.errorProvider));
            return this.CreateBasicAction(afterRunErrorCondition: afterRunErrorConditionList);
        }
    }
}

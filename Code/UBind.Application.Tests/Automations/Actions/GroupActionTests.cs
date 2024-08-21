// <copyright file="GroupActionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Actions
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using FluentAssertions;
    using Flurl.Http.Testing;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class GroupActionTests : IAsyncLifetime
    {
        private ICachingResolver mockCachingResolver;
        private IJobClient mockJobClient;
        private HttpTest testSuite;
        private AutomationData mockAutomationData;
        private IActionRunner actionRunner;
        private ProviderContext providerContext;
        private Mock<ICqrsMediator> mockMediator;

        public async Task InitializeAsync()
        {
            this.mockAutomationData = await MockAutomationData.CreateWithHttpTrigger();
            this.providerContext = new ProviderContext(this.mockAutomationData);
            this.mockJobClient = new Mock<IJobClient>().Object;
            this.mockCachingResolver = new Mock<ICachingResolver>().Object;
            this.mockMediator = new Mock<ICqrsMediator>();
            this.actionRunner = new ActionRunner(this.mockJobClient, this.mockCachingResolver, this.mockMediator.Object);
            this.testSuite = new HttpTest();
            this.testSuite.RespondWithJson(new { response = "OK" }, 200);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task GroupAction_ShouldSucceed_IfAllChildActionsSucceed()
        {
            // Arrange
            var groupAction = this.Setup();
            var actionData = groupAction.CreateActionData();
            actionData.UpdateState(ActionState.Started);
            actionData.UpdateState(ActionState.BeforeRunErrorChecking);
            Result<Domain.Helpers.Void, Domain.Error> executionResult;

            // Act
            using (this.testSuite)
            {
                executionResult = await groupAction.Execute(this.providerContext, actionData);
            }

            // Assert
            executionResult.IsSuccess.Should().BeTrue();
            actionData.State.Should().Be(ActionState.Running);

            var groupActionData = (GroupActionData)actionData;
            groupActionData.Actions.Keys.Count.Should().Be(2);
            groupActionData.Actions.Keys.Should().Contain("httpActionTest");
            groupActionData.Actions.Keys.Should().Contain("raiseErrorTestWithHandling");

            var hasFirstChild = groupActionData.Actions.TryGetValue("httpActionTest", out ActionData httpActionData);
            if (hasFirstChild)
            {
                httpActionData.State.Should().Be(ActionState.Completed);
            }

            var hasSecondChild = groupActionData.Actions.TryGetValue("raiseErrorTestWithHandling", out ActionData raiseErrorActionData);
            if (hasSecondChild)
            {
                raiseErrorActionData.State.Should().Be(ActionState.Completed);
            }
        }

        [Fact]
        public async Task GroupAction_ShouldExecuteErrorHandling_And_Succeed_IfChildActionErrorIsNotHandledByChildAction()
        {
            // Arrange
            var groupAction = this.Setup(childActionErrors: true, hasParentHandling: true);
            var actionData = groupAction.CreateActionData();
            this.mockAutomationData.AddActionData(actionData);

            // Act
            using (var httpTest = new HttpTest())
            {
                await this.actionRunner.HandleAction(this.mockAutomationData, groupAction, actionData, CancellationToken.None);
            }

            // Assert
            actionData.State.Should().Be(ActionState.Completed);

            var groupActionData = (GroupActionData)actionData;
            groupActionData.Actions.Keys.Count.Should().Be(2);
            groupActionData.Actions.Keys.Should().Contain("httpActionTest");
            groupActionData.Actions.Keys.Should().Contain("raiseErrorTestWithoutHandling");

            var hasFirstChild = groupActionData.Actions.TryGetValue("httpActionTest", out ActionData httpActionData);
            if (hasFirstChild)
            {
                httpActionData.State.Should().Be(ActionState.Completed);
            }

            var hasSecondChild = groupActionData.Actions.TryGetValue("raiseErrorTestWithoutHandling", out ActionData raiseErrorActionData);
            if (hasSecondChild)
            {
                raiseErrorActionData.State.Should().Be(ActionState.Completed);
                raiseErrorActionData.Error.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GroupAction_ShouldFail_And_ReturnChildActionError_IfChildActionErrorIsNotHandledByParentAndChild()
        {
            // Arrange
            var groupAction = this.Setup(childActionErrors: true, hasParentHandling: false);
            var actionData = groupAction.CreateActionData();
            this.mockAutomationData.AddActionData(actionData);

            // Act
            using (this.testSuite)
            {
                await this.actionRunner.HandleAction(this.mockAutomationData, groupAction, actionData, CancellationToken.None);
            }

            // Assert
            actionData.State.Should().Be(ActionState.Completed);
            actionData.Succeeded.Should().BeFalse();

            var groupActionData = (GroupActionData)actionData;
            groupActionData.Actions.Keys.Count.Should().Be(2);
            groupActionData.Actions.Keys.Should().Contain("httpActionTest");
            groupActionData.Actions.Keys.Should().Contain("raiseErrorTestWithoutHandling");

            var hasFirstChild = groupActionData.Actions.TryGetValue("httpActionTest", out ActionData httpActionData);
            if (hasFirstChild)
            {
                httpActionData.State.Should().Be(ActionState.Completed);
            }

            var hasSecondChild = groupActionData.Actions.TryGetValue("raiseErrorTestWithoutHandling", out ActionData raiseErrorActionData);
            if (hasSecondChild)
            {
                raiseErrorActionData.State.Should().Be(ActionState.Completed);
                raiseErrorActionData.Error.Should().NotBeNull();
            }

            groupActionData.Error.Should().NotBeNull();
            var expectedError = (await this.GetTestAutomationError().Resolve(this.providerContext)).GetValueOrThrowIfFailed();
            groupActionData.Error.Code.Should().Be(expectedError.Code);
            groupActionData.Error.Message.Should().Be(expectedError.Message);
        }

        [Fact]
        public async Task ExecuteWithChildActions_ShouldRunInParallel_WhenParallelIsTrue()
        {
            // Arrange
            var childActions = new List<MockAction>
            {
                this.CreateMockAction(),
                this.CreateMockAction(),
                this.CreateMockAction(),
            };

            var groupAction = this.CreateGroupAction(childActions, isParallel: true);
            var actionData = groupAction.CreateActionData();

            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await groupAction.Execute(this.providerContext, actionData);
            stopwatch.Stop();

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task GroupAction_ShouldFailAndReturnChildActionError_WhenRunningInParallel()
        {
            // Arrange
            var groupAction = this.Setup(childActionErrors: true, hasParentHandling: false, parallel: true);
            var actionData = groupAction.CreateActionData();
            this.mockAutomationData.AddActionData(actionData);

            // Act
            var result = await groupAction.Execute(this.providerContext, actionData);

            // Assert
            result.IsFailure.Should().BeTrue();
            var expectedError = (await this.GetTestAutomationError().Resolve(this.providerContext)).GetValueOrThrowIfFailed();
            result.Error.Code.Should().Be(expectedError.Code);
            result.Error.Message.Should().Be(expectedError.Message);
        }

        [Fact]
        public async Task ExecuteWithChildActions_ShouldRunSequentially_WhenParallelIsFalse()
        {
            // Arrange
            var childActions = new List<MockAction>
            {
                this.CreateMockAction(),
                this.CreateMockAction(),
            };

            var groupAction = this.CreateGroupAction(childActions, isParallel: false);
            var actionData = groupAction.CreateActionData();

            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await groupAction.Execute(this.providerContext, actionData);
            stopwatch.Stop();

            // Assert
            result.IsSuccess.Should().BeTrue();
            stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo((childActions.Count - 1) * 1000);
        }

        private GroupAction CreateGroupAction(
            List<MockAction> childActions, bool isAsync = false, bool isParallel = false)
        {
            var groupAction = new GroupAction(
                "groupTest",
                "groupTest",
                string.Empty,
                isAsync,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                childActions,
                isParallel,
                this.actionRunner,
                this.mockJobClient,
                new TestClock());
            return groupAction;
        }

        private MockAction CreateMockAction(bool asynchronous = false)
        {
            var id = Guid.NewGuid();

            var dataProviderBuilder = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(
                "1", AutomationDeserializationConfiguration.ModelSettings);

            var serviceProvider = new Mock<IServiceProvider>().AddLoggers().Object;
            var dataProvider = dataProviderBuilder.Build(serviceProvider);
            var propertyNameProvider = new StaticProvider<Data<string>>("test");

            var mockAction = new MockAction(
                $"Mock Action {id}",
                $"mockAction{id}",
                $"Description for MockAction {id}",
                asynchronous,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                new TestClock());

            var setVariableAction = new SetVariableAction(
                $"SetVariable {id}",
                $"setVariable{id}",
                $"Description for SetVariable {id}",
                asynchronous,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                propertyNameProvider,
                dataProvider,
                new TestClock(),
                null,
                null,
                serviceProvider);

            return mockAction;
        }

        private GroupAction Setup(bool isAsync = false, bool childActionErrors = false, bool hasParentHandling = false, bool parallel = false)
        {
            var childActionList = new List<Action>
            {
                this.GetHttpRequestAction(),
            };

            if (childActionErrors)
            {
                childActionList.Add(this.GetRaiseErrorAction());
            }
            else
            {
                childActionList.Add(this.GetRaiseErrorActionWithHandling());
            }

            var onErrorActions = new List<Action>()
            {
                this.GetRaiseErrorActionWithHandling(),
            };

            var groupAction = new GroupAction(
                "groupTest",
                "groupTest",
                string.Empty,
                isAsync,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                hasParentHandling ? onErrorActions : Enumerable.Empty<Action>(),
                childActionList,
                parallel,
                this.actionRunner,
                this.mockJobClient,
                new TestClock());
            return groupAction;
        }

        private HttpRequestAction GetHttpRequestAction()
        {
            var url = new StaticProvider<Data<string>>($"https://app.ubind.com.au");
            var httpRequest = new HttpRequestConfiguration(
                url, new StaticProvider<Data<string>>("get"), Enumerable.Empty<HttpHeaderProvider>(), null, null, null, null);
            var httpAction = new HttpRequestAction(
                "httpActionTest",
                "httpActionTest",
                "test for http action",
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                httpRequest,
                new TestClock());
            return httpAction;
        }

        private RaiseErrorAction GetRaiseErrorAction()
        {
            var error = this.GetTestAutomationError();
            var raiseErrorAction = new RaiseErrorAction(
                "raiseErrorTest", "raiseErrorTestWithoutHandling", string.Empty, null, Enumerable.Empty<ErrorCondition>(), Enumerable.Empty<ErrorCondition>(), Enumerable.Empty<Action>(), error, new TestClock());
            return raiseErrorAction;
        }

        private RaiseErrorAction GetRaiseErrorActionWithHandling()
        {
            var error = this.GetTestAutomationError();
            var errorActionHandler = this.GetHttpRequestAction();
            var errorActionList = new List<Action>
            {
                errorActionHandler,
            };
            var raiseErrorAction = new RaiseErrorAction(
                "raiseErrorTest", "raiseErrorTestWithHandling", string.Empty, null, Enumerable.Empty<ErrorCondition>(), Enumerable.Empty<ErrorCondition>(), errorActionList, error, new TestClock());
            return raiseErrorAction;
        }

        private IProvider<ConfiguredError> GetTestAutomationError()
        {
            var error = new ErrorProviderConfigModel()
            {
                Code = new StaticBuilder<Data<string>> { Value = "test.error.for.test" },
                Title = new StaticBuilder<Data<string>> { Value = "Test Error" },
                Message = new StaticBuilder<Data<string>> { Value = "This is a test error" },
                HttpStatusCode = new StaticBuilder<Data<long>> { Value = 400 },
            };
            return error.Build(null);
        }
    }
}

// <copyright file="IterateActionTests.cs" company="uBind">
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
    using Flurl.Http.Testing;
    using Moq;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.PathLookup;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Conditions;
    using UBind.Application.Automation.Providers.Integer;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="IterateAction"/>.
    /// </summary>
    public class IterateActionTests
    {
        [Fact]
        public async Task IterateAction_ShouldExecuteActions_ForEachIterateItem()
        {
            // Arrange
            var httpTest = new HttpTest();
            httpTest.RespondWith("Ok", status: 200);
            var iterateAction = this.CreateAction();
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = iterateAction.CreateActionData();
            automationData.AddActionData(actionData);
            var providerContext = new ProviderContext(automationData);

            // Act
            using (httpTest)
            {
                await iterateAction.Execute(providerContext, actionData, true);
            }

            // Assert
            automationData.Actions.TryGetValue("testIterate", out ActionData data);
            var iterateData = data as IterateActionData;
            iterateData.Alias.Should().Be("testIterate");
            iterateData.LastIteration.Index.Should().Be(2);
            iterateData.LastIteration.Item.Should().Be("odette");
            iterateData.IterationsCompleted.Should().Be(3);
            iterateData.StartIndex.Should().Be(null);
            iterateData.EndIndex.Should().Be(null);
        }

        [Fact]
        public async Task IterateAction_ShouldIterateStartingFromAndEndingOn_TheGivenStartAndEndIndexIfAvailable()
        {
            // Arrange
            var httpTest = new HttpTest();
            httpTest.RespondWith("Ok", status: 200);
            var iterateAction = this.CreateAction(1, 2);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = iterateAction.CreateActionData();
            automationData.AddActionData(actionData);
            var providerContext = new ProviderContext(automationData);

            // Act
            using (httpTest)
            {
                await iterateAction.Execute(providerContext, actionData, true);
            }

            // Assert
            automationData.Actions.TryGetValue("testIterate", out ActionData data);
            var iterateData = data as IterateActionData;
            iterateData.IterationsCompleted.Should().Be(1);
            iterateData.LastIteration.Index.Should().Be(1);
            iterateData.LastIteration.Item.Should().Be("lake");
            iterateData.Reverse.Should().BeFalse();
        }

        [Fact]
        public async Task IterateAction_ShouldRunOnReverseOrder_IfStartIndexIsGreaterThanEndIndex()
        {
            // Arrange
            var httpTest = new HttpTest();
            httpTest.RespondWith("Ok", status: 200);
            var iterateAction = this.CreateAction(2, 1);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = iterateAction.CreateActionData();
            automationData.AddActionData(actionData);
            var providerContext = new ProviderContext(automationData);

            // Act
            using (httpTest)
            {
                await iterateAction.Execute(providerContext, actionData, true);
            }

            // Assert
            automationData.Actions.TryGetValue("testIterate", out ActionData data);
            var iterateData = data as IterateActionData;
            iterateData.IterationsCompleted.Should().Be(1);
            iterateData.LastIteration.Index.Should().Be(2);
            iterateData.LastIteration.Item.Should().Be("odette");
            iterateData.Reverse.Should().BeTrue();
        }

        [Fact]
        public async Task IterateAction_ShouldRunOnNormalOrder_EvenIfReverseIsTrueWhenEndIndexIsGreaterThanStartIndex()
        {
            // Arrange
            var httpTest = new HttpTest();
            httpTest.RespondWith("Ok", status: 200);
            var iterateAction = this.CreateAction(0, 1, true);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = iterateAction.CreateActionData();
            automationData.AddActionData(actionData);
            var providerContext = new ProviderContext(automationData);

            // Act
            using (httpTest)
            {
                await iterateAction.Execute(providerContext, actionData, true);
            }

            // Assert
            automationData.Actions.TryGetValue("testIterate", out ActionData data);
            var iterateData = data as IterateActionData;
            iterateData.IterationsCompleted.Should().Be(1);
            iterateData.LastIteration.Index.Should().Be(0);
            iterateData.LastIteration.Item.Should().Be("swan");
            iterateData.Reverse.Should().BeFalse();
        }

        [Fact]
        public async Task IterateAction_ShouldReverseExecutionOrder_WhenReverseIsTrue()
        {
            // Arrange
            var httpTest = new HttpTest();
            httpTest.RespondWith("Ok", status: 200);
            var iterateAction = this.CreateAction(reverse: true);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = iterateAction.CreateActionData();
            automationData.AddActionData(actionData);
            var providerContext = new ProviderContext(automationData);

            // Act
            using (httpTest)
            {
                await iterateAction.Execute(providerContext, actionData, true);
            }

            // Assert
            automationData.Actions.TryGetValue("testIterate", out ActionData data);
            var iterateData = data as IterateActionData;
            iterateData.IterationsCompleted.Should().Be(3);
            iterateData.LastIteration.Index.Should().Be(0);
            iterateData.LastIteration.Item.Should().Be("swan");
            iterateData.Reverse.Should().BeTrue();
        }

        [Fact]
        public async Task IterateAction_ShouldFollowDoWhileCondition_WhenConfigured()
        {
            // Arrange
            var httpTest = new HttpTest();
            httpTest.RespondWith("Ok", status: 200);
            var lookupPath = new FixedObjectPathLookup(
                new StaticProvider<Data<string>>("actions.testIterate.currentIteration.index"), "objectPathLookupText");
            var initialComparison = new PathLookupIntegerProvider(lookupPath, null, null, null, null, null, null, null);
            var forComparisonProvider = new StaticProvider<Data<long>>(1);
            var doWhileConditionProvider = new ComparisonCondition<long>(
                initialComparison, forComparisonProvider, (first, second) => first <= second, "testCondition");
            var iterateAction = this.CreateAction(endIndex: 1);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = iterateAction.CreateActionData();
            automationData.AddActionData(actionData);
            var providerContext = new ProviderContext(automationData);

            // Act
            using (httpTest)
            {
                await iterateAction.Execute(providerContext, actionData, true);
            }

            // Assert
            automationData.Actions.TryGetValue("testIterate", out ActionData data);
            var iterateData = data as IterateActionData;
            iterateData.IterationsCompleted.Should().Be(1);
            iterateData.LastIteration.Index.Should().Be(0);
            iterateData.LastIteration.Item.Should().Be("swan");
        }

        [Fact]
        public async Task IterateAction_ShouldSucceedCompletelyButNoIterations_WhenListHasNoItems()
        {
            // Arrange
            var httpTest = new HttpTest();
            httpTest.RespondWith("Ok", status: 200);
            var iterateAction = this.CreateAction(customList: Enumerable.Empty<object>().ToList());
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = iterateAction.CreateActionData();
            automationData.AddActionData(actionData);
            var providerContext = new ProviderContext(automationData);

            // Act
            using (httpTest)
            {
                await iterateAction.Execute(providerContext, actionData, true);
            }

            // Assert
            automationData.Actions.TryGetValue("testIterate", out ActionData data);
            var iterateData = data as IterateActionData;
            iterateData.IterationsCompleted.Should().Be(0);
            iterateData.LastIteration.Should().BeNull();
            iterateData.LastIteration.Should().BeNull();
        }

        private IterateAction CreateAction(
            int? startIndex = null, int? endIndex = null, bool? reverse = null, bool? doWhile = null, List<object> customList = null)
        {
            var numberList = customList ?? new List<object>() { "swan", "lake", "odette" };
            var listProvider =
                new StaticListProvider<object>(new GenericDataList<object>(numberList), null);
            var startIndexProvider = startIndex.HasValue ?
                new StaticProvider<Data<long>>(startIndex.Value) :
                null;
            var endIndexProvider = endIndex.HasValue ?
                new StaticProvider<Data<long>>(endIndex.Value) :
                null;
            var reverseProvider = reverse.HasValue ?
                new StaticProvider<Data<bool>>(reverse.Value) :
                null;
            var doWhileProvider = doWhile.HasValue ?
                new StaticProvider<Data<bool>>(doWhile.Value) :
                null;
            var actionsList = new List<Action>()
            {
                this.GetSampleAction(0),
                this.GetSampleAction(1),
            };
            var jobClientMock = new Mock<IJobClient>();
            var cachingResolver = new Mock<ICachingResolver>();
            var mockMediator = new Mock<ICqrsMediator>();
            var actionRunner = new ActionRunner(jobClientMock.Object, cachingResolver.Object, mockMediator.Object);
            var iterateAction = new IterateAction(
                "Iterate Action",
                "testIterate",
                string.Empty,
                false,
                null,
                null,
                null,
                null,
                listProvider,
                startIndexProvider,
                endIndexProvider,
                reverseProvider,
                doWhileProvider,
                actionsList,
                actionRunner,
                jobClientMock.Object,
                SystemClock.Instance);
            return iterateAction;
        }

        private HttpRequestAction GetSampleAction(int i)
        {
            var httpRequest = new HttpRequestConfiguration(
                new StaticProvider<Data<string>>("http://test.com"),
                new StaticProvider<Data<string>>("get"),
                Enumerable.Empty<HttpHeaderProvider>(),
                null,
                null,
                null,
                null);
            return new HttpRequestAction(
                $"Child Action{i}",
                $"childAction{i}",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                httpRequest,
                SystemClock.Instance);
        }
    }
}

// <copyright file="FixedObjectPathLookupTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.PathLookup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Flurl.Http.Testing;
    using Microsoft.Extensions.Logging;
    using MimeKit;
    using Moq;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Email;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.PathLookup;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Conditions;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Automation.Providers.Text;
    using UBind.Application.Services;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using Action = UBind.Application.Automation.Actions.Action;

    public class FixedObjectPathLookupTests
    {
        private Mock<ICachingResolver> mockCachingResolver = new Mock<ICachingResolver>();
        private Mock<ICqrsMediator> mockMediator = new Mock<ICqrsMediator>();

        [Fact]
        public async Task Resolve_ShouldReturnCorrectData_WhenPathUsedIsReferencePathAgainstCurrentAction()
        {
            // Arrange
            var path = new StaticProvider<Data<string>>("0/alias");
            var pathLookup = new FixedObjectPathLookup(path, "objectPathLookupText");
            var objectPathLookupText = new PathLookupTextProvider(pathLookup, null, null, null, null, null, null, null);
            var runCondition = new TextIsEqualToCondition(objectPathLookupText, new StaticProvider<Data<string>>("httpRequest"), new StaticProvider<Data<bool>>(true));
            var httpRequestConfig = new HttpRequestConfiguration(
                new StaticProvider<Data<string>>("http://test.com"),
                new StaticProvider<Data<string>>("get"),
                Enumerable.Empty<HttpHeaderProvider>(),
                null,
                null,
                null,
                null);
            var httpRequestAction = new HttpRequestAction("httpRequest", "httpRequest", string.Empty, false, runCondition, Enumerable.Empty<ErrorCondition>(), Enumerable.Empty<ErrorCondition>(), Enumerable.Empty<Action>(), httpRequestConfig, new TestClock());
            var actionRunner = new ActionRunner(new Mock<IJobClient>().Object, this.mockCachingResolver.Object, this.mockMediator.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var cleanActionData = httpRequestAction.CreateActionData();
            automationData.AddActionData(cleanActionData);

            // Act
            using (var test = new HttpTest())
            {
                await actionRunner.HandleAction(automationData, httpRequestAction, cleanActionData, CancellationToken.None);
            }

            // Assert
            automationData.Actions.Should().HaveCount(1);
            automationData.Actions.TryGetValue("httpRequest", out ActionData actionDatum);
            var httpActionData = actionDatum as HttpRequestActionData;

            httpActionData.State.Should().Be(ActionState.Completed);
        }

        [Theory]
        [InlineData("0/alias", "sendEmailActionChild")]
        [InlineData("3/currentIteration/item", "dos")]
        [InlineData("3/currentIteration/count", "1")]
        public async Task ResolveReferencePathOfChildOfIterateAction_ShouldReturnCorrectData_WhenPathUsedIsReferencePath(string path, string expectedValue)
        {
            // Arrange
            var pathProvider = new StaticProvider<Data<string>>(path);
            var pathLookup = new FixedObjectPathLookup(pathProvider, "objectPathLookupText");
            var objectPathLookupText = new PathLookupTextProvider(pathLookup, null, null, null, null, null, null, null);
            var sendEmailAction = this.GetChildAction(objectPathLookupText);

            var list = new List<string> { "uno", "dos" };
            var iterateAction = new IterateAction(
                "Parent Test Action",
                "parentAction",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                new StaticListProvider<object>(new GenericDataList<object>(list), null),
                null,
                null,
                null,
                null,
                new List<Action> { sendEmailAction },
                new ActionRunner(new Mock<IJobClient>().Object, this.mockCachingResolver.Object, this.mockMediator.Object),
                new Mock<IJobClient>().Object,
                new TestClock());

            var actionRunner = new ActionRunner(
                new Mock<IJobClient>().Object, this.mockCachingResolver.Object, this.mockMediator.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var cleanActionData = iterateAction.CreateActionData();
            automationData.AddActionData(cleanActionData);

            // Act
            await actionRunner.HandleAction(automationData, iterateAction, cleanActionData, CancellationToken.None);

            // Assert
            var iterationData = cleanActionData as IterateActionData;
            iterationData.LastIteration.Actions.TryGetValue(sendEmailAction.Alias, out ActionData childActionData);
            var emailActionData = childActionData as SendEmailActionData;
            emailActionData.Email.Subject.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("0/alias", "sendEmailActionChild")]
        [InlineData("2/alias", "parentGroupAction")]
        public async Task ResolveReferencePathOfChildOfGroupAction_ShouldReturnCorrectData_WhenPathUsedIsReferencePath(string path, string expectedValue)
        {
            // Arrange
            var pathProvider = new StaticProvider<Data<string>>(path);
            var pathLookup = new FixedObjectPathLookup(pathProvider, "objectPathLookupText");
            var objectPathLookupText = new PathLookupTextProvider(pathLookup, null, null, null, null, null, null, null);
            var sendEmailAction = this.GetChildAction(objectPathLookupText);

            var groupAction = new GroupAction(
                "Parent Group Action",
                "parentGroupAction",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                new List<Action> { sendEmailAction },
                false,
                new ActionRunner(new Mock<IJobClient>().Object, this.mockCachingResolver.Object, this.mockMediator.Object),
                new Mock<IJobClient>().Object,
                new TestClock());
            var actionRunner = new ActionRunner(
                new Mock<IJobClient>().Object, this.mockCachingResolver.Object, this.mockMediator.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var cleanActionData = groupAction.CreateActionData();
            automationData.AddActionData(cleanActionData);

            // Act
            await actionRunner.HandleAction(automationData, groupAction, cleanActionData, CancellationToken.None);

            // Assert
            var groupActionData = cleanActionData as GroupActionData;
            groupActionData.Actions.TryGetValue(sendEmailAction.Alias, out ActionData childData);
            var emailActionData = childData as SendEmailActionData;
            emailActionData.Email.Subject.Should().Be(expectedValue);
        }

        private SendEmailAction GetChildAction(IProvider<Data<string>> lookupPathProvider)
        {
            var messagingService = new Mock<IMessagingService>();
            messagingService.Setup(s => s.SendAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<MimeMessage>())).Verifiable();
            var cachingResolver = new Mock<ICachingResolver>();
            cachingResolver.Setup(t => t.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(new Tenant(TenantFactory.DefaultId)));
            cachingResolver.Setup(p => p.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(new Domain.Product.Product(TenantFactory.DefaultId, ProductFactory.DefaultId, "bar", "bar", SystemClock.Instance.GetCurrentInstant())));
            var emailConfiguration = new EmailConfiguration(
               new StaticProvider<Data<string>>("no-reply@tests.com"),
               Enumerable.Empty<IProvider<Data<string>>>(),
               new List<IProvider<Data<string>>> { new StaticProvider<Data<string>>("no-response@test.com") },
               Enumerable.Empty<IProvider<Data<string>>>(),
               Enumerable.Empty<IProvider<Data<string>>>(),
               lookupPathProvider,
               lookupPathProvider,
               null,
               null,
               null,
               null,
               Enumerable.Empty<IProvider<KeyValuePair<string, IEnumerable<string>>>>());
            var sendEmailAction = new SendEmailAction(
                messagingService.Object,
                "Test Action",
                "sendEmailActionChild",
                "Test Description",
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                null,
                emailConfiguration,
                Enumerable.Empty<IProvider<Data<string>>>(),
                Enumerable.Empty<RelationshipConfiguration>(),
                new Mock<IEmailService>().Object,
                cachingResolver.Object,
                new Mock<ILogger<SendEmailAction>>().Object,
                new TestClock());
            return sendEmailAction;
        }
    }
}

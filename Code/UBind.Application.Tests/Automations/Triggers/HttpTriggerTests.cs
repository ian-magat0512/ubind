// <copyright file="HttpTriggerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Triggers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Tests.Fakes;
    using UBind.Web.ResourceModels;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class HttpTriggerTests
    {
        [Fact]
        public async Task DoesMatch_ReturnsFalse_WhenAutomationDataDoesNotIncludeHttpRequest()
        {
            // Arrange
            var automationData = MockAutomationData.CreateWithEventTrigger();
            var sut = this.CreateTrigger();

            // Act
            var doesMatch = await sut.DoesMatch(automationData);

            // Assert
            doesMatch.Should().BeFalse();
        }

        [Fact]
        public async Task DoesMatch_ReturnsFalse_WhenAutomationDataDoesNotMatchHttpVerb()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var sut = this.CreateTrigger();

            // Act
            var doesMatch = await sut.DoesMatch(automationData);

            // Assert
            doesMatch.Should().BeFalse();
        }

        [Fact]
        public async Task DoesMatch_ReturnsFalse_WhenURLPathDoesNotHaveAllTheRequiredSegments()
        {
            // Arrange
            var endPoint = "testURLPathParameters";
            var triggerEndpointPathWithToken = "testURLPathParameters/{quoteId}";
            var context = new FakeHttpContextBuilder()
                .WithRequestPath($"/api/v1/tenant/carl/product/dev/environment/development/automations/{endPoint}")
                .Build();
            var model = new AutomationRequest(context);
            var serviceProvider = MockAutomationData.GetDefaultServiceProvider();
            var dataContext = AutomationData.CreateFromHttpRequest(
                TenantFactory.DefaultId,
                default,
                ProductFactory.DefaultId,
                Guid.NewGuid(),
                DeploymentEnvironment.Development,
                await model.ToTriggerRequest("secretcode"),
                serviceProvider);
            var sut = this.CreateTriggerWithToken(triggerEndpointPathWithToken);

            // Act
            var doesMatch = await sut.DoesMatch(dataContext);

            // Assert
            doesMatch.Should().BeFalse();
        }

        private HttpTrigger CreateTrigger()
        {
            return new HttpTrigger(
                "MyName",
                "MyAlias",
                "My description",
                null,
                new TriggerRequestEndpoint(
                    "example.com/test-trigger",
                    "POST",
                    Enumerable.Empty<ErrorCondition>()),
                new StaticProvider<Data<string>>("my context"),
                new HttpResponse(
                    new StaticProvider<Data<long>>(200),
                    Enumerable.Empty<IProvider<KeyValuePair<string, IEnumerable<string>>>>(),
                    new StaticProvider<Data<string>>("application/json"),
                    new TextContentProvider(new StaticProvider<Data<string>>("My content"))));
        }

        private HttpTrigger CreateTriggerWithToken(string path)
        {
            return new HttpTrigger(
                "MyName",
                "MyAlias",
                "My description",
                null,
                new TriggerRequestEndpoint(
                    path,
                    "GET",
                    Enumerable.Empty<ErrorCondition>()),
                new StaticProvider<Data<string>>("my context"),
                new HttpResponse(
                    new StaticProvider<Data<long>>(200),
                    Enumerable.Empty<IProvider<KeyValuePair<string, IEnumerable<string>>>>(),
                    new StaticProvider<Data<string>>("application/json"),
                    new TextContentProvider(new StaticProvider<Data<string>>("My content"))));
        }
    }
}

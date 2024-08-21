// <copyright file="HttpRequestActionIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Actions
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Flurl.Http.Testing;
    using Namotion.Reflection;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.PathLookup;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Text;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Unit tests for the different automation actions.
    /// </summary>
    public class HttpRequestActionIntegrationTests : IAsyncLifetime
    {
        private HttpRequestAction httpAction;
        private AutomationData mockData;
        private ProviderContext providerContext;

        public async Task InitializeAsync()
        {
            var url = new StaticProvider<Data<string>>("http://foo-bar.com");
            var httpRequest = new HttpRequestConfiguration(
                url, new StaticProvider<Data<string>>("get"), Enumerable.Empty<HttpHeaderProvider>(), null, null, null, null);
            this.httpAction = new HttpRequestAction(
                "actionTest", "actionTest", "test for action", false, null, Enumerable.Empty<ErrorCondition>(), Enumerable.Empty<ErrorCondition>(), Enumerable.Empty<Action>(), httpRequest, new TestClock());
            this.mockData = await MockAutomationData.CreateWithHttpTrigger();
            this.providerContext = new ProviderContext(this.mockData);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task ExecuteAction_ShouldRunSuccessfully_GivenCompleteConfiguration()
        {
            // Arrange
            var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Fooz = "foo" }, 200);
            var actionData = this.httpAction.CreateActionData();
            this.mockData.AddActionData(actionData);

            // Act
            using (httpTest)
            {
                await this.httpAction.Execute(this.providerContext, actionData);
            }

            // Assert
            httpTest.Dispose();

            var actionDataResponse = actionData.TryGetPropertyValue<Response>("HttpResponse");
            string responseContent = actionDataResponse.Content.ToString();
            responseContent.Should().Be(JToken.Parse("{\"Fooz\":\"foo\"}").ToString());
        }

        [Fact]
        public async Task HandleExecution_ShouldReturnAFailingStatus_WhenAnErrorResponseIsReceived()
        {
            // Arrange
            var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { message = "Missing parameter" }, 500);
            var actionData = this.httpAction.CreateActionData();
            this.mockData.AddActionData(actionData);

            // Act
            using (httpTest)
            {
                await this.httpAction.Execute(this.providerContext, actionData);
            }

            // Assert
            httpTest.Dispose();

            var actionDataResponse = actionData.TryGetPropertyValue<Response>("HttpResponse");
            string responseContent = actionDataResponse.Content.ToString();
            responseContent.Should().Be(JToken.Parse("{\"message\":\"Missing parameter\"}").ToString());
        }

        [Fact]
        public async Task JsonData_ShouldParseSuccessfully_AndAllowPathLookups()
        {
            // Arrange
            var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { fooz = new { baz = "foo", jane = "doe" } }, 200);
            var actionData = this.httpAction.CreateActionData();
            this.mockData.AddActionData(actionData);
            using (httpTest)
            {
                await this.httpAction.Execute(this.providerContext, actionData);
            }

            httpTest.Dispose();
            var staticPathProvider = new StaticProvider<Data<string>>(
                new Data<string>("/actions/actionTest/httpResponse/content/fooz/jane"));
            IObjectPathLookupProvider path = new FixedObjectPathLookup(staticPathProvider, "objectPathLookupText");
            var pathLookupTextProvider = new PathLookupTextProvider(path, null, null, null, null, null, null, null);

            // Act
            var result = (await pathLookupTextProvider.Resolve(this.providerContext)).GetValueOrThrowIfFailed();

            // Assert
            result.DataValue.Should().Be("doe");
        }
    }
}

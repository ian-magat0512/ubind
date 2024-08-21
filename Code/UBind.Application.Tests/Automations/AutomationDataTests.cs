// <copyright file="AutomationDataTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1615 // Element return value should be documented

namespace UBind.Application.Tests.Automations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.SystemEvents.Payload;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using UBind.Domain.Tests.Fakes;
    using UBind.Web.ResourceModels;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class AutomationDataTests
    {
        [Fact]
        public async Task Request_CanBeInstantiatedCorrectly_FromAnHTTPRequest()
        {
            // Arrange
            var context = new FakeHttpContextBuilder()
                .WithRequestPath(@"/api/v1/tenant/carl/product/dev/environment/development/automations/addressMatch")
                .WithQueryString("?address=34 Malibu Point")
                .Build();
            var model = new AutomationRequest(context);
            var serviceProvider = MockAutomationData.GetDefaultServiceProvider();

            // Act
            var triggerRequest = await model.ToTriggerRequest("secretcode");
            triggerRequest.DetectPathParameters("addressMatch'");
            var dataContext = AutomationData.CreateFromHttpRequest(
                TenantFactory.DefaultId,
                default,
                ProductFactory.DefaultId,
                Guid.NewGuid(),
                DeploymentEnvironment.Development,
                triggerRequest,
                serviceProvider);

            // Assert
            dataContext.Should().NotBeNull();
            var actionPath = await dataContext.GetValue("/trigger/httpRequest/actionPath");
            var address = await dataContext.GetValue("/trigger/httpRequest/getParameters/address");
            actionPath.ToString().Should().Be("addressMatch");
            address.ToString().Should().Be("34 Malibu Point");
        }

        /// <summary>
        /// Should create valid automation data context.
        /// </summary>
        [Fact]
        public async Task Controller_ShouldCreateAutomationDataContextWithoutQueryParameters_FromHttpRequest()
        {
            // Arrange
            var context = new FakeHttpContextBuilder()
                .WithRequestPath(@"/api/v1/tenant/carl/product/dev/environment/development/automations/addressMatch")
                .WithQueryString("?address=34 Malibu Point")
                .Build();
            var model = new AutomationRequest(context);
            var serviceProvider = MockAutomationData.GetDefaultServiceProvider();

            // Act
            var triggerRequest = await model.ToTriggerRequest("secretcode");
            triggerRequest.DetectPathParameters("addressMatch'");
            var dataContext = AutomationData.CreateFromHttpRequest(
                TenantFactory.DefaultId,
                default,
                ProductFactory.DefaultId,
                Guid.NewGuid(),
                DeploymentEnvironment.Development,
                triggerRequest,
                serviceProvider);

            // Assert
            var content = await dataContext.GetValue("/trigger/httpRequest/content");

            dataContext.Should().NotBeNull();
            content?.ToString().Should().BeNullOrEmpty();
            var actionPath = await dataContext.GetValue("/trigger/httpRequest/actionPath");
            actionPath.ToString().Should().Be("addressMatch");
        }

        /// <summary>
        /// Should create valid automation data context.
        /// </summary>
        [Fact]
        public async Task Controller_ShouldCreateAutomationDataContextWithAllData_FromSystemEventRequest()
        {
            // Arrange
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var productContext = new ProductContext(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DeploymentEnvironment.Development);
            var organisationId = Guid.NewGuid();
            var systemEvent = SystemEvent.CreateWithPayload(
                productContext.TenantId,
                organisationId,
                productContext.ProductId,
                productContext.Environment,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload,
                default);

            // Act
            var dataContext = AutomationData.CreateFromSystemEvent(systemEvent, null, MockAutomationData.GetDefaultServiceProvider());

            // Assert
            var eventType = await dataContext.GetValue("/trigger/eventType");
            var eventData = await dataContext.GetValue("/trigger/eventData");
            var eventContextOrganisationId = ((IEntity)dataContext.Context["organisation"]).Id;

            dataContext.Should().NotBeNull();
            eventType.ToString().Should().Be("customerExpiredQuoteOpened");
            eventData.ToString().Should().Be(JObject.Parse(systemEvent.PayloadJson).ToString());
            eventContextOrganisationId.Should().Be(organisationId.ToString());
        }

        /// <summary>
        /// Should create valid automation data context.
        /// </summary>
        [Fact]
        public async Task Controller_ShouldCreateAutomationDataContextWithAllData_FromSystemEventRequest_WithoutProduct()
        {
            // Arrange
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var organisationId = Guid.NewGuid();
            var systemEvent = SystemEvent.CreateWithPayload(
                Guid.NewGuid(),
                organisationId,
                DeploymentEnvironment.Development,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload,
                default);

            // Act
            var dataContext = AutomationData.CreateFromSystemEvent(systemEvent, null, MockAutomationData.GetDefaultServiceProvider());

            // Assert
            var eventType = await dataContext.GetValue("/trigger/eventType");
            var eventData = await dataContext.GetValue("/trigger/eventData");
            var eventContextOrganisationId = ((IEntity)dataContext.Context["organisation"]).Id;

            dataContext.Should().NotBeNull();
            eventType.ToString().Should().Be("customerExpiredQuoteOpened");
            eventData.ToString().Should().Be(JObject.Parse(systemEvent.PayloadJson).ToString());
            eventContextOrganisationId.Should().Be(organisationId.ToString());
        }

        [Fact]
        public async Task Request_ActionPathSegments_ShouldHaveCorrectPathSegmentsCount_FromHttpRequestTrigger()
        {
            // Arrange
            var expectedSegmentCount = 4;
            var context = new FakeHttpContextBuilder()
                .WithRequestPath($"/api/v1/tenant/carl/product/dev/environment/development/automations/segment0/segment1/segment2/segment3")
                .Build();
            var model = new AutomationRequest(context);
            var serviceProvider = MockAutomationData.GetDefaultServiceProvider();

            // Act
            var triggerRequest = await model.ToTriggerRequest("secretcode");
            triggerRequest.DetectPathParameters("addressMatch'");
            var dataContext = AutomationData.CreateFromHttpRequest(
                TenantFactory.DefaultId,
                default,
                ProductFactory.DefaultId,
                Guid.NewGuid(),
                DeploymentEnvironment.Development,
                triggerRequest,
                serviceProvider);

            // Assert
            var pathSegment = await dataContext.GetValue("/trigger/httpRequest/actionPathSegments");
            (pathSegment as string[]).Length.Should().Be(expectedSegmentCount);
        }

        [Fact]
        public async Task Request_ActionPathSegments_ShouldReturnCorrectValueOnTheGivenIndex_FromHttpRequestTrigger()
        {
            // Arrange
            var segment0 = "segment0";
            var segment1 = "segment1";
            var segment2 = "segment2";
            var segment3 = "segment3";
            var context = new FakeHttpContextBuilder()
                .WithRequestPath($"/api/v1/tenant/carl/product/dev/environment/development/automations/{segment0}/{segment1}/{segment2}/{segment3}")
                .Build();
            var model = new AutomationRequest(context);
            var serviceProvider = MockAutomationData.GetDefaultServiceProvider();

            // Act
            var triggerRequest = await model.ToTriggerRequest("secretcode");
            var dataContext = AutomationData.CreateFromHttpRequest(
                TenantFactory.DefaultId,
                default,
                ProductFactory.DefaultId,
                Guid.NewGuid(),
                DeploymentEnvironment.Development,
                triggerRequest,
                serviceProvider);

            // Assert
            var pathSegment0 = await dataContext.GetValue("/trigger/httpRequest/actionPathSegments/0");
            pathSegment0.ToString().Should().Be(segment0);
            var pathSegment1 = await dataContext.GetValue("/trigger/httpRequest/actionPathSegments/1");
            pathSegment1.ToString().Should().Be(segment1);
            var pathSegment2 = await dataContext.GetValue("/trigger/httpRequest/actionPathSegments/2");
            pathSegment2.ToString().Should().Be(segment2);
            var pathSegment3 = await dataContext.GetValue("/trigger/httpRequest/actionPathSegments/3");
            pathSegment3.ToString().Should().Be(segment3);
        }

        [Fact]
        public async Task Request_PathParameters_ShouldBeEmpty_WhenNoParameterInTheTriggerEndpointPath()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var endPoint = "customer";
            var context = new FakeHttpContextBuilder()
                .WithRequestPath($"/api/v1/tenant/carl/product/dev/environment/development/automations/{endPoint}/customer/{customerId}")
                .Build();
            var model = new AutomationRequest(context);
            var serviceProvider = MockAutomationData.GetDefaultServiceProvider();

            // Act
            var triggerRequest = await model.ToTriggerRequest("secretcode");
            triggerRequest.DetectPathParameters("customer");
            var dataContext = AutomationData.CreateFromHttpRequest(
                TenantFactory.DefaultId,
                default,
                ProductFactory.DefaultId,
                Guid.NewGuid(),
                DeploymentEnvironment.Development,
                triggerRequest,
                serviceProvider);

            // Assert
            Dictionary<string, string> value = (await dataContext.GetValue("/trigger/httpRequest/pathParameters")) as Dictionary<string, string>;
            value.Should().BeEmpty();
        }

        [Fact]
        public async Task Request_PathParameters_ShouldContainCorrectValueOfTheGivenToken_FromHttpRequestTrigger()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var endPoint = "entity-lookup";
            var entityTypeTokenValue = "customer";
            var triggerEndpointPathWithToken = "entity-lookup/{entityType}/{entityId}";
            var triggerEndpoint = new TriggerRequestEndpoint(
                triggerEndpointPathWithToken, "GET", Enumerable.Empty<ErrorCondition>());
            var context = new FakeHttpContextBuilder()
                .WithRequestPath($"/api/v1/tenant/carl/product/dev/environment/development/automations/{endPoint}/{entityTypeTokenValue}/{customerId}")
                .Build();
            var model = new AutomationRequest(context);
            var serviceProvider = MockAutomationData.GetDefaultServiceProvider();

            // Act
            var triggerRequest = await model.ToTriggerRequest("secretcode");
            triggerRequest.DetectPathParameters(triggerEndpointPathWithToken);
            var dataContext = AutomationData.CreateFromHttpRequest(
                TenantFactory.DefaultId,
                default,
                ProductFactory.DefaultId,
                Guid.NewGuid(),
                DeploymentEnvironment.Development,
                triggerRequest,
                serviceProvider);

            // Assert
            var entityType = await dataContext.GetValue("/trigger/httpRequest/pathParameters/entityType");
            entityType.Should().Be(entityTypeTokenValue);
            var entityId = await dataContext.GetValue("/trigger/httpRequest/pathParameters/entityId");
            entityId.Should().Be(customerId.ToString());
        }

        [Fact]
        public async Task Request_PathParameters_ShouldContainSingleValueOfTheGivenToken_FromHttpRequestTrigger()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var endPoint = "entity-lookup";
            var entityTypeTokenValue = "customer";
            var triggerEndpointPathWithToken = "entity-lookup/{entityType}/{entityId}";
            var triggerEndpoint = new TriggerRequestEndpoint(
                triggerEndpointPathWithToken, "GET", Enumerable.Empty<ErrorCondition>());
            var context = new FakeHttpContextBuilder()
                .WithRequestPath($"/api/v1/tenant/carl/product/dev/environment/development/automations/{endPoint}/{entityTypeTokenValue}/{customerId}")
                .Build();
            var model = new AutomationRequest(context);
            var serviceProvider = MockAutomationData.GetDefaultServiceProvider();

            // Act
            var triggerRequest = await model.ToTriggerRequest("secretcode");
            triggerRequest.DetectPathParameters(triggerEndpointPathWithToken);
            var dataContext = AutomationData.CreateFromHttpRequest(
                TenantFactory.DefaultId,
                default,
                ProductFactory.DefaultId,
                Guid.NewGuid(),
                DeploymentEnvironment.Development,
                triggerRequest,
                serviceProvider);

            // Assert
            var pathParameterObject = (await dataContext.GetValue("/trigger/httpRequest/pathParameters/"))
                .As<Dictionary<string, string>>();

            pathParameterObject.GetValueOrDefault("entityType").Should().Be(entityTypeTokenValue);
            pathParameterObject.GetValueOrDefault("entityId").Should().Be(customerId.ToString());
        }

        /// <summary>
        /// This test is to ensure that the AutomationData can be serialized and deserialized successfully/properly.
        /// Async actions use serialized automation data to pass to hangfire so we need to make sure
        /// we don't break this functionality.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AutomationData_Serialization_ShouldDeserializeProperly()
        {
            // Arrange
            var context = new FakeHttpContextBuilder()
                .WithRequestPath(@"/api/v1/tenant/carl/product/dev/environment/development/automations/addressMatch")
                .WithQueryString("?address=Ligas Malolos City")
                .Build();
            var model = new AutomationRequest(context);
            var serviceProvider = MockAutomationData.GetDefaultServiceProvider();

            // Act
            var triggerRequest = await model.ToTriggerRequest("secretcode");
            triggerRequest.DetectPathParameters("addressMatch'");
            var dataContext = AutomationData.CreateFromHttpRequest(
                TenantFactory.DefaultId,
                default,
                ProductFactory.DefaultId,
                Guid.NewGuid(),
                DeploymentEnvironment.Development,
                triggerRequest,
                serviceProvider);

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                DateParseHandling = DateParseHandling.None,
            };
            var serializedAutomationData = JsonConvert.SerializeObject(dataContext, jsonSerializerSettings);
            var deserializedContext = JsonConvert.DeserializeObject<AutomationData>(
                serializedAutomationData, AutomationDeserializationConfiguration.DataSettings);

            // Assert
            deserializedContext.Should().NotBeNull();
            var origRequest = ((HttpTriggerData)dataContext.Trigger).HttpRequest;
            var deserializedRequest = ((HttpTriggerData)deserializedContext.Trigger).HttpRequest;

            deserializedRequest.Url.Should().Be(origRequest.Url);
            deserializedRequest.HttpVerb.Should().Be(origRequest.HttpVerb);
            deserializedRequest.Path.Should().Be(origRequest.Path);
            deserializedRequest.ActionPath.Should().Be(origRequest.ActionPath);
            deserializedRequest.QueryString.Should().Be(origRequest.QueryString);
            for (var i = 0; i < deserializedRequest.PathSegments.Length; i++)
            {
                deserializedRequest.PathSegments[i].Should().Be(origRequest.PathSegments[i]);
            }
        }

        private HttpTrigger CreateTriggerWithPathParameter(TriggerRequestEndpoint triggerRequestEndpoint)
        {
            return new HttpTrigger(
                "MyName",
                "MyAlias",
                "My description",
                null,
                new TriggerRequestEndpoint(
                    "entity-lookup/{entityType}/{entityId}",
                    "GET",
                    Enumerable.Empty<ErrorCondition>()),
                new StaticProvider<Data<string>>("my context"),
                new HttpResponse(
                    new StaticProvider<Data<long>>(200),
                    Enumerable.Empty<IProvider<KeyValuePair<string, IEnumerable<string>>>>(),
                    new StaticProvider<Data<string>>("application/json"),
                    new TextContentProvider(new StaticProvider<Data<string>>("My content"))));
        }

        private HttpTrigger CreateTriggerWithNoPathParameter()
        {
            return new HttpTrigger(
                "MyName",
                "MyAlias",
                "My description",
                null,
                new TriggerRequestEndpoint(
                    "entity-lookup",
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

// <copyright file="MaintenanceControllerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers.Quoter
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NodaTime;
    using UBind.Application.Authorisation;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Aggregates;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels.Quote;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="MaintenanceControllerTests" />.
    /// </summary>
    public class MaintenanceControllerTests
    {
        private MaintenanceController maintenanceController;
        private Mock<IAuthorisationService> authorisationService;
        private Mock<IQuoteAggregateResolverService> quoteAggregateResolverService;
        private Mock<IQuoteEventRepository> quoteEventRepository;
        private Mock<ICachingResolver> cachingResolver;
        private Mock<ICqrsMediator> mediator;
        private Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolver;
        private Mock<ILogger<MaintenanceController>> loggerMock;
        private Guid aggregateId = new Guid("12729fbb-400f-40d0-a67f-4b057746d22a");
        private Guid tenantId = new Guid("3DF7E5BE-E242-4DF4-B7CF-179776FF88EB");

        /// <summary>
        /// Defines the quoteInitializedEventJson.
        /// </summary>
        private string quoteInitializedEventJson =
            @"{""$type"":""UBind.Domain.Aggregates.Quote.QuoteAggregate+QuoteInitializedEvent, UBind.Domain"",""QuoteId"":""12729fbb-400f-40d0-a67f-4b057746d22a"",""TenantId"":""carl"",""ProductId"":""dev"",""Environment"":1,""Type"":0,""IsTestData"":false,""AggregateId"":""12729fbb-400f-40d0-a67f-4b057746d22a"",""CreatedTimestamp"":{""$type"":""NodaTime.Instant, NodaTime"",""days"":18373,""nanoOfDay"":12798805425300}}";

        /// <summary>
        /// Defines the workflowStepEventJson.
        /// </summary>
        private string workflowStepEventJson =
            @"{""$type"":""UBind.Domain.Aggregates.Quote.QuoteAggregate+WorkflowStepAssignedEvent, UBind.Domain"",""QuoteId"":""12729fbb-400f-40d0-a67f-4b057746d22a"",""WorkflowStep"":""purchaseDetails"",""AggregateId"":""12729fbb-400f-40d0-a67f-4b057746d22a"",""CreatedTimestamp"":{""$type"":""NodaTime.Instant, NodaTime"",""days"":18373,""nanoOfDay"":13022592364700}}";

        public MaintenanceControllerTests()
        {
            this.quoteEventRepository = new Mock<IQuoteEventRepository>();
            this.authorisationService = new Mock<IAuthorisationService>();
            this.quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
            this.quoteAggregateResolverService.Setup(x => x.GetQuoteAggregateIdForQuoteId(this.aggregateId)).Throws(new ErrorException(Errors.General.NotFound("quote", this.aggregateId)));
            this.quoteAggregateResolverService.Setup(x => x.GetQuoteAggregateIdForPolicyId(this.aggregateId)).Returns(this.aggregateId);
            this.quoteAggregateResolverService.Setup(x => x.GetQuoteAggregateIdForQuoteIdOrPolicyId(this.aggregateId)).Returns(this.aggregateId);
            this.cachingResolver = new Mock<ICachingResolver>();
            this.mediator = new Mock<ICqrsMediator>();
            this.httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
            this.loggerMock = new Mock<ILogger<MaintenanceController>>();
            this.maintenanceController = new MaintenanceController(
                this.authorisationService.Object,
                this.quoteAggregateResolverService.Object,
                this.quoteEventRepository.Object,
                this.cachingResolver.Object,
                this.mediator.Object,
                this.loggerMock.Object);
        }

        /// <summary>
        /// The GetAggregateEvent_InitializedEvent_MustNotHaveWorkflowStep.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetAggregateEvent_InitializedEvent_MustNotHaveWorkflowStep()
        {
            // Arrange
            var eventRecord = new EventRecordWithGuidId(this.tenantId, this.aggregateId, 1, this.quoteInitializedEventJson, AggregateType.AdditionalPropertyDefinition, SystemClock.Instance.GetCurrentInstant());
            var collection = QuoteEventSummary.CreateFromEvents(eventRecord);
            this.quoteEventRepository.Setup(rec => rec.GetEventSummaries(this.aggregateId))
                .Returns(collection);

            // Act
            var response = await this.maintenanceController.GetQuoteAggregateEvents(
                this.aggregateId, TenantFactory.DefaultId.ToString()) as OkObjectResult;
            var eventSummaries = response.Value as QuoteEventSummarySetModel;
            var firstEvent = eventSummaries.Events.FirstOrDefault();

            // Assert
            Assert.Equal(1, firstEvent.SequenceNumber);
            Assert.Equal(ApplicationEventType.QuoteCreated.ToString(), firstEvent.EventType);
            Assert.Null(firstEvent.WorkflowStep);
        }

        /// <summary>
        /// The GetAggregateEvet_WorkflowStepEvent_MustIncludeWorkflowStep.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetAggregateEvet_WorkflowStepEvent_MustIncludeWorkflowStep()
        {
            // Arrange
            var eventRecord = new EventRecordWithGuidId(this.tenantId, this.aggregateId, 7, this.workflowStepEventJson, AggregateType.AdditionalPropertyDefinition, SystemClock.Instance.GetCurrentInstant());
            var collection = QuoteEventSummary.CreateFromEvents(eventRecord);
            this.quoteEventRepository.Setup(rec => rec.GetEventSummaries(this.aggregateId))
                .Returns(collection);

            // Act
            var response = await this.maintenanceController.GetQuoteAggregateEvents(
                this.aggregateId, TenantFactory.DefaultId.ToString()) as OkObjectResult;
            var eventSummaries = response.Value as QuoteEventSummarySetModel;
            var workflowEvent = eventSummaries.Events.FirstOrDefault();

            // Assert
            Assert.True(workflowEvent != null);
            Assert.Equal("purchaseDetails", workflowEvent.WorkflowStep);
            Assert.Equal(ApplicationEventType.WorkflowStepChanged.ToString(), workflowEvent.EventType);
        }
    }
}

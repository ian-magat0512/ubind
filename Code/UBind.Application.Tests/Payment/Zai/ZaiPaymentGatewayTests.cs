// <copyright file="ZaiPaymentGatewayTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Payment.Zai
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Payment.Zai;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Payment;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class ZaiPaymentGatewayTests
    {
        private readonly Guid performingUserId = Guid.NewGuid();

        private readonly PriceBreakdown sampleCalculationBreakdown = PriceBreakdown.CreateFromCalculationFormatV2(
            new Domain.CalculatedPriceComponents(100, 10, 20, 20, 0, 10, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 10, 0, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13.5m, 0.16m, 0.35m, 0, 20m, 140m, 10.16m, 174.01m, "AUD"));

        private readonly Domain.Aggregates.Quote.Quote quote;

        private readonly CustomerAggregate customer;

        private readonly Guid organisationID;

        private readonly int currentYearPlus1;

        private ZaiPaymentGateway stub;

        public ZaiPaymentGatewayTests()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.organisationID = Guid.NewGuid();
            const DeploymentEnvironment environment = DeploymentEnvironment.Staging;
            var clock = SystemClock.Instance;
            var workflowProvider = new DefaultQuoteWorkflowProvider();
            var quoteExpirySettingProvider = new DefaultExpirySettingsProvider();
            var performingUserResolver = new Mock<IHttpContextPropertiesResolver>();
            performingUserResolver.Setup(x => x.ClientIpAddress).Returns(new IPAddress(1923142512));
            IPersonalDetails customerDetails = new FakePersonalDetails();
            this.quote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                this.organisationID,
                ProductFactory.DefaultId,
                environment,
                QuoteExpirySettings.Default,
                this.performingUserId,
                clock.Now(),
                Guid.NewGuid());
            var personAggregate = PersonAggregate.CreatePerson(
                TenantFactory.DefaultId,
                this.organisationID,
                this.performingUserId,
                clock.Now());
            this.customer = CustomerAggregate.CreateNewCustomer(
                TenantFactory.DefaultId,
                personAggregate,
                environment,
                this.performingUserId,
                null,
                clock.Now());
            this.quote.Aggregate.RecordAssociationWithCustomer(this.customer, customerDetails, this.performingUserId, clock.Now());
            this.quote.AssignQuoteNumber("TESTQ0001", default, clock.Now());
            this.quote.Actualise(default, clock.Now(), new DefaultQuoteWorkflow());
            var customerDetailsUpdate = new QuoteDataUpdate<IPersonalDetails>(
                Guid.NewGuid(), customerDetails, clock.Now());
            this.quote.RecordCustomerDetailsUpdate(customerDetailsUpdate);

            var currentYear = SystemClock.Instance.Now().ToDateTimeUtc().Year;
            this.currentYearPlus1 = currentYear + 1;

            var zaiPaymentConfiguration = new TestZaiConfiguration();
            var accessTokenProvider = new ZaiAccessTokenProvider(zaiPaymentConfiguration, clock);

            var cachingResolver = new Moq.Mock<ICachingResolver>();
            cachingResolver.Setup(x => x.GetOrganisationOrThrow(TenantFactory.DefaultId, this.organisationID))
                .Returns(Task.FromResult(new Domain.ReadModel.OrganisationReadModel(TenantFactory.DefaultId, this.organisationID, "defaultOrg5", "DefaultOrganisation", null, true, false, clock.Now())));
            this.stub = new ZaiPaymentGateway(
                zaiPaymentConfiguration,
                accessTokenProvider,
                cachingResolver.Object,
                performingUserResolver.Object,
                new Mock<ICqrsMediator>().Object);
        }

        [Fact(Skip = "This will be fixed in UB-9415")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_ShouldSucceed_IfAggregateAndBreakdownAreComplete()
        {
            // Arrange
            var cardDetails = new CreditCardDetails(
                "4444333322221111",
                "John Smith",
                "11",
                int.Parse(this.currentYearPlus1.ToString().Substring(2)),
                "123");

            // Act
            var response = await this.stub.MakePayment(this.quote, this.sampleCalculationBreakdown, cardDetails, "foo");

            // Assert
            response.Success.Should().BeTrue();
        }

        [Fact(Skip = "This will be fixed in UB-9415")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_ShouldUseSavedPaymentMethod_IfIdPassed()
        {
            // Arrange
            IPersonalDetails customerDetails = new FakePersonalDetails();
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                this.organisationID,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                QuoteExpirySettings.Default,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant(),
                Guid.NewGuid());
            quote.Aggregate.RecordAssociationWithCustomer(this.customer, customerDetails, this.performingUserId, SystemClock.Instance.Now());
            quote.Aggregate.WithCustomerDetails(quote.Id, customerDetails);
            quote.AssignQuoteNumber("TESTQ002", this.performingUserId, SystemClock.Instance.Now());
            var cardDetails = new CreditCardDetails(
                "4444333322221111",
                "John Smith",
                "11",
                int.Parse(this.currentYearPlus1.ToString().Substring(2)),
                "123");
            var initialPayment = await this.stub.MakePayment(quote, this.sampleCalculationBreakdown, cardDetails, "foo");
            if (initialPayment != null && initialPayment.Success)
            {
                var paymentDetailsJson = JObject.Parse(initialPayment.Details.Request);
                var maskedCardDetails = new MaskedCreditCardDetails(
                    "**** **** **111111",
                    "John Smith",
                    11,
                    int.Parse(this.currentYearPlus1.ToString().Substring(2)),
                    "visa");
                var existingZaiRecordData = new JObject()
            {
                { "userAccountId", paymentDetailsJson["userAccountId"]?.ToString() },
                { "cardAccountId",  paymentDetailsJson["cardAccountId"]?.ToString() },
            };
                var savedPaymentMethod = new SavedPaymentMethod(
                    TenantFactory.DefaultId,
                    this.quote.Aggregate.CustomerId.Value,
                    PaymentMethod.GetZaiPaymentMethod().Id,
                    new LocalDate(this.currentYearPlus1, 10, 11),
                    JsonConvert.SerializeObject(maskedCardDetails),
                    JsonConvert.SerializeObject(existingZaiRecordData),
                    SystemClock.Instance.Now());
                var renewalQuote = quote
                    .Aggregate
                    .WithCalculationResult(quote.Id)
                    .WithPolicy(quote.Id, "TESTPOL001")
                    .WithRenewalQuote();

                // Act
                var response = await this.stub.MakePayment(renewalQuote, this.sampleCalculationBreakdown, savedPaymentMethod);

                // Assert
                response.Success.Should().BeTrue();
            }

            initialPayment.Success.Should().BeTrue();
        }
    }
}

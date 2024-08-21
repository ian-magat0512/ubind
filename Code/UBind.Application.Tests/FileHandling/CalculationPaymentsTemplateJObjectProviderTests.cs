// <copyright file="CalculationPaymentsTemplateJObjectProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.FileHandling
{
    using System;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.FileHandling.Template_Provider;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Events;
    using UBind.Domain.Json;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class CalculationPaymentsTemplateJObjectProviderTests
    {
        private readonly IClock clock = SystemClock.Instance;
        private readonly Guid? performingUserId = Guid.NewGuid();

        [Fact]
        public void JsonObject_IncludesPayableComponentsTotalPayable()
        {
            Quote quote = this.CreateQuoteFromCalculationResultJson(
                CalculationResultJsonFactory.SampleWithSoftReferral);
            var eventType = QuoteEventTypeMap.Map(quote.Aggregate.UnsavedEvents.Last());
            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                eventType.First(),
                quote.Aggregate,
                quote.Id,
                0,
                "1",
                quote.ProductReleaseId.Value);
            CalculationPaymentsTemplateJObjectProvider provider = new CalculationPaymentsTemplateJObjectProvider();
            provider.CreateJsonObject(applicationEvent);

            // Act
            var payable = provider.JsonObject["PayableComponentsTotalPayable"];

            // Assert
            Assert.Equal("$121.00", payable);
        }

        [Fact]
        public void JsonObject_IncludesRefundComponentsTotalPayable()
        {
            Quote quote = this.CreateQuoteFromCalculationResultJson(
                CalculationResultJsonFactory.SampleWithSoftReferral);
            var eventType = QuoteEventTypeMap.Map(quote.Aggregate.UnsavedEvents.Last());
            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                eventType.First(),
                quote.Aggregate,
                quote.Id,
                0,
                "1",
                quote.ProductReleaseId.Value);
            CalculationPaymentsTemplateJObjectProvider provider = new CalculationPaymentsTemplateJObjectProvider();
            provider.CreateJsonObject(applicationEvent);

            // Act
            var refund = provider.JsonObject.SelectToken("RefundComponentsTotalPayable");

            // Assert
            Assert.Equal("$0.00", refund);
        }

        private Quote CreateQuoteFromCalculationResultJson(string calculationResultJson)
        {
            Instant currentTimestamp = this.clock.GetCurrentInstant();
            var tenant = new Tenant(Guid.NewGuid(), "Tenant-Foo", "foo", null, default, default, currentTimestamp);
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                QuoteExpirySettings.Default,
                this.performingUserId,
                currentTimestamp,
                Guid.NewGuid(),
                Timezones.AET);
            quote.UpdateFormData(new Domain.Aggregates.Quote.FormData("{}"), this.performingUserId, currentTimestamp);
            var personAggregate = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, currentTimestamp);

            var customerAggregate = CustomerAggregate.CreateNewCustomer(
                tenant.Id, personAggregate, DeploymentEnvironment.Staging, this.performingUserId, null, currentTimestamp);
            quote.Aggregate.RecordAssociationWithCustomer(customerAggregate, personAggregate, this.performingUserId, currentTimestamp);
            PersonCommonProperties person = new PersonCommonProperties()
            {
                FullName = "Foo",
                MobilePhoneNumber = "04 1234 1234",
                Email = "foo@example.com",
            };

            var personDetails = new PersonalDetails(tenant.Id, person);
            personAggregate.Update(personDetails, this.performingUserId, currentTimestamp);

            var formDataSchema = new FormDataSchema(new JObject());

            var customerDetails = new PersonalDetails(personAggregate);
            quote.Aggregate.UpdateCustomerDetails(customerDetails, this.performingUserId, currentTimestamp, quote.Id);
            var formData = new Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var calculationData = new CachingJObjectWrapper(calculationResultJson);
            quote.UpdateFormData(formData, this.performingUserId, currentTimestamp);
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            quote.RecordCalculationResult(
                CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver),
                calculationData,
                currentTimestamp,
                formDataSchema,
                false,
                this.performingUserId);
            quote.AssignQuoteNumber("TestFoo", this.performingUserId, currentTimestamp);
            quote.Aggregate.CreateVersion(this.performingUserId, currentTimestamp, quote.Id);
            return quote;
        }
    }
}

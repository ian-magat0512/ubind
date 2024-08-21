// <copyright file="QuoteFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Imports;
    using UBind.Domain.Json;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Domain.ValueTypes;

    public static class QuoteFactory
    {
        public const DeploymentEnvironment DefaultEnvironment = DeploymentEnvironment.Staging;
        private static Guid? performingUserId = Guid.NewGuid();

        public static Instant DefaultTime { get; set; } = Instant.FromUtc(2018, 1, 1, 12, 0);

        public static Instant DefaultInceptionTime { get; set; } = DefaultTime.Plus(Duration.FromDays(1));

        public static Instant DefaultExpiryTime { get; set; } = DefaultInceptionTime.Plus(Duration.FromDays(365));

        public static IQuoteWorkflowProvider QuoteWorkflowProvider { get; set; } = new DefaultQuoteWorkflowProvider();

        public static IQuoteWorkflow QuoteWorkflow { get; set; } = new DefaultQuoteWorkflow();

        public static IQuoteExpirySettingsProvider QuoteExpirySettingsProvider { get; set; }
            = new DefaultExpirySettingsProvider();

        public static ProductConfiguration ProductConfiguation { get; set; }
            = new ProductConfiguration(ProductConfigurationJson.Default(), new FormDataSchema(JObject.Parse("{}")));

        public static ConfiguredIQumulateQuoteDatumLocations IQumulateQuoteDataLocations { get; set; }
            = JsonConvert.DeserializeObject<ConfiguredIQumulateQuoteDatumLocations>("{}");

        public static IClock Clock { get; set; } = SystemClock.Instance;

        // User ID to record in events for auditing purposes.
        public static Guid UserId { get; set; }

        public static StandardQuoteDataRetriever QuoteDataRetriever(CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            var productConfig = new ProductConfiguration(ProductConfigurationJson.Default(), new FormDataSchema(JObject.Parse("{}")));
            var quoteDataRetriever = new StandardQuoteDataRetriever(productConfig, formData, calculationData);
            return quoteDataRetriever;
        }

        public static IPersonalDetails PersonalDetails(Guid? tenantId = null)
        {
            tenantId = tenantId ?? TenantFactory.DefaultId;
            return new PersonalDetails(CreatePersonAggregate(tenantId));
        }

        public static PersonAggregate CreatePersonAggregate(Guid? tenantId = null)
        {
            tenantId = !tenantId.HasValue ? TenantFactory.DefaultId : tenantId.Value;

            var tenant = TenantFactory.Create(tenantId.Value);
            var person = new PersonCommonProperties()
            {
                FullName = "Noris McWhirter",
                MobilePhoneNumber = "04 1234 1234",
                Email = "test@test.com",
            };
            var personDetails = new PersonalDetails(tenant.Id, person);
            var personAggregate = PersonAggregate.CreatePersonFromPersonalDetails(
                tenant.Id, tenant.Details.DefaultOrganisationId, personDetails, performingUserId, Clock.Now());

            return personAggregate;
        }

        public static QuoteAggregate CreateImportedPolicy(
            Guid? tenantId = null,
            Guid? productId = null,
            DeploymentEnvironment environment = DefaultEnvironment,
            Guid? organisationId = null,
            Guid? personId = null,
            Guid? customerId = null,
            IPersonalDetails personalDetails = null)
        {
            tenantId = tenantId ?? TenantFactory.DefaultId;
            productId = productId ?? ProductFactory.DefaultId;
            organisationId = organisationId ?? Guid.NewGuid();
            if (personId == null)
            {
                var person = CreatePersonAggregate(tenantId);
                personId = person.Id;
            }

            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            var policyImportDataJson = new JObject()
            {
                { "CustomerEmail", "test@mail.co" },
                { "CustomerName", "John Doe" },
                { "PolicyNumber", "P-0001" },
                { "InceptionDate", "21/01/2020" },
                { "ExpiryDate", "21/01/2021" },
                { "PolicyInceptionDate", "21/01/2020" },
                { "PolicyEndDate", "21/01/2021" },
                { "PolicyExpiryDate", "21/01/2021" },
                { "PolicyStartDate", "21/01/2020" },
            };
            var policyImportData = new PolicyImportData(policyImportDataJson, PolicyMapping.Default());
            var aggregate = QuoteAggregate.CreateImportedPolicy(
                tenantId.Value,
                organisationId.Value,
                productId.Value,
                environment,
                personId.Value,
                customerId.GetValueOrDefault(),
                personalDetails ?? PersonalDetails(tenantId),
                policyImportData,
                Timezones.AET,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                null,
                Clock.Now(),
                null);

            return aggregate;
        }

        public static Quote CreateNewBusinessQuote(
            Guid? tenantId = null,
            Guid? productId = null,
            DeploymentEnvironment environment = DefaultEnvironment,
            IQuoteExpirySettings quoteExpirySettings = null,
            Guid? organisationId = null,
            bool isTestData = true,
            FormData? formData = null,
            string? quoteNumber = null,
            string? initialQuoteState = null,
            DateTimeZone? timeZone = null)
        {
            if (quoteExpirySettings == null)
            {
                quoteExpirySettings = QuoteExpirySettings.Default;
            }

            timeZone = timeZone ?? Timezones.AET;
            tenantId = tenantId ?? TenantFactory.DefaultId;
            productId = productId ?? ProductFactory.DefaultId;

            var quote = QuoteAggregate.CreateNewBusinessQuote(
                tenantId.Value,
                organisationId == null ? Guid.NewGuid() : organisationId.Value,
                productId.Value,
                environment,
                quoteExpirySettings,
                performingUserId,
                Clock.Now(),
                Guid.NewGuid(),
                timeZone,
                false,
                null,
                isTestData,
                formData?.JObject,
                quoteNumber,
                initialQuoteState);
            return quote;
        }

        public static Quote CreateQuoteWithPolicyIssued(
            Guid? tenantId = null,
            Guid? productId = null,
            DeploymentEnvironment environment = DefaultEnvironment,
            string formDataJson = null,
            string calculationResultJson = null,
            string policyNumber = "POLNUM1",
            Guid organisationId = default)
        {
            tenantId = tenantId.HasValue ? tenantId.Value : TenantFactory.DefaultId;
            productId = productId.HasValue ? productId.Value : ProductFactory.DefaultId;

            var quote = CreateNewBusinessQuote(tenantId, productId, environment, organisationId: organisationId);
            quote.Aggregate
                .WithCustomerDetails(quote.Id)
                .WithCustomer()
                .WithQuoteNumber(quote.Id)
                .WithCalculationResult(quote.Id, formDataJson, calculationResultJson)
                .WithSubmission(quote.Id)
                .WithPolicy(quote.Id, policyNumber);
            return quote;
        }

        public static QuoteAggregate CreateNewPolicyWithSubmittedQuote(
            Guid? tenantId = null,
            Guid? productId = null,
            DeploymentEnvironment environment = DefaultEnvironment,
            string formDataJson = null,
            string calculationResultJson = null,
            string policyNumber = "POLNUM1",
            Guid organisationId = default)
        {
            tenantId = tenantId.HasValue ? tenantId.Value : TenantFactory.DefaultId;
            productId = productId.HasValue ? productId.Value : ProductFactory.DefaultId;

            var quote = CreateNewBusinessQuote(tenantId, productId, environment, organisationId: organisationId);
            return quote.Aggregate
                .WithCustomerDetails(quote.Id)
                .WithCustomer()
                .WithQuoteNumber(quote.Id)
                .WithCalculationResult(quote.Id, formDataJson, calculationResultJson)
                .WithSubmission(quote.Id)
                .WithPolicy(quote.Id, policyNumber);
        }

        public static QuoteAggregate CreateNewPolicy(
            Guid? tenantId = null,
            Guid? productId = null,
            DeploymentEnvironment environment = DefaultEnvironment,
            string formDataJson = null,
            string calculationResultJson = null,
            string policyNumber = "POLNUM1",
            Guid organisationId = default,
            DateTimeZone? timeZone = null)
        {
            timeZone = timeZone ?? Timezones.AET;
            tenantId = tenantId.HasValue ? tenantId.Value : TenantFactory.DefaultId;
            productId = productId.HasValue ? productId.Value : ProductFactory.DefaultId;

            var quote = CreateNewBusinessQuote(tenantId, productId, environment, organisationId: organisationId, timeZone: timeZone);
            return quote.Aggregate
                .WithCustomerDetails(quote.Id)
                .WithCustomer()
                .WithQuoteNumber(quote.Id)
                .WithCalculationResult(quote.Id, formDataJson, calculationResultJson)
                .WithPolicy(quote.Id, policyNumber);
        }

        public static QuoteAggregate WithFormData(this QuoteAggregate quoteAggregate, Guid quoteId, string formDataJson = null)
        {
            if (formDataJson == null)
            {
                formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            }

            var formData = new Domain.Aggregates.Quote.FormData(formDataJson);
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            quote.UpdateFormData(formData, performingUserId, Clock.Now());
            return quoteAggregate;
        }

        public static QuoteAggregate WithCalculationResult(
            this QuoteAggregate quoteAggregate,
            Guid quoteId,
            string formDataJson = null,
            string calculationResultJson = null)
        {
            if (formDataJson == null)
            {
                formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            }

            var formData = new Domain.Aggregates.Quote.FormData(formDataJson);

            if (calculationResultJson == null)
            {
                calculationResultJson = CalculationResultJsonFactory.Create();
            }

            var calculationResultData = new CachingJObjectWrapper(calculationResultJson);
            var formDataSchema = new FormDataSchema(new JObject());
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            var formDataUpdateId = quote.UpdateFormData(formData, performingUserId, Clock.Now());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationResultData);
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationResultData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataUpdateId;
            quote.RecordCalculationResult(
                calculationResult,
                calculationResultData,
                Clock.Now(),
                formDataSchema,
                false,
                performingUserId);
            return quoteAggregate;
        }

        public static QuoteAggregate WithCustomerDetails(
            this QuoteAggregate quoteAggregate, Guid quoteId, IPersonalDetails personalDetails = null)
        {
            if (personalDetails == null)
            {
                personalDetails = new FakePersonalDetails();
            }

            quoteAggregate.UpdateCustomerDetails(personalDetails, performingUserId, Clock.Now(), quoteId);
            return quoteAggregate;
        }

        public static QuoteAggregate WithCustomer(
            this QuoteAggregate quoteAggregate, CustomerAggregate customerAggregate = null)
        {
            if (customerAggregate == null)
            {
                customerAggregate = CustomerAggregate.CreateNewCustomer(
                    quoteAggregate.TenantId,
                    CreatePersonAggregate(quoteAggregate.TenantId),
                    DeploymentEnvironment.Staging,
                    performingUserId,
                    null,
                    Clock.Now());
            }

            quoteAggregate.RecordAssociationWithCustomer(
                customerAggregate, PersonalDetails(quoteAggregate.TenantId), performingUserId, Clock.Now());
            return quoteAggregate;
        }

        public static QuoteAggregate WithQuoteNumber(this QuoteAggregate quoteAggregate, Guid quoteId, string quoteNumber = "ABCDEF")
        {
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            quote.Actualise(performingUserId, Clock.Now(), QuoteWorkflow);
            quote.AssignQuoteNumber(quoteNumber, performingUserId, Clock.Now());
            return quoteAggregate;
        }

        public static QuoteAggregate WithExpiryTime(this QuoteAggregate quoteAggregate, Instant dateTime, Guid quoteId)
        {
            quoteAggregate.SetExpiryDate(quoteId, dateTime, performingUserId, Clock.Now(), QuoteExpirySettings.Default);
            return quoteAggregate;
        }

        public static QuoteAggregate WithExpiryTimeFromSettings(this QuoteAggregate quoteAggregate, Guid quoteId)
        {
            var now = Clock.Now();
            quoteAggregate.SetQuoteExpiryFromSettings(quoteId, performingUserId, now, QuoteExpirySettings.Default);
            return quoteAggregate;
        }

        public static QuoteAggregate WithExpiryDate(this QuoteAggregate quoteAggregate, Instant dateTime, Guid quoteId)
        {
            quoteAggregate.SetExpiryDate(quoteId, dateTime, performingUserId, Clock.Now(), QuoteExpirySettings.Default);
            return quoteAggregate;
        }

        public static QuoteAggregate WithQuoteVersion(this QuoteAggregate quoteAggregate, Guid quoteId)
        {
            quoteAggregate.CreateVersion(performingUserId, Clock.Now(), quoteId);
            return quoteAggregate;
        }

        public static QuoteAggregate WithPolicy(this QuoteAggregate quoteAggregate, Guid quoteId, string policyNumber = "POLNUM1")
        {
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            var newBusinessQuote = quote as NewBusinessQuote;
            newBusinessQuote.IssuePolicy(
                quote.LatestCalculationResult.Id,
                () => policyNumber,
                ProductConfiguation,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                UserId,
                Clock.Now(),
                QuoteWorkflow);
            return quoteAggregate;
        }

        public static QuoteAggregate WithPolicyRenewalTransaction(this QuoteAggregate quoteAggregate, Guid quoteId)
        {
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            var renewalQuote = quote as RenewalQuote;
            renewalQuote.RenewPolicy(
                quote.LatestCalculationResult.Id,
                ProductConfiguation,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                UserId,
                Clock.Now(),
                QuoteWorkflow,
                false);
            return quoteAggregate;
        }

        public static QuoteAggregate WithPolicyAdjustmentTransaction(this QuoteAggregate quoteAggregate, Guid quoteId, Instant? timestamp = null)
        {
            timestamp = timestamp ?? Clock.Now();
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            var adjustmentQuote = quote as AdjustmentQuote;
            adjustmentQuote.AdjustPolicy(
                quote.LatestCalculationResult.Id,
                ProductConfiguation,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                UserId,
                timestamp.Value,
                QuoteWorkflow,
                false);
            return quoteAggregate;
        }

        public static QuoteAggregate WithPolicyCancellationTransaction(this QuoteAggregate quoteAggregate, Guid quoteId)
        {
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            var adjustmentQuote = quote as CancellationQuote;
            adjustmentQuote.CancelPolicy(
                quote.LatestCalculationResult.Id,
                ProductConfiguation,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                UserId,
                Clock.Now(),
                QuoteWorkflow,
                false);
            return quoteAggregate;
        }

        public static QuoteAggregate WithSubmission(this QuoteAggregate quoteAggregate, Guid quoteId)
        {
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            quote.Submit(UserId, Clock.Now(), QuoteWorkflow);
            return quoteAggregate;
        }

        public static AdjustmentQuote WithAdjustmentQuote(
            this QuoteAggregate quoteAggregate,
            Instant createdTimestamp = default,
            string quoteNumber = "QQQQQA")
        {
            if (createdTimestamp == default)
            {
                createdTimestamp = Clock.Now();
            }

            return quoteAggregate.Policy.CreateAdjustmentQuote(
                createdTimestamp,
                quoteNumber,
                Enumerable.Empty<IClaimReadModel>(),
                performingUserId,
                QuoteWorkflow,
                QuoteExpirySettings.Default,
                false,
                Guid.NewGuid(),
                initialQuoteState: StandardQuoteStates.Incomplete);
        }

        public static CancellationQuote WithCancellationQuote(
            this QuoteAggregate quoteAggregate,
            Instant createdTimestamp = default,
            string quoteNumber = "QQQQQC")
        {
            if (createdTimestamp == default)
            {
                createdTimestamp = Clock.Now();
            }

            return quoteAggregate.Policy.CreateCancellationQuote(
                createdTimestamp,
                quoteNumber,
                performingUserId,
                QuoteWorkflow,
                QuoteExpirySettings.Default,
                false,
                Guid.NewGuid(),
                initialQuoteState: StandardQuoteStates.Incomplete);
        }

        public static RenewalQuote WithRenewalQuote(
            this QuoteAggregate quoteAggregate,
            Instant createdTimestamp = default,
            string quoteNumber = "QQQQQR")
        {
            if (createdTimestamp == default)
            {
                createdTimestamp = Clock.Now();
            }

            return quoteAggregate.Policy.CreateRenewalQuote(
                Enumerable.Empty<IClaimReadModel>(),
                createdTimestamp,
                quoteNumber,
                performingUserId,
                QuoteWorkflow,
                QuoteExpirySettings.Default,
                QuoteFactory.ProductConfiguation,
                false,
                Guid.NewGuid(),
                initialQuoteState: StandardQuoteStates.Incomplete);
        }

        private class MockQuoteNumberGenerator : IQuoteReferenceNumberGenerator
        {
            public Guid TenantId { get; set; }

            public Guid ProductId { get; set; }

            public DeploymentEnvironment Environment { get; set; }

            public string Generate()
            {
                return "QUOTEX";
            }

            public void SetProperties(Guid tenantId, Guid productId, DeploymentEnvironment env)
            {
                this.TenantId = tenantId;
                this.ProductId = productId;
                this.Environment = env;
            }
        }
    }
}

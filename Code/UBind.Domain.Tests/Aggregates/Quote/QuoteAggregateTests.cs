// <copyright file="QuoteAggregateTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1202

namespace UBind.Domain.Tests.Aggregates.Quote
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Commands;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="QuoteAggregate"/>.
    ///
    /// There are integration tests covering the persistence quote aggregates in
    /// UBind.Persistence.Tests.Aggregates.Quote.QuoteAggregateIntegrationTests.
    ///
    /// There are integration tests covering related read models in
    /// UBind.Persistence.Tests.Aggregates.Quote.QuoteIntegrationTests.
    /// </summary>
    public class QuoteAggregateTests
    {
        private readonly IClock clock = SystemClock.Instance;
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly IQuoteWorkflow quoteWorkflow = new DefaultQuoteWorkflow();

        // Paths for patch tests.
        private readonly JsonPath validFormDataPath = new JsonPath("objectProperty.nestedProperty");
        private readonly JsonPath validCalculationResultPath = new JsonPath("questions.ratingPrimary.objectProperty.nestedProperty");

        [Fact]
        public void Submit_Throws_WhenApplicationHasNoFormData()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Staging;
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                organisationId,
                ProductFactory.DefaultId,
                environment,
                QuoteExpirySettings.Default,
                this.performingUserId,
                this.clock.Now(),
                Guid.NewGuid(),
                Timezones.AET);

            // Act
            Action act = () => quote.Submit(default, this.clock.GetCurrentInstant(), this.quoteWorkflow);

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("quote.submission.requires.form.data");
        }

        [Fact]
        public void Submit_Throws_WhenApplicationAlreadySubmitted()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Staging;
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                organisationId,
                ProductFactory.DefaultId,
                environment,
                QuoteExpirySettings.Default,
                this.performingUserId,
                this.clock.Now(),
                Guid.NewGuid(),
                Timezones.AET);
            quote.UpdateFormData(new FormData("{}"), this.performingUserId, this.clock.GetCurrentInstant());
            quote.Aggregate.WithCalculationResult(quote.Id);
            quote.Submit(default, this.clock.GetCurrentInstant(), this.quoteWorkflow);

            // Act
            Action act = () => quote.Submit(default, this.clock.GetCurrentInstant(), this.quoteWorkflow);

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("quote.already.submitted");
        }

        [Fact]
        public void QuoteVersionCreation_Succeeds_WhenQuoteIsNascent()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var sut = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                organisationId,
                ProductFactory.DefaultId,
                environment,
                QuoteExpirySettings.Default,
                this.performingUserId,
                this.clock.GetCurrentInstant(),
                Guid.NewGuid(),
                Timezones.AET);
            sut.Actualise(this.performingUserId, this.clock.Now(), this.quoteWorkflow);

            // Act
            Action act = () => sut.Aggregate.CreateVersion(this.performingUserId, this.clock.GetCurrentInstant(), sut.Id);

            // Assert
            act.Should().NotThrow<ErrorException>();
        }

        [Fact]
        public void UpdateFormData_Succeeds_WhenQuoteIsNotComplete()
        {
            // Arrange
            var sut = QuoteFactory.CreateNewBusinessQuote();
            var formDataJson = FormDataJsonFactory.GetUniqueSample();

            // Act
            sut.UpdateFormData(new FormData(formDataJson), this.performingUserId, this.clock.Now());

            // Assert
            Assert.Equal(formDataJson, sut.LatestFormData.Data.Json);
        }

        // SaveQuote
        [Fact]
        public void RecordCalculationResult_Fails_IfQuoteAlreadySubmitted()
        {
            // Arrange
            var sut = QuoteFactory.CreateNewBusinessQuote();
            sut.Aggregate.WithFormData(sut.Id)
            .WithCalculationResult(sut.Id)
            .WithSubmission(sut.Id);

            var formnDataSchema = new FormDataSchema(new JObject());
            var formData = new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            var formDataId = sut.UpdateFormData(formData, this.performingUserId, this.clock.Now());
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataId;

            // Act
            Action act = () => sut.RecordCalculationResult(
                    calculationResult,
                    calculationData,
                    this.clock.Now(),
                    formnDataSchema,
                    false,
                    this.performingUserId);

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.perform.operation.on.submitted.quote");
        }

        [Fact]
        public void RecordCalculationResult_Fails_IfCurrentQuoteHasPolicyIssued()
        {
            // Arrange
            var sut = QuoteFactory.CreateNewPolicy();

            var formnDataSchema = new FormDataSchema(new JObject());
            var quote = sut.GetQuoteOrThrow(sut.Policy.QuoteId.GetValueOrDefault());
            var formData = new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            var formDataId = quote.UpdateFormData(formData, this.performingUserId, this.clock.Now());
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataId;

            // Act
            Action act = () => quote.RecordCalculationResult(
                calculationResult,
                calculationData,
                this.clock.Now(),
                formnDataSchema,
                false,
                this.performingUserId);

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.perform.operation.on.issued.policy");
        }

        [Fact]
        public void RecordCalculationResult_Succeeds_IfCurrentQuoteNotComplete()
        {
            // Arrange
            var sut = QuoteFactory.CreateNewBusinessQuote();
            var formnDataSchema = new FormDataSchema(new JObject());
            var formData = new FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            var formDataId = sut.UpdateFormData(formData, this.performingUserId, this.clock.Now());
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataId;

            // Act
            sut.RecordCalculationResult(
                    calculationResult,
                    calculationData,
                    this.clock.Now(),
                    formnDataSchema,
                    false,
                    this.performingUserId);

            // Assert
            Assert.Equal(
                JObject.Parse(FormDataJsonFactory.GetSampleWithStartAndEndDates()).ToString(),
                JObject.Parse(sut.LatestFormData.Data.Json).ToString());
            Assert.Equal(
                JObject.Parse(CalculationResultJsonFactory.Create()).ToString(),
                JObject.Parse(sut.LatestCalculationResult.Data.Json).ToString());
        }

        [Fact]
        public void RecordCalculationResult_CalculatesAdjustmentPricingCorrectlyForNewBreakdowns()
        {
            // TODO: Implement me
        }

        // RecordCalculationResult
        // Succeeds

        // UpdateCustomerDetails
        // Throw if complete
        // Succeeds

        // AssignQuoteNumber
        // Throw if already assigned
        // Throw if customer not assigned
        // Succeed

        // CreateVersion
        // Throw if complete
        // Succeed

        // Discard
        // Throw if complete
        // Throw if already discarded
        // Succeed

        // Submit
        // Throw if complete
        // Succeed

        // MakeEnquiry
        // ?

        // IssuePolicy
        // Throw if complete
        // Throw if not new business quote
        // Throw if calculation result is not purchasable
        // Succeed
        [Fact]
        public void IssuePolicy_ResultsInPolicyCreation_WhenSuccessful()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewBusinessQuote();
            quote.Aggregate
                .WithCalculationResult(quote.Id)
                .WithCustomerDetails(quote.Id)
                .WithCustomer()
                .WithQuoteNumber(quote.Id);

            // Act
            var newBusinessQuote = quote as NewBusinessQuote;
            newBusinessQuote.IssuePolicy(
                quote.LatestCalculationResult.Id,
                () => "POL001",
                QuoteFactory.ProductConfiguation,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                this.performingUserId,
                this.clock.Now(),
                this.quoteWorkflow);

            // Assert
            Assert.NotNull(quote.Aggregate);
        }

        [Fact]
        public void IssuePolicy_ResultsInNewSingleTransaction_ForNewBusinessQuote()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewBusinessQuote();
            quote.Aggregate
                .WithCalculationResult(quote.Id)
                .WithCustomerDetails(quote.Id)
                .WithCustomer()
                .WithQuoteNumber(quote.Id);

            // Act
            var newBusinessQuote = quote as NewBusinessQuote;
            newBusinessQuote.IssuePolicy(
                quote.LatestCalculationResult.Id,
                () => "POL001",
                QuoteFactory.ProductConfiguation,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                this.performingUserId,
                this.clock.Now(),
                this.quoteWorkflow);

            // Assert
            Assert.Single(quote.Aggregate.Policy.Transactions);
        }

        [Fact]
        public void RenewPolicy_ResultsInRenewalTransaction_ForRenewalQuote()
        {
            // Arrange
            var seedGenerator = new Mock<IUniqueNumberSequenceGenerator>().Object;
            var quoteNumberGenerator = new QuoteReferenceNumberGenerator(seedGenerator);
            var oneYearFromNow = this.clock.Today().PlusYears(1);
            var aggregate = QuoteFactory.CreateNewPolicy();
            var renewQuote = aggregate.WithRenewalQuote();
            var quote = aggregate.GetQuoteOrThrow(renewQuote.Id);
            aggregate
                .WithCalculationResult(
                    quote.Id,
                    formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate: oneYearFromNow),
                    calculationResultJson: CalculationResultJsonFactory.Create(startDate: oneYearFromNow));

            // Act
            var renewalQuote = quote as RenewalQuote;
            renewalQuote.RenewPolicy(
                quote.LatestCalculationResult.Id,
                QuoteFactory.ProductConfiguation,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                this.performingUserId,
                this.clock.Now(),
                this.quoteWorkflow,
                false);

            // Assert
            Assert.Equal(2, aggregate.Policy.Transactions.Count);
        }

        // CreateAdjustmentQuote
        // Throw if undiscarded quote exists since policy's quote
        // Throw if policy is not adjustable.
        // Sets form data to policy's form data

        // CreateRenewalQuote
        // Throw if undiscarded quote exists since policy's quote
        // Throw if policy is not renewable (not close enough to expiry?, expired too long ago?)
        [Fact]
        public void CreateRenewalQuote_SetsInceptionDateToPreviousExpiryDate()
        {
            // Arrange
            var elevenMonthsAgo = this.clock.Today().PlusMonths(-11);
            var aggregate = QuoteFactory.CreateNewPolicy(
                formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(
                    inceptionDate: elevenMonthsAgo,
                    durationInMonths: 12),
                calculationResultJson: CalculationResultJsonFactory.Create(startDate: elevenMonthsAgo));
            var expectedNewInceptionDate = elevenMonthsAgo.PlusYears(1);

            // Act
            var quote = aggregate.Policy.CreateRenewalQuote(
                Enumerable.Empty<IClaimReadModel>(),
                this.clock.Now(),
                "FOO",
                this.performingUserId,
                this.quoteWorkflow,
                QuoteExpirySettings.Default,
                QuoteFactory.ProductConfiguation,
                false,
                Guid.NewGuid());

            // Assert
            var inceptionDateString = quote.LatestFormData.Data.GetValue(FormDataPaths.DefaultEffectiveDatePath);
            var newInceptionDate = inceptionDateString.ToLocalDateFromIso8601OrddMMyyyyOrddMMyy();
            Assert.Equal(expectedNewInceptionDate, newInceptionDate);
        }

        [Fact]
        public void CreateRenewalQuote_SetsExpiryDateToSensibleDefault()
        {
            // Arrange
            var elevenMonthsAgo = this.clock.Today().PlusMonths(-11);
            var aggregate = QuoteFactory.CreateNewPolicy(
                formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(
                    inceptionDate: elevenMonthsAgo,
                    durationInMonths: 12),
                calculationResultJson: CalculationResultJsonFactory.Create(startDate: elevenMonthsAgo));
            var expectedNewExpiryDate = elevenMonthsAgo.PlusMonths(24);

            // Act
            var quote = aggregate.Policy.CreateRenewalQuote(
                Enumerable.Empty<IClaimReadModel>(),
                this.clock.Now(),
                "FOO",
                this.performingUserId,
                this.quoteWorkflow,
                QuoteExpirySettings.Default,
                QuoteFactory.ProductConfiguation,
                false,
                Guid.NewGuid());

            // Assert
            var expiryDateString = quote.LatestFormData.Data.GetValue(FormDataPaths.DefaultExpiryDatePath);
            var newExpiryDate = expiryDateString.ToLocalDateFromIso8601OrddMMyyyyOrddMMyy();
            Assert.Equal(expectedNewExpiryDate, newExpiryDate);
        }

        [Fact]
        public void PatchFormData_ReturnSuccess_WhenGlobalCommandIsPassedWithNoCondition()
        {
            // Arrange
            var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
            var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
            var aggregate = QuoteFactory.CreateNewPolicyWithSubmittedQuote(
                formDataJson: formData,
                calculationResultJson: calculationResultJson);
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateGlobalPatchScope(),
                PatchRules.None);

            // Act
            var result = aggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void PatchFormData_ReturnsFailure_ForGlobalPatchWhenNoFormDataExists()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewBusinessQuote();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateGlobalPatchScope(),
                PatchRules.None);

            // Act
            var result = quote.Aggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal($"Could not find form data for quote {quote.Id}.", result.Error);
        }

        [Fact]
        public void PatchFormData_ReturnSuccess_ForFullQuotePatch()
        {
            // Arrange
            var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
            var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
            var aggregate = QuoteFactory.CreateNewPolicyWithSubmittedQuote(
                formDataJson: formData,
                calculationResultJson: calculationResultJson);
            var quote = aggregate.GetQuoteOrThrow(aggregate.Policy.QuoteId.GetValueOrDefault());

            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateFullQuotePatchScope(quote.Id),
                PatchRules.None);

            // Act
            var result = aggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void PatchFormData_ReturnsFailure_ForFullQuotePatchWithUnknownId()
        {
            // Arrange
            var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
            var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
            var aggregate = QuoteFactory.CreateNewPolicyWithSubmittedQuote(
                formDataJson: formData,
                calculationResultJson: calculationResultJson);
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateFullQuotePatchScope(Guid.NewGuid()),
                PatchRules.None);

            // Act
            var result = aggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Could not find any matching target to patch.", result.Error);
        }

        [Fact]
        public void PatchFormData_ReturnSuccess_ForPolicyTransactionPatchForExistingPolicy()
        {
            // Arrange
            var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
            var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
            var aggregate = QuoteFactory.CreateNewPolicyWithSubmittedQuote(
                formDataJson: formData,
                calculationResultJson: calculationResultJson);
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreatePolicyTransactionPatchScope(aggregate.Policy.Transactions.First().Id),
                PatchRules.None);

            // Act
            var result = aggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void UpdateFormDataWithCustomerDetails_ReturnSuccessAndUpdatesLatestFormData_WithCustomerDetailsChanged()
        {
            // Arrange
            var personalDetails = new FakePersonalDetails();
            var formDataWithContact = $@"{{
    ""formModel"": {{
        ""policyStartDate"": ""{LocalDatePattern.Iso.Format(SystemClock.Instance.Now().ToLocalDateInAet())}"",
        ""policyEndDate"": ""{LocalDatePattern.Iso.Format(SystemClock.Instance.Now().ToLocalDateInAet().PlusYears(1))}"",
        ""contactEmail"": ""{personalDetails.Email}"",
        ""contactName"": ""{personalDetails.DisplayName}"",
        ""contactMobile"": ""{personalDetails.MobilePhone}"",
        ""contactPhone"": ""{personalDetails.HomePhone}"",
        }}
    }}";
            var aggregate = QuoteFactory
                .CreateNewPolicy(formDataJson: formDataWithContact);
            var quote = aggregate.GetQuoteOrThrow(aggregate.Policy.QuoteId.GetValueOrDefault());
            aggregate
                .WithFormData(quote.Id, formDataWithContact);
            var contactQuestionMetaData = $@"{{
    ""questionMetaData"": {{
        ""questionSets"": {{
            ""contact"": {{
                ""contactName"": {{
                ""dataType"": ""text"",
                ""displayable"": false,
                ""canChangeWhenApproved"": true,
                ""private"": false,
                ""resetForNewQuotes"": false
                }},
            ""contactEmail"": {{
                ""dataType"": ""email"",
                ""displayable"": false,
                ""canChangeWhenApproved"": true,
                ""private"": false,
                ""resetForNewQuotes"": false
                }},
            ""contactPhone"": {{
                ""dataType"": ""phone"",
                ""displayable"": false,
                ""canChangeWhenApproved"": true,
                ""private"": false,
                ""resetForNewQuotes"": false
                }},
            ""contactMobile"": {{
                ""dataType"": ""phone"",
                ""displayable"": false,
                ""canChangeWhenApproved"": true,
                ""private"": false,
                ""resetForNewQuotes"": false
                }}
            }}
            }}
        }}
    }}";

            FormDataSchema dataSchema = new FormDataSchema(JObject.Parse(contactQuestionMetaData));
            var productConfiguration = new ProductConfiguration(ProductConfigurationJson.DatesFromCalculationResult(), dataSchema);
            var newEmail = "john.smith.new@example.com";
            personalDetails.Email = newEmail;

            // Act
            aggregate.UpdateFormDataWithCustomerDetails(personalDetails, dataSchema.GetQuestionMetaData(), productConfiguration, this.performingUserId, SystemClock.Instance.Now(), quote.Id);

            // Assert
            quote.LatestFormData.Data.GetValue("contactEmail").Should().Be(newEmail);
        }

        [Fact]
        public void PatchFormData_ReturnsFailure_ForPolicyTransactionPatchWithUnknownId()
        {
            // Arrange
            var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
            var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
            var aggregate = QuoteFactory.CreateNewPolicyWithSubmittedQuote(
                formDataJson: formData,
                calculationResultJson: calculationResultJson);
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreatePolicyTransactionPatchScope(Guid.NewGuid()),
                PatchRules.None);

            // Act
            var result = aggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Could not find any matching target to patch.", result.Error);
        }

        private int GetUnsavedEventSequenceNumber<TEvent>(QuoteAggregate quoteAggregate)
        {
            int sequenceNumber = 0;
            foreach (var @event in quoteAggregate.UnsavedEvents)
            {
                if (@event.GetType() == typeof(TEvent))
                {
                    return sequenceNumber;
                }

                sequenceNumber++;
            }

            return -1;
        }

        [Fact]
        public void QuoteCustomerDetailsUpdatedEvent_CustomerFullName_ShouldGenerateFromNameParts()
        {
            // Arrange
            var aggregateId = Guid.NewGuid();

            var customerDetails = new FakePersonalDetails();
            var timestamp = SystemClock.Instance.Now();
            var sut = new QuoteAggregate.CustomerDetailsUpdatedEvent(customerDetails.TenantId, aggregateId, Guid.NewGuid(), customerDetails, this.performingUserId, timestamp);
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
            };

            // Act
            var json = JsonConvert.SerializeObject(sut, serializerSettings);
            var deserializedEvent = JsonConvert.DeserializeObject<QuoteAggregate.CustomerDetailsUpdatedEvent>(json, serializerSettings);

            // Assert
            var fullName = PersonPropertyHelper.GetFullNameFromParts(
                        deserializedEvent.CustomerDetails.PreferredName,
                        deserializedEvent.CustomerDetails.NamePrefix,
                        deserializedEvent.CustomerDetails.FirstName,
                        deserializedEvent.CustomerDetails.LastName,
                        deserializedEvent.CustomerDetails.NameSuffix,
                        deserializedEvent.CustomerDetails.MiddleNames);
            fullName.Should().Be("Dr John (Jonno) Doe Smith Jr");
        }
    }
}

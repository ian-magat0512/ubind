// <copyright file="QuoteReadModelWriterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.ReadModels
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NodaTime;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Application.Tests.Fakes;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.ReadModels.Quote;
    using Xunit;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    public class QuoteReadModelWriterTests
    {
        private readonly Mock<IWritableReadModelRepository<NewQuoteReadModel>> mockQuoteReadModelRepository
            = new Mock<IWritableReadModelRepository<NewQuoteReadModel>>();

        private readonly Mock<IWritableReadModelRepository<PolicyReadModel>> mockPolicyReadModelRepository =
            new Mock<IWritableReadModelRepository<PolicyReadModel>>();

        private readonly Mock<IWritableReadModelRepository<PolicyTransaction>> mockPolicyTransactionReadModelRepository =
            new Mock<IWritableReadModelRepository<PolicyTransaction>>();

        private readonly Mock<IWritableReadModelRepository<CustomerReadModel>> mockCustomerReadModelRepository =
            new Mock<IWritableReadModelRepository<CustomerReadModel>>();

        private readonly Mock<ITenantRepository> mockTenantRepository =
            new Mock<ITenantRepository>();

        private readonly Mock<IQuoteFileAttachmentRepository> mockQuoteFileAttachmentRepository =
            new Mock<IQuoteFileAttachmentRepository>();

        private readonly Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();

        private readonly Mock<IWritableReadModelRepository<QuoteFileAttachmentReadModel>> mockFileAttachmentRepository =
            new Mock<IWritableReadModelRepository<QuoteFileAttachmentReadModel>>();

        private readonly Mock<IWritableReadModelRepository<PersonReadModel>> mockPersonReadModelRepository =
            new Mock<IWritableReadModelRepository<PersonReadModel>>();

        private readonly Mock<IPolicyTransactionTimeOfDayScheme> mockTimeOfDayScheme =
            new Mock<IPolicyTransactionTimeOfDayScheme>();

        private readonly Mock<IProductReleaseService> mockProductReleaseService = new Mock<IProductReleaseService>();

        private readonly PropertyTypeEvaluatorService propertyTypeEvaluatorService;
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly Guid tenantId = TenantFactory.DefaultId;
        private readonly IPolicyTransactionTimeOfDayScheme timeOfDayScheme;
        private IClock clock;

        public QuoteReadModelWriterTests()
        {
            this.clock = new TestClock();
            this.propertyTypeEvaluatorService = new PropertyTypeEvaluatorService(new Dictionary<AdditionalPropertyDefinitionType, IAdditionalPropertyValueProcessor>());
            this.timeOfDayScheme = this.mockTimeOfDayScheme.Object;
        }

        /// <summary>
        /// The Handle_UpdatesTheInceptionTimeStampOfTheNewBusinessQuote_WhenFormDataIsUpdated.
        /// </summary>
        [Fact]
        public async Task Handle_UpdatesTheInceptionTimeStampOfTheNewBusinessQuote_WhenFormDataIsUpdated()
        {
            // Arrange
            var timestamp = this.clock.Now();
            var writer = this.CreateQuoteReadModelWriter();

            // new effective date to be set on the form data
            var newEffectiveDate = new LocalDate(2023, 10, 04);

            // create formdata
            var sut = FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDates(newEffectiveDate, 1, 0);

            // create new business quote
            var quote = QuoteFactory.CreateNewBusinessQuote();
            var aggregate = quote.Aggregate;
            this.mockQuoteReadModelRepository.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new FakeNewQuoteReadModel(quote.Id));

            // The computation of the expected inception timestamp based on the form data
            Instant newEffectiveTimestamp =
                this.GetEffectiveTimestampForEffectiveDateUsingTimeOfDaySchemeBasedOnQuoteType(
                    newEffectiveDate,
                    timestamp,
                    quote.TimeZone,
                    quote.Type);

            // Act
            await writer.Handle(
                aggregate,
                new FormDataUpdatedEvent(
                    this.tenantId,
                    aggregate.Id,
                    quote.Id,
                    sut,
                    this.performingUserId,
                    timestamp),
                quote.EventSequenceNumber);

            var updatedQuote = aggregate.GetQuoteOrThrow(quote.Id);

            // Assert
            updatedQuote.Should().NotBeNull();
            updatedQuote.ReadModel.PolicyTransactionEffectiveTimestamp.Should().Be(newEffectiveTimestamp);
        }

        /// <summary>
        /// The Handle_UpdatesTheEffectiveTimestampOfTheAdjustmentQuote_WhenFormDataIsUpdated.
        /// </summary>
        [Fact]
        public async Task Handle_UpdatesTheEffectiveTimestampOfTheAdjustmentQuote_WhenFormDataIsUpdated()
        {
            // Arrange
            var timestamp = this.clock.Now();
            var writer = this.CreateQuoteReadModelWriter();

            // new effective date to be set on the form data
            var newEffectiveDate = new LocalDate(2023, 11, 04);

            // create the update formdata where the effective date is equal to the inception date
            var sut = FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDates(newEffectiveDate, 1, 0);

            // create adjustment quote
            var aggregate = QuoteFactory.CreateNewPolicy();
            var quote = aggregate.WithAdjustmentQuote(timestamp);

            this.mockQuoteReadModelRepository.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new FakeNewQuoteReadModel(quote.Id));

            // The computation of the expected inception timestamp based on the form data
            Instant newEffectiveTimestamp =
                this.GetEffectiveTimestampForEffectiveDateUsingTimeOfDaySchemeBasedOnQuoteType(
                    newEffectiveDate,
                    timestamp,
                    quote.TimeZone,
                    quote.Type);

            // Act
            await writer.Handle(
                aggregate,
                new FormDataUpdatedEvent(
                    this.tenantId,
                    aggregate.Id,
                    quote.Id,
                    sut,
                    this.performingUserId,
                    timestamp),
                quote.EventSequenceNumber);

            var updatedQuote = aggregate.GetQuoteOrThrow(quote.Id);

            // Assert
            updatedQuote.Should().NotBeNull();
            updatedQuote.ReadModel.PolicyTransactionEffectiveTimestamp.Should().Be(newEffectiveTimestamp);
        }

        /// <summary>
        /// The Handle_UpdatesTheEffectiveTimestampOfTheRenewalQuote_WhenFormDataIsUpdated.
        /// </summary>
        [Fact]
        public async Task Handle_UpdatesTheEffectiveTimestampOfTheRenewalQuote_WhenFormDataIsUpdated()
        {
            // Arrange
            var timestamp = this.clock.Now();
            var writer = this.CreateQuoteReadModelWriter();

            // new effective date to be set on the form data
            var newEffectiveDate = this.clock.Now().ToLocalDateInAet();

            // create the update formdata where the effective date is equal to the inception date
            var sut = FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDates(newEffectiveDate, 1, 0);

            // create renewal quote
            var aggregate = QuoteFactory.CreateNewPolicy();
            var quote = aggregate.WithRenewalQuote(timestamp);

            this.mockQuoteReadModelRepository.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new FakeNewQuoteReadModel(quote.Id));

            // The computation of the expected inception timestamp based on the form data
            Instant newEffectiveTimestamp =
                this.GetEffectiveTimestampForEffectiveDateUsingTimeOfDaySchemeBasedOnQuoteType(
                    newEffectiveDate,
                    timestamp,
                    quote.TimeZone,
                    quote.Type);

            // Act
            await writer.Handle(
                aggregate,
                new FormDataUpdatedEvent(
                    this.tenantId,
                    aggregate.Id,
                    quote.Id,
                    sut,
                    this.performingUserId,
                    timestamp),
                quote.EventSequenceNumber);

            var updatedQuote = aggregate.GetQuoteOrThrow(quote.Id);

            // Assert
            updatedQuote.Should().NotBeNull();
            updatedQuote.ReadModel.PolicyTransactionEffectiveTimestamp.Should().Be(newEffectiveTimestamp);
        }

        /// <summary>
        /// The Handle_UpdatesTheEffectiveTimestampOfTheCancellationQuote_WhenFormDataIsUpdated.
        /// </summary>
        [Fact]
        public async Task Handle_UpdatesTheEffectiveTimestampOfTheCancellationQuote_WhenFormDataIsUpdated()
        {
            // Arrange
            var timestamp = this.clock.Now();
            var writer = this.CreateQuoteReadModelWriter();

            // new effective date to be set on the form data
            var newEffectiveDate = this.clock.Now().ToLocalDateInAet();

            // create the update formdata where the effective date is equal to the inception date
            var sut = FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDates(newEffectiveDate, 1, 0);

            // create cancellation quote
            var aggregate = QuoteFactory.CreateNewPolicy();
            var quote = aggregate.WithCancellationQuote(timestamp);

            this.mockQuoteReadModelRepository.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new FakeNewQuoteReadModel(quote.Id));

            // The computation of the expected inception timestamp based on the form data
            Instant newEffectiveTimestamp =
                this.GetEffectiveTimestampForEffectiveDateUsingTimeOfDaySchemeBasedOnQuoteType(
                    newEffectiveDate,
                    timestamp,
                    quote.TimeZone,
                    quote.Type);

            // Act
            await writer.Handle(
                aggregate,
                new FormDataUpdatedEvent(
                    this.tenantId,
                    aggregate.Id,
                    quote.Id,
                    sut,
                    this.performingUserId,
                    timestamp),
                quote.EventSequenceNumber);

            var updatedQuote = aggregate.GetQuoteOrThrow(quote.Id);

            // Assert
            updatedQuote.Should().NotBeNull();
            updatedQuote.ReadModel.PolicyTransactionEffectiveTimestamp.Should().Be(newEffectiveTimestamp);
        }

        /// <summary>
        /// Get an instance of QuoteReadModelWriter
        /// </summary>
        private QuoteReadModelWriter CreateQuoteReadModelWriter()
        {
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var connectionConfig = new ConnectionStrings();
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            connectionConfig.UBind = config.GetConnectionString(DatabaseFixture.TestConnectionStringName);

            return new QuoteReadModelWriter(
                this.mockQuoteReadModelRepository.Object,
                this.mockPolicyReadModelRepository.Object,
                this.mockPolicyTransactionReadModelRepository.Object,
                this.mockFileAttachmentRepository.Object,
                this.mockCustomerReadModelRepository.Object,
                this.mockPersonReadModelRepository.Object,
                this.mockProductRepository.Object,
                new QuoteReadModelRepository(dbContext, connectionConfig, this.clock),
                this.propertyTypeEvaluatorService,
                this.mockQuoteFileAttachmentRepository.Object,
                this.mockTenantRepository.Object,
                this.clock,
                this.mockTimeOfDayScheme.Object,
                new DefaultProductConfigurationProvider(),
                this.mockProductReleaseService.Object);
        }

        /// <summary>
        /// Usually there is a time of day scheme in place, which has the following rule:
        /// If the effective date of the new business policy or adjustment transaction is
        /// today's date, then we can make that policy transaction active immediately, which means we set
        /// the effective time of day to the current time. This method will adjust the time of day
        /// according to those rules and convert it into timestamp.
        /// </summary>
        private Instant GetEffectiveTimestampForEffectiveDateUsingTimeOfDaySchemeBasedOnQuoteType(
            LocalDate effectiveDate,
            Instant now,
            DateTimeZone timeZone,
            QuoteType quoteType)
        {
            LocalDateTime effectiveDateTime = effectiveDate.At(this.timeOfDayScheme.GetEndTime());
            if (quoteType == QuoteType.NewBusiness || quoteType == QuoteType.Adjustment)
            {
                ZonedDateTime nowZonedDateTime = now.InZone(timeZone);
                var todaysDate = nowZonedDateTime.Date;
                if (effectiveDate.Equals(todaysDate))
                {
                    var nowLocalDateTime = nowZonedDateTime.LocalDateTime;
                    if ((nowLocalDateTime > effectiveDateTime && !this.timeOfDayScheme.DoesAllowInceptionTimeInThePast)
                        || (effectiveDateTime > nowLocalDateTime && this.timeOfDayScheme.DoesAllowImmediateCoverage))
                    {
                        effectiveDateTime = nowLocalDateTime;
                    }
                }
            }

            return effectiveDateTime.InZoneLeniently(timeZone).ToInstant();
        }
    }
}

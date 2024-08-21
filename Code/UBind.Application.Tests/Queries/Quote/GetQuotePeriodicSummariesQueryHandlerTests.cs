// <copyright file="GetQuotePeriodicSummariesQueryHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Queries.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Humanizer;
    using Moq;
    using NodaTime;
    using UBind.Application.Dashboard;
    using UBind.Application.Dashboard.Model;
    using UBind.Application.Queries.Quote;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.ValueTypes;
    using UBind.Web.ResourceModels;
    using Xunit;

    public class GetQuotePeriodicSummariesQueryHandlerTests
    {
        /// <summary>
        /// Defines the tenantId.
        /// </summary>
        private readonly Guid tenantId = TenantFactory.DefaultId;

        /// <summary>
        /// Defines the organisation.
        /// </summary>
        private readonly string organisation = Guid.NewGuid().ToString();

        /// <summary>
        /// Defines the userId.
        /// </summary>
        private readonly Guid userId = new Guid("A1B2A3B4-3333-2222-1111-000000000000");
        private readonly Mock<ICachingResolver> cacheResolver;
        private readonly Mock<IQuoteReadModelRepository> quoteReadModelRepository;
        private readonly ISummaryGeneratorFactory<QuoteDashboardSummaryModel, QuotePeriodicSummaryModel> summaryGeneratorFactory;

        public GetQuotePeriodicSummariesQueryHandlerTests()
        {
            this.cacheResolver = new Mock<ICachingResolver>();
            this.quoteReadModelRepository = new Mock<IQuoteReadModelRepository>();
            this.summaryGeneratorFactory = new QuoteSummaryGeneratorFactory();
        }

        [Fact]
        public async Task GetQuotePeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenDaySamplePeriodLength()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                            new Claim("Tenant", this.tenantId.ToString()),
                            new Claim("OrganisationId", this.organisation),
            }));
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = "2023-12-04";
            options.ToDateTime = "2023-12-31";
            options.SamplePeriodLengthString = SamplePeriodLength.Day.ToString();
            options.IncludeProperties = new List<string>()
            {
                nameof(QuotePeriodicSummaryModel.ConvertedCount),
            };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetQuotePeriodicSummariesQuery(this.tenantId, filters, options);
            this.quoteReadModelRepository.Setup(p => p.ListQuotesForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(Enumerable.Empty<QuoteDashboardSummaryModel>()));
            var handler = new GetQuotePeriodicSummariesQueryHandler(this.quoteReadModelRepository.Object, this.summaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            // expected to get 28 sets of summary for samplePeriodLength "day", between fromDate and toDate, including days with no quotes
            var expectedNumberOfSummarySets = 28;
            result.Count.Should().Be(expectedNumberOfSummarySets);
        }

        [Fact]
        public async Task GetQuotePeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenMonthSamplePeriodLength()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                            new Claim("Tenant", this.tenantId.ToString()),
                            new Claim("OrganisationId", this.organisation),
            }));
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = new LocalDateTime(2022, 12, 4, 0, 0, 0).ToExtendedIso8601();
            options.ToDateTime = new LocalDateTime(2023, 3, 25, 0, 0, 0).ToExtendedIso8601();
            options.SamplePeriodLengthString = Domain.Enums.SamplePeriodLength.Month.ToString();
            options.IncludeProperties = new List<string>()
            {
                nameof(QuotePeriodicSummaryModel.CreatedCount),
                nameof(QuotePeriodicSummaryModel.ConvertedCount),
                nameof(QuotePeriodicSummaryModel.AbandonedCount),
            };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetQuotePeriodicSummariesQuery(this.tenantId, filters, options);
            this.quoteReadModelRepository.Setup(p => p.ListQuotesForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(this.GetFakeQuoteData()));

            // expected to have the first summary set to be:
            var expected1stSummarySet = new QuotePeriodicSummaryModel()
            {
                FromDateTime = new LocalDateTime(2022, 12, 4, 0, 0, 0).ToExtendedIso8601(),
                ToDateTime = new LocalDateTime(2022, 12, 31, 0, 0, 0).ToExtendedIso8601(),
                CreatedCount = 3,
                AbandonedCount = 1,
                ConvertedCount = 2,
            };
            var handler = new GetQuotePeriodicSummariesQueryHandler(this.quoteReadModelRepository.Object, this.summaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            // expected to get 4 sets of summary for the month, including months with no quotes
            var expectedNumberOfSummarySets = 4;
            result.Count.Should().Be(expectedNumberOfSummarySets);

            // expected the first summary set will have its from date the start of the month period
            result.First().FromDateTime.Should().Be("2022-12-01T00:00:00.0000000+11:00");
            result.First().ToDateTime.Should().Be("2022-12-31T23:59:59.9999999+11:00");

            // expected to have the last date of the last set to be the end date of the month of the toDate
            result.Last().ToDateTime.Should().Be("2023-03-31T23:59:59.9999999+11:00");

            // expected to have correct AbandonesCount and ConvertedCount
            result.First().AbandonedCount.Should().Be(expected1stSummarySet.AbandonedCount);
            result.First().ConvertedCount.Should().Be(expected1stSummarySet.ConvertedCount);
        }

        [Fact]
        public async Task GetQuotePeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenQuarterSamplePeriodLength()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                            new Claim("Tenant", this.tenantId.ToString()),
                            new Claim("OrganisationId", this.organisation),
            }));
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = new LocalDateTime(2022, 12, 4, 0, 0, 0).ToExtendedIso8601();
            options.ToDateTime = new LocalDateTime(2023, 5, 25, 0, 0, 0).ToExtendedIso8601();
            options.SamplePeriodLengthString = SamplePeriodLength.Quarter.ToString();
            options.IncludeProperties = new List<string>()
            {
                nameof(QuotePeriodicSummaryModel.CreatedCount),
                nameof(QuotePeriodicSummaryModel.ConvertedCount),
                nameof(QuotePeriodicSummaryModel.AbandonedCount),
            };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetQuotePeriodicSummariesQuery(this.tenantId, filters, options);
            this.quoteReadModelRepository.Setup(p => p.ListQuotesForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(this.GetFakeQuoteData()));
            var handler = new GetQuotePeriodicSummariesQueryHandler(this.quoteReadModelRepository.Object, this.summaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            // expected to get 3 sets of summary for the quarter
            var expectedNumberOfSummarySets = 3;
            result.Count.Should().Be(expectedNumberOfSummarySets);

            // expected the first summary set will have its from date the start of the period
            result.First().FromDateTime.Should().Be("2022-10-01T00:00:00.0000000+10:00");

            // expected the last summary set will have its toDate the last the of the 2nd Quarter of 2023
            result.Last().ToDateTime.Should().Be("2023-06-30T23:59:59.9999999+10:00");
        }

        [Theory]
        [InlineData("2022-12-04")]
        [InlineData("")]
        public async Task GetQuotePeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenYearSamplePeriodLength(string fromDateTime)
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                            new Claim("Tenant", this.tenantId.ToString()),
                            new Claim("OrganisationId", this.organisation),
            }));
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = fromDateTime;
            options.ToDateTime = new LocalDateTime(2023, 3, 25, 0, 0, 0).ToExtendedIso8601();
            options.SamplePeriodLengthString = SamplePeriodLength.Year.ToString();
            options.IncludeProperties = new List<string>()
            {
                nameof(QuotePeriodicSummaryModel.CreatedCount),
                nameof(QuotePeriodicSummaryModel.ConvertedCount),
                nameof(QuotePeriodicSummaryModel.AbandonedCount),
            };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetQuotePeriodicSummariesQuery(this.tenantId, filters, options);
            this.quoteReadModelRepository.Setup(p => p.ListQuotesForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(this.GetFakeQuoteData()));
            var handler = new GetQuotePeriodicSummariesQueryHandler(this.quoteReadModelRepository.Object, this.summaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            // expected to get 2 sets of summary for the year, including year with no quotes
            var expectedNumberOfSummarySets = 2;
            result.Count.Should().Be(expectedNumberOfSummarySets);

            // expected the first summary set will have its from date the start of the year period
            result.First().FromDateTime.Should().Be("2022-01-01T00:00:00.0000000+11:00");

            // expected the first summary set will have its to date same as the end of the year date
            result.First().ToDateTime.Should().Be("2022-12-31T23:59:59.9999999+11:00");

            // expected to have correct AbandonesCount and ConvertedCount
            result.First().AbandonedCount.Should().Be(1);
            result.First().ConvertedCount.Should().Be(2);
        }

        [Theory]
        [InlineData("2022-12-01", "2022-12-30", 21600, 2, "2022-12-01T00:00:00.0000000+11:00", "2022-12-30T23:59:59.9999999+11:00")]
        [InlineData("2022-12-01", "2022-12-30", 1440, 30, "2022-12-01T00:00:00.0000000+11:00", "2022-12-30T23:59:59.9999999+11:00")]
        [InlineData("2022-12-01", "2022-12-30", 43199, 1, "2022-12-01T00:01:00.0000000+11:00", "2022-12-30T23:59:59.9999999+11:00")]
        [InlineData("2022-12-01", "2022-12-30", 44, 981, "2022-12-01T00:36:00.0000000+11:00", "2022-12-30T23:59:59.9999999+11:00")]
        [InlineData("2022-12-04", "2022-12-25", 7200, 4, "2022-12-06T00:00:00.0000000+11:00", "2022-12-25T23:59:59.9999999+11:00")]
        [InlineData("2023-09-02T01:20:45.2563974", "2023-09-15T01:20:45.2563974", 1440, 13, "2023-09-02T01:20:45.2563975+10:00", "2023-09-15T01:20:45.2563974+10:00")]
        [InlineData("2023-09-02T01:20:45.2563974", "2023-10-02T01:20:45.2563974", 1440, 30, "2023-09-02T01:20:45.2563975+10:00", "2023-10-02T01:20:45.2563974+11:00")]
        [InlineData("2022-12-01T09:00:05.0000000", "2022-12-25", 55, 644, "2022-12-01T09:40:00.0000000+11:00", "2022-12-25T23:59:59.9999999+11:00")]
        [InlineData("2022-12-01T08:45:00.0000000", "2022-12-25", 55, 645, "2022-12-01T08:45:00.0000000+11:00", "2022-12-25T23:59:59.9999999+11:00")]
        [InlineData("2022-12-01T08:55:00.0000001", "2022-12-25", 55, 644, "2022-12-01T09:40:00.0000000+11:00", "2022-12-25T23:59:59.9999999+11:00")]
        [InlineData("2022-12-01T08:45:00.00000009", "2022-12-25", 55, 645, "2022-12-01T08:45:00.0000000+11:00", "2022-12-25T23:59:59.9999999+11:00")]
        [InlineData("2022-12-01T08:55:00.0000001", "2022-12-26T01:20:45.2563974", 55, 646, "2022-12-01T09:10:45.2563975+11:00", "2022-12-26T01:20:45.2563974+11:00")]
        public async Task GetQuotePeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenCustomSamplePeriodMinutesIsValid(
            string fromDateTime,
            string toDateTime,
            int customSamplePeriodMinutes,
            int expectedNumberOfSummarySets,
            string firstSummaryFromDateTime,
            string lastSummarToDateTime)
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                            new Claim("Tenant", this.tenantId.ToString()),
                            new Claim("OrganisationId", this.organisation),
            }));
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = fromDateTime;
            options.ToDateTime = toDateTime;
            options.SamplePeriodLengthString = SamplePeriodLength.Custom.ToString();
            options.CustomSamplePeriodMinutesString = customSamplePeriodMinutes.ToString();
            options.IncludeProperties = new List<string>()
            {
                nameof(QuotePeriodicSummaryModel.CreatedCount),
                nameof(QuotePeriodicSummaryModel.ConvertedCount),
                nameof(QuotePeriodicSummaryModel.AbandonedCount),
            };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetQuotePeriodicSummariesQuery(this.tenantId, filters, options);
            this.quoteReadModelRepository.Setup(p => p.ListQuotesForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(this.GetFakeQuoteData().Where(p =>
                    p.CreatedTimestamp.ToLocalDateTimeInZone(options.Timezone) > options.FromDateTime.ToLocalDateTimeFromExtendedIso8601()
                    && p.CreatedTimestamp.ToLocalDateTimeInZone(options.Timezone) < options.ToDateTime.ToLocalDateTimeFromExtendedIso8601())));
            var handler = new GetQuotePeriodicSummariesQueryHandler(this.quoteReadModelRepository.Object, this.summaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            result.First().FromDateTime.Should().Be(firstSummaryFromDateTime);
            result.Last().ToDateTime.Should().Be(lastSummarToDateTime);
            result.Count.Should().Be(expectedNumberOfSummarySets);
        }

        [Theory]
        [InlineData("2022-12-04T00:00:00.0000000", "2022-12-25T00:00:00.0000000", "year", 1, "2022-01-01T00:00:00.0000000+11:00", "2022-12-31T23:59:59.9999999+11:00")]
        [InlineData("2022-12-04T00:00:00.0000000", "2022-12-27T00:00:00.0000000", "month", 1, "2022-12-01T00:00:00.0000000+11:00", "2022-12-31T23:59:59.9999999+11:00")]
        [InlineData("2022-11-04T00:00:00.0000000", "2022-12-25T00:00:00.0000000", "quarter", 1, "2022-10-01T00:00:00.0000000+10:00", "2022-12-31T23:59:59.9999999+11:00")]
        public async Task GetQuotePeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenDatesAreOnSamePeriod(
            string fromDate,
            string toDate,
            string samplePeriodLength,
            int expectedNumberOfSummary,
            string expectedFromDateOfFirstSummary,
            string expectedToDateOfLastSummary)
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                            new Claim("Tenant", this.tenantId.ToString()),
                            new Claim("OrganisationId", this.organisation),
            }));
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = fromDate;
            options.ToDateTime = toDate;
            options.SamplePeriodLengthString = samplePeriodLength;

            options.IncludeProperties = new List<string>()
            {
                nameof(QuotePeriodicSummaryModel.CreatedCount),
                nameof(QuotePeriodicSummaryModel.ConvertedCount),
                nameof(QuotePeriodicSummaryModel.AbandonedCount),
            };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetQuotePeriodicSummariesQuery(this.tenantId, filters, options);
            this.quoteReadModelRepository.Setup(p => p.ListQuotesForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(this.GetFakeQuoteData()));
            var handler = new GetQuotePeriodicSummariesQueryHandler(this.quoteReadModelRepository.Object, this.summaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            // expected to get 1 sets of summary for the year, including year with no quotes
            result.Count.Should().Be(expectedNumberOfSummary);

            // expected the first summary set will have its from date same as first quote
            result.First().FromDateTime.Should().Be(expectedFromDateOfFirstSummary);

            // expected the last summary set will have its to date same as expectedToDateOfLastSummary
            result.Last().ToDateTime.Should().Be(expectedToDateOfLastSummary);

            // expected to have correct AbandonesCount and ConvertedCount
            result.First().AbandonedCount.Should().Be(1);
            result.First().ConvertedCount.Should().Be(2);
            result.First().CreatedCount.Should().Be(3);
        }

        [Fact]
        public async Task GetQuotePeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenAllSamplePeriodLength()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                            new Claim("Tenant", this.tenantId.ToString()),
                            new Claim("OrganisationId", this.organisation),
            }));
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = "2022-12-04T00:00:00.0000000";
            options.ToDateTime = "2023-03-25T23:59:59.9999999";
            options.SamplePeriodLengthString = SamplePeriodLength.All.ToString();
            options.IncludeProperties = new List<string>()
            {
                nameof(QuotePeriodicSummaryModel.CreatedCount),
                nameof(QuotePeriodicSummaryModel.ConvertedCount),
                nameof(QuotePeriodicSummaryModel.AbandonedCount),
            };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetQuotePeriodicSummariesQuery(this.tenantId, filters, options);
            this.quoteReadModelRepository.Setup(p => p.ListQuotesForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(this.GetFakeQuoteData()));
            var handler = new GetQuotePeriodicSummariesQueryHandler(this.quoteReadModelRepository.Object, this.summaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            // expected to get 1 set of summary
            var expectedNumberOfSummarySets = 1;
            result.Count.Should().Be(expectedNumberOfSummarySets);

            // expected the first summary set will have its from date same as first quote
            result.First().FromDateTime.Should().Be("2022-12-04T00:00:00.0000000+11:00");

            // expected the summary set will have its toDate same as toDate of request
            result.First().ToDateTime.Should().Be("2023-03-25T23:59:59.9999999+11:00");

            // expected to have correct AbandonesCount and ConvertedCount
            result.First().AbandonedCount.Should().Be(1);
            result.First().ConvertedCount.Should().Be(2);
        }

        private IEnumerable<QuoteDashboardSummaryModel> GetFakeQuoteData()
        {
            return new List<QuoteDashboardSummaryModel>()
                {
                    new QuoteDashboardSummaryModel() // Converted quote
                    {
                        CreatedTicksSinceEpoch = new LocalDateTime(2022, 12, 4, 0, 0, 0).Date.GetTicksAtStartOfDayInZone(Timezones.AET),
                        LastModifiedTimeInTicksSinceEpoch = new LocalDateTime(2022, 12, 4, 0, 0, 0).Date.GetTicksAtStartOfDayInZone(Timezones.AET),
                        QuoteState = StandardQuoteStates.Complete,
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(),
                    },
                    new QuoteDashboardSummaryModel() // Converted quote
                    {
                        CreatedTicksSinceEpoch = new LocalDateTime(2022, 12, 5, 0, 0, 0).Date.GetTicksAtStartOfDayInZone(Timezones.AET),
                        LastModifiedTimeInTicksSinceEpoch = new LocalDateTime(2022, 12, 5, 0, 0, 0).Date.GetTicksAtStartOfDayInZone(Timezones.AET),
                        QuoteState = StandardQuoteStates.Complete,
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(),
                    },
                    new QuoteDashboardSummaryModel() // Abandoned quote
                    {
                        CreatedTicksSinceEpoch = new LocalDateTime(2022, 12, 24, 0, 0, 0).Date.GetTicksAtStartOfDayInZone(Timezones.AET),
                        LastModifiedTimeInTicksSinceEpoch = new LocalDateTime(2022, 12, 24, 0, 0, 0).Date.GetTicksAtStartOfDayInZone(Timezones.AET),
                        QuoteState = StandardQuoteStates.Incomplete,
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(),
                    },
                };
        }
    }
}
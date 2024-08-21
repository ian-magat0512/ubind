// <copyright file="GetPolicyTransactionPeriodicSummariesQueryHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Queries.PolicyTransaction
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
    using UBind.Application.Queries.PolicyTransaction;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using UBind.Web.ResourceModels;
    using Xunit;

    public class GetPolicyTransactionPeriodicSummariesQueryHandlerTests
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
        private readonly Mock<IPolicyTransactionReadModelRepository> policyTransactionReadModelRepository;
        private readonly ISummaryGeneratorFactory<PolicyTransactionDashboardSummaryModel, PolicyTransactionPeriodicSummaryModel> policyTransactionSummaryGeneratorFactory;

        public GetPolicyTransactionPeriodicSummariesQueryHandlerTests()
        {
            this.cacheResolver = new Mock<ICachingResolver>();
            this.policyTransactionReadModelRepository = new Mock<IPolicyTransactionReadModelRepository>();
            this.policyTransactionSummaryGeneratorFactory = new PolicyTransactionSummaryGeneratorFactory();
        }

        [Fact]
        public async Task GetPolicyTransactionPeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenDaySamplePeriodLength()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.tenantId.ToString()),
                new Claim("OrganisationId", this.organisation),
            }));
            var options = new PolicyTransactionPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = new LocalDateTime(2022, 12, 4, 0, 0, 0).ToExtendedIso8601();
            options.ToDateTime = new LocalDateTime(2022, 12, 31, 0, 0, 0).ToExtendedIso8601();
            options.SamplePeriodLengthString = Domain.Enums.SamplePeriodLength.Day.ToString();
            options.IncludeProperties = new List<string>()
            {
                nameof(PolicyTransactionPeriodicSummaryModel.CreatedCount),
            };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetPolicyTransactionPeriodicSummariesQuery(this.tenantId, filters, options);
            this.policyTransactionReadModelRepository.Setup(p => p.ListPolicyTransactionForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(Enumerable.Empty<PolicyTransactionDashboardSummaryModel>()));
            var handler = new GetPolicyTransactionPeriodicSummariesQueryHandler(this.policyTransactionReadModelRepository.Object, this.policyTransactionSummaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            // expected to get 28 sets of summary for samplePeriodLength "day", between fromDateTime and toDateTime, including days with no policyTransactions
            var expectedNumberOfSummarySets = 28;
            result.Count.Should().Be(expectedNumberOfSummarySets);
        }

        [Fact]
        public async Task GetPolicyTransactionPeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenMonthSamplePeriodLength()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.tenantId.ToString()),
                new Claim("OrganisationId", this.organisation),
            }));
            var options = new PolicyTransactionPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = new LocalDateTime(2022, 12, 4, 0, 0, 0).ToExtendedIso8601();
            options.ToDateTime = new LocalDateTime(2023, 3, 25, 0, 0, 0).ToExtendedIso8601();
            options.SamplePeriodLengthString = Domain.Enums.SamplePeriodLength.Month.ToString();
            options.IncludeProperties = new List<string>()
            {
                nameof(PolicyTransactionPeriodicSummaryModel.CreatedCount),
            };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetPolicyTransactionPeriodicSummariesQuery(this.tenantId, filters, options);
            this.policyTransactionReadModelRepository.Setup(p => p.ListPolicyTransactionForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(this.GetFakePolicyTransactionData()));

            // expected to have the first summary set to be:
            var expected1stSummarySet = new PolicyTransactionPeriodicSummaryModel()
            {
                FromDateTime = "2022-12-04T00:00:00.0000000+11:00",
                ToDateTime = "2022-12-31T23:59:59.9999999+11:00",
                CreatedCount = 3,
            };
            var handler = new GetPolicyTransactionPeriodicSummariesQueryHandler(this.policyTransactionReadModelRepository.Object, this.policyTransactionSummaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            // expected to get 4 sets of summary for the month, including months with no policyTransactions
            var expectedNumberOfSummarySets = 4;
            result.Count.Should().Be(expectedNumberOfSummarySets);

            // expected the first summary set will have its from date the start of the month period
            result.First().FromDateTime.Should().Be("2022-12-01T00:00:00.0000000+11:00");
            result.First().ToDateTime.Should().Be("2022-12-31T23:59:59.9999999+11:00");

            // expected to have the last date of the last set to be the end date of the month of the toDateTime
            result.Last().ToDateTime.Should().Be("2023-03-31T23:59:59.9999999+11:00");

            // expected to have correct AbandonedCount and CreatedCount
            result.First().CreatedCount.Should().Be(expected1stSummarySet.CreatedCount);
        }

        [Fact]
        public async Task GetPolicyTransactionPeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenQuarterSamplePeriodLength()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.tenantId.ToString()),
                new Claim("OrganisationId", this.organisation),
            }));
            var options = new PolicyTransactionPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = new LocalDateTime(2022, 12, 4, 0, 0, 0).ToExtendedIso8601();
            options.ToDateTime = new LocalDateTime(2023, 5, 25, 0, 0, 0).ToExtendedIso8601();
            options.SamplePeriodLengthString = Domain.Enums.SamplePeriodLength.Quarter.ToString();
            options.IncludeProperties = new List<string>()
            {
                nameof(PolicyTransactionPeriodicSummaryModel.CreatedCount),
            };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetPolicyTransactionPeriodicSummariesQuery(this.tenantId, filters, options);
            this.policyTransactionReadModelRepository.Setup(p => p.ListPolicyTransactionForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(this.GetFakePolicyTransactionData()));
            var handler = new GetPolicyTransactionPeriodicSummariesQueryHandler(this.policyTransactionReadModelRepository.Object, this.policyTransactionSummaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            // expected to get 3 sets of summary for the quarter
            var expectedNumberOfSummarySets = 3;
            result.Count.Should().Be(expectedNumberOfSummarySets);

            // expected the first summary set will have its from date the start of the period
            result.First().FromDateTime.Should().Be("2022-10-01T00:00:00.0000000+10:00");

            // expected the last summary set will have its toDateTime the last the of the 2nd Quarter of 2023
            result.Last().ToDateTime.Should().Be("2023-06-30T23:59:59.9999999+10:00");
        }

        [Theory]
        [InlineData("2022-12-04")]
        [InlineData("")]
        public async Task GetPolicyTransactionPeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenYearSamplePeriodLength(string fromDateTime)
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.tenantId.ToString()),
                new Claim("OrganisationId", this.organisation),
            }));
            var options = new PolicyTransactionPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = fromDateTime;
            options.ToDateTime = new LocalDateTime(2023, 3, 25, 0, 0, 0).ToExtendedIso8601();
            options.SamplePeriodLengthString = Domain.Enums.SamplePeriodLength.Year.ToString();
            options.IncludeProperties = new List<string>() { nameof(PolicyTransactionPeriodicSummaryModel.CreatedCount), };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetPolicyTransactionPeriodicSummariesQuery(this.tenantId, filters, options);
            this.policyTransactionReadModelRepository.Setup(p => p.ListPolicyTransactionForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(this.GetFakePolicyTransactionData()));
            var handler = new GetPolicyTransactionPeriodicSummariesQueryHandler(this.policyTransactionReadModelRepository.Object, this.policyTransactionSummaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            // expected to get 2 sets of summary for the year, including year with no policyTransactions
            var expectedNumberOfSummarySets = 2;
            result.Count.Should().Be(expectedNumberOfSummarySets);

            // expected the first summary set will have its from date the start of the year period
            result.First().FromDateTime.Should().Be("2022-01-01T00:00:00.0000000+11:00");

            // expected the first summary set will have its to date same as the end of the year date
            result.First().ToDateTime.Should().Be("2022-12-31T23:59:59.9999999+11:00");

            // expected to have correct CreatedCount
            result.First().CreatedCount.Should().Be(3);
        }

        [Theory]
        [InlineData("2022-12-04", "2022-12-25", "year", 1, "2022-01-01T00:00:00.0000000+11:00", "2022-12-31T23:59:59.9999999+11:00")]
        [InlineData("2022-12-04", "2022-12-27", "month", 1, "2022-12-01T00:00:00.0000000+11:00", "2022-12-31T23:59:59.9999999+11:00")]
        [InlineData("2022-11-04", "2022-12-25", "quarter", 1, "2022-10-01T00:00:00.0000000+10:00", "2022-12-31T23:59:59.9999999+11:00")]
        public async Task GetPolicyTransactionPeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenDatesAreOnSamePeriod(
            string fromDateTime,
            string toDateTime,
            string samplePeriodLength,
            int expectedNoOfSummary,
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
            var options = new PolicyTransactionPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = fromDateTime;
            options.ToDateTime = toDateTime;
            options.SamplePeriodLengthString = samplePeriodLength;
            options.IncludeProperties = new List<string>() { nameof(PolicyTransactionPeriodicSummaryModel.CreatedCount), };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetPolicyTransactionPeriodicSummariesQuery(this.tenantId, filters, options);
            this.policyTransactionReadModelRepository.Setup(p => p.ListPolicyTransactionForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(this.GetFakePolicyTransactionData()));
            var handler = new GetPolicyTransactionPeriodicSummariesQueryHandler(this.policyTransactionReadModelRepository.Object, this.policyTransactionSummaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            // expected to get 1 sets of summary for the year, including year with no policyTransactions
            result.Count.Should().Be(expectedNoOfSummary);

            // expected the first summary set will have its from date same as first policyTransaction
            result.First().FromDateTime.Should().Be(expectedFromDateOfFirstSummary);

            // expected the last summary set will have its to date same as expectedToDateOfLastSummary
            result.Last().ToDateTime.Should().Be(expectedToDateOfLastSummary);

            // expected to have correct CreatedCount
            result.First().CreatedCount.Should().Be(3);
        }

        [Fact]
        public async Task GetPolicyTransactionPeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenAllSamplePeriodLength()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.tenantId.ToString()),
                new Claim("OrganisationId", this.organisation),
            }));
            var options = new PolicyTransactionPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = new LocalDateTime(2022, 12, 4, 0, 0, 0).ToExtendedIso8601();
            options.ToDateTime = new LocalDateTime(2023, 3, 25, 0, 0, 0).ToExtendedIso8601();
            options.SamplePeriodLengthString = "all";
            options.IncludeProperties = new List<string>() { nameof(PolicyTransactionPeriodicSummaryModel.CreatedCount), };
            options.ValidateQueryOptions();
            this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
            this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
            this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
            var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
            var command = new GetPolicyTransactionPeriodicSummariesQuery(this.tenantId, filters, options);
            this.policyTransactionReadModelRepository.Setup(p => p.ListPolicyTransactionForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
                .Returns(Task.FromResult(this.GetFakePolicyTransactionData()));
            var handler = new GetPolicyTransactionPeriodicSummariesQueryHandler(this.policyTransactionReadModelRepository.Object, this.policyTransactionSummaryGeneratorFactory);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            // expected to get 1 set of summary
            var expectedNumberOfSummarySets = 1;
            result.Count.Should().Be(expectedNumberOfSummarySets);

            // expected the first summary set will have its from date same as first policyTransaction
            result.First().FromDateTime.Should().Be("2022-12-04T00:00:00.0000000+11:00");

            // expected the summary set will have its toDateTime same as toDateTime of request
            result.First().ToDateTime.Should().Be("2023-03-25T00:00:00.0000000+11:00");

            // expected to have correct CreatedCount
            result.First().CreatedCount.Should().Be(3);
        }

        private IEnumerable<PolicyTransactionDashboardSummaryModel> GetFakePolicyTransactionData()
        {
            return new List<PolicyTransactionDashboardSummaryModel>()
            {
                new PolicyTransactionDashboardSummaryModel() // Converted policyTransaction
                {
                    CreatedTicksSinceEpoch = new LocalDate(2022, 12, 4).GetTicksAtStartOfDayInZone(DateTimeZone.Utc),
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                },
                new PolicyTransactionDashboardSummaryModel() // Converted policyTransaction
                {
                    CreatedTicksSinceEpoch = new LocalDate(2022, 12, 5).GetTicksAtStartOfDayInZone(DateTimeZone.Utc),
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                },
                new PolicyTransactionDashboardSummaryModel() // Abandoned policyTransaction
                {
                    CreatedTicksSinceEpoch = new LocalDate(2022, 12, 24).GetTicksAtStartOfDayInZone(DateTimeZone.Utc),
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                },
            };
        }
    }
}
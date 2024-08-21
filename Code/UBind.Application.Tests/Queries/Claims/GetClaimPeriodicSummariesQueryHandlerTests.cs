// <copyright file="GetClaimPeriodicSummariesQueryHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Queries.Claim;

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
using UBind.Application.Queries.Claim;
using UBind.Domain;
using UBind.Domain.Enums;
using UBind.Domain.Extensions;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Claim;
using UBind.Domain.Tests.Fakes;
using UBind.Web.ResourceModels;
using Xunit;

public class GetClaimPeriodicSummariesQueryHandlerTests
{
    /// <summary>
    /// Defines the tenantId.
    /// </summary>
    private readonly Guid tenantId = TenantFactory.DefaultId;

    /// <summary>
    /// Defines the organisation.
    /// </summary>
    private readonly string organisationId = Guid.NewGuid().ToString();

    /// <summary>
    /// Defines the userId.
    /// </summary>
    private readonly Guid userId = new Guid("A1B2A3B4-3333-2222-1111-000000000000");
    private readonly Mock<ICachingResolver> cacheResolver;
    private readonly Mock<IClaimReadModelRepository> claimReadModelRepository;
    private readonly ISummaryGeneratorFactory<ClaimDashboardSummaryModel, ClaimPeriodicSummaryModel> summaryGeneratorFactory;

    public GetClaimPeriodicSummariesQueryHandlerTests()
    {
        this.cacheResolver = new Mock<ICachingResolver>();
        this.claimReadModelRepository = new Mock<IClaimReadModelRepository>();
        this.summaryGeneratorFactory = new ClaimSummaryGeneratorFactory();
    }

    [Fact]
    public async Task GetClaimPeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenDaySamplePeriodLength()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
            new Claim("Tenant", this.tenantId.ToString()),
            new Claim("OrganisationId", this.organisationId),
        }));
        var options = new ClaimPeriodicSummaryQueryOptionsModel();
        options.FromDateTime = new LocalDate(2022, 12, 4).ToIso8601();
        options.ToDateTime = new LocalDate(2022, 12, 31).ToIso8601();
        options.SamplePeriodLengthString = SamplePeriodLength.Day.ToString();
        options.IncludeProperties = new List<string>()
        {
            nameof(ClaimPeriodicSummaryModel.ProcessedCount),
        };
        options.ValidateQueryOptions();
        this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
        this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
        this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
        var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
        var command = new GetClaimPeriodicSummariesQuery(this.tenantId, filters, options);
        this.claimReadModelRepository.Setup(p => p.ListClaimsForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
            .Returns(Task.FromResult(Enumerable.Empty<ClaimDashboardSummaryModel>()));
        var handler = new GetClaimPeriodicSummariesQueryHandler(this.claimReadModelRepository.Object, this.summaryGeneratorFactory);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        // expected to get 28 sets of summary for samplePeriodLength "day", between fromDate and toDate, including days with no claims
        var expectedNoOfSummarySets = 28;
        result.Count.Should().Be(expectedNoOfSummarySets);
    }

    [Fact]
    public async Task GetClaimPeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenMonthSamplePeriodLength()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
            new Claim("Tenant", this.tenantId.ToString()),
            new Claim("OrganisationId", this.organisationId),
        }));
        var options = new ClaimPeriodicSummaryQueryOptionsModel();
        options.FromDateTime = new LocalDate(2022, 12, 4).ToIso8601();
        options.ToDateTime = new LocalDate(2023, 3, 25).ToIso8601();
        options.SamplePeriodLengthString = SamplePeriodLength.Month.ToString();
        options.IncludeProperties = new List<string>()
        {
            nameof(ClaimPeriodicSummaryModel.ProcessedCount),
            nameof(ClaimPeriodicSummaryModel.AverageProcessingTime),
            nameof(ClaimPeriodicSummaryModel.AverageSettlementAmount),
        };
        options.ValidateQueryOptions();
        this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
        this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
        this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
        var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
        var command = new GetClaimPeriodicSummariesQuery(this.tenantId, filters, options);
        this.claimReadModelRepository.Setup(p => p.ListClaimsForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
             .Returns(Task.FromResult(this.GetFakeClaimData()));

        // expected to have the first summary set to be:
        var expected1stSummarySet = new ClaimPeriodicSummaryModel()
        {
            FromDateTime = new LocalDate(2022, 12, 4).ToIso8601(),
            ToDateTime = new LocalDate(2022, 12, 31).ToIso8601(),
            ProcessedCount = 3,
        };
        var handler = new GetClaimPeriodicSummariesQueryHandler(this.claimReadModelRepository.Object, this.summaryGeneratorFactory);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        // expected to get 4 sets of summary for the month, including months with no claims
        var expectedNoOfSummarySets = 4;
        result.Count.Should().Be(expectedNoOfSummarySets);

        // expected the first summary set will have its from date the start of the month period
        result.First().FromDateTime.Should().Be("2022-12-01T00:00:00.0000000+11:00");
        result.First().ToDateTime.Should().Be("2022-12-31T23:59:59.9999999+11:00");

        // expected to have the last date of the last set to be the end date of the month of the toDate
        result.Last().ToDateTime.Should().Be("2023-03-31T23:59:59.9999999+11:00");

        // expected to have correct ProcessedCount
        result.First().ProcessedCount.Should().Be(expected1stSummarySet.ProcessedCount);
    }

    [Fact]
    public async Task GetClaimPeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenQuarterSamplePeriodLength()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
            new Claim("Tenant", this.tenantId.ToString()),
            new Claim("OrganisationId", this.organisationId),
        }));
        var options = new ClaimPeriodicSummaryQueryOptionsModel();
        options.FromDateTime = new LocalDate(2022, 12, 4).ToIso8601();
        options.ToDateTime = new LocalDate(2023, 5, 25).ToIso8601();
        options.SamplePeriodLengthString = SamplePeriodLength.Quarter.ToString();
        options.IncludeProperties = new List<string>()
        {
            nameof(ClaimPeriodicSummaryModel.ProcessedCount),
            nameof(ClaimPeriodicSummaryModel.AverageProcessingTime),
            nameof(ClaimPeriodicSummaryModel.AverageSettlementAmount),
        };
        options.ValidateQueryOptions();
        this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
        this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
        this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
        var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
        var command = new GetClaimPeriodicSummariesQuery(this.tenantId, filters, options);
        this.claimReadModelRepository.Setup(p => p.ListClaimsForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
             .Returns(Task.FromResult(this.GetFakeClaimData()));
        var handler = new GetClaimPeriodicSummariesQueryHandler(this.claimReadModelRepository.Object, this.summaryGeneratorFactory);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        // expected to get 3 sets of summary for the quarter
        result.Count.Should().Be(3);
        result.First().AverageProcessingTime.Should().Be(10.0f);
        result.First().AverageSettlementAmount.Should().Be(150.0f);

        // expected the first summary set will have its from date the start of the period
        result.First().FromDateTime.Should().Be("2022-10-01T00:00:00.0000000+10:00");

        // expected the last summary set will have its toDate the last the of the 2nd Quarter of 2023
        result.Last().ToDateTime.Should().Be("2023-06-30T23:59:59.9999999+10:00");
    }

    [Theory]
    [InlineData("2022-12-04")]
    [InlineData("")]
    public async Task GetClaimPeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenYearSamplePeriodLength(string fromDateTime)
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
            new Claim("Tenant", this.tenantId.ToString()),
            new Claim("OrganisationId", this.organisationId),
        }));
        var options = new ClaimPeriodicSummaryQueryOptionsModel();
        options.FromDateTime = fromDateTime;
        options.ToDateTime = new LocalDate(2023, 3, 25).ToIso8601();
        options.SamplePeriodLengthString = SamplePeriodLength.Year.ToString();
        options.IncludeProperties = new List<string>() {
            nameof(ClaimPeriodicSummaryModel.ProcessedCount),
            nameof(ClaimPeriodicSummaryModel.AverageProcessingTime),
            nameof(ClaimPeriodicSummaryModel.AverageSettlementAmount),
        };
        options.ValidateQueryOptions();
        this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
        this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
        this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
        var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
        var command = new GetClaimPeriodicSummariesQuery(this.tenantId, filters, options);
        this.claimReadModelRepository.Setup(p => p.ListClaimsForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
             .Returns(Task.FromResult(this.GetFakeClaimData()));
        var handler = new GetClaimPeriodicSummariesQueryHandler(this.claimReadModelRepository.Object, this.summaryGeneratorFactory);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        // expected to get 2 sets of summary for the year, including year with no claims
        var expectedNoOfSummarySets = 2;
        result.Count.Should().Be(expectedNoOfSummarySets);

        // expected the first summary set will have its from date the start of the year period
        result.First().FromDateTime.Should().Be("2022-01-01T00:00:00.0000000+11:00");

        // expected the first summary set will have its to date same as the end of the year date
        result.First().ToDateTime.Should().Be("2022-12-31T23:59:59.9999999+11:00");

        // expected to have correct ProcessedCount
        result.First().ProcessedCount.Should().Be(3);
        result.First().AverageProcessingTime.Should().Be(10.0f);
        result.First().AverageSettlementAmount.Should().Be(150.0f);
    }

    [Theory]
    [InlineData("2022-12-04T00:00:00.0000000", "2022-12-25T00:00:00.0000000", "year", 1, "2022-01-01T00:00:00.0000000+11:00", "2022-12-31T23:59:59.9999999+11:00")]
    [InlineData("2022-12-04T00:00:00.0000000", "2022-12-27T00:00:00.0000000", "month", 1, "2022-12-01T00:00:00.0000000+11:00", "2022-12-31T23:59:59.9999999+11:00")]
    [InlineData("2022-11-04T00:00:00.0000000", "2022-12-25T00:00:00.0000000", "quarter", 1, "2022-10-01T00:00:00.0000000+10:00", "2022-12-31T23:59:59.9999999+11:00")]
    public async Task GetClaimPeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenDatesAreOnSamePeriod(
        string fromDate,
        string toDate,
        string samplePeriodLength,
        int expectedNoOfSummary,
        string expectedFromDateTimeOfFirstSummary,
        string expectedToDateTimeOfLastSummary)
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
            new Claim("Tenant", this.tenantId.ToString()),
            new Claim("OrganisationId", this.organisationId),
        }));
        var options = new ClaimPeriodicSummaryQueryOptionsModel();
        options.FromDateTime = fromDate;
        options.ToDateTime = toDate;
        options.SamplePeriodLengthString = samplePeriodLength;
        options.IncludeProperties = new List<string>()
        {
            nameof(ClaimPeriodicSummaryModel.ProcessedCount),
            nameof(ClaimPeriodicSummaryModel.AverageProcessingTime),
            nameof(ClaimPeriodicSummaryModel.AverageSettlementAmount),
        };
        options.ValidateQueryOptions();
        this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
        this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
        this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
        var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
        var command = new GetClaimPeriodicSummariesQuery(this.tenantId, filters, options);
        this.claimReadModelRepository.Setup(p => p.ListClaimsForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
             .Returns(Task.FromResult(this.GetFakeClaimData()));
        var handler = new GetClaimPeriodicSummariesQueryHandler(this.claimReadModelRepository.Object, this.summaryGeneratorFactory);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        // expected to get 1 sets of summary for the year, including year with no claims
        result.Count.Should().Be(expectedNoOfSummary);

        // expected the first summary set will have its from date same as first claim
        result.First().FromDateTime.Should().Be(expectedFromDateTimeOfFirstSummary);

        // expected the last summary set will have its to date same as expectedToDateTimeOfLastSummary
        result.Last().ToDateTime.Should().Be(expectedToDateTimeOfLastSummary);

        // expected to have correct ProcessedCount
        result.First().ProcessedCount.Should().Be(3);
        result.First().AverageProcessingTime.Should().Be(10.0f);
        result.First().AverageSettlementAmount.Should().Be(150.0f);
    }

    [Fact]
    public async Task GetClaimPeriodicSummariesQuery_ReturnsCorrectListOfSummary_WhenAllSamplePeriodLength()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
            new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
            new Claim("Tenant", this.tenantId.ToString()),
            new Claim("OrganisationId", this.organisationId),
        }));
        var options = new ClaimPeriodicSummaryQueryOptionsModel();
        options.FromDateTime = new LocalDate(2022, 12, 4).ToIso8601();
        options.ToDateTime = new LocalDate(2023, 3, 25).ToIso8601();
        options.SamplePeriodLengthString = SamplePeriodLength.All.ToString();
        options.IncludeProperties = new List<string>()
        {
            nameof(ClaimPeriodicSummaryModel.ProcessedCount),
            nameof(ClaimPeriodicSummaryModel.AverageProcessingTime),
            nameof(ClaimPeriodicSummaryModel.AverageSettlementAmount),
        };
        options.ValidateQueryOptions();
        this.cacheResolver.Setup(p => p.GetTenantOrNull(this.tenantId)).Returns(Task.FromResult(It.IsAny<Domain.Tenant>()));
        this.cacheResolver.Setup(p => p.GetProductOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<Domain.Product.Product>()));
        this.cacheResolver.Setup(p => p.GetOrganisationOrNull(this.tenantId, It.IsAny<Guid>())).Returns(Task.FromResult(It.IsAny<OrganisationReadModel>()));
        var filters = await options.ToFilters(this.tenantId, this.cacheResolver.Object);
        var command = new GetClaimPeriodicSummariesQuery(this.tenantId, filters, options);
        this.claimReadModelRepository.Setup(p => p.ListClaimsForPeriodicSummary(this.tenantId, filters, CancellationToken.None))
             .Returns(Task.FromResult(this.GetFakeClaimData()));
        var handler = new GetClaimPeriodicSummariesQueryHandler(this.claimReadModelRepository.Object, this.summaryGeneratorFactory);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        // expected to get 1 set of summary
        var expectedNoOfSummarySets = 1;
        result.Count.Should().Be(expectedNoOfSummarySets);

        // expected the first summary set will have its from date same as first quote
        result.First().FromDateTime.Should().Be("2022-12-04T00:00:00.0000000+11:00");

        // expected the summary set will have its toDate same as toDate of request
        result.First().ToDateTime.Should().Be("2023-03-25T23:59:59.9999999+11:00");

        // expected to have correct ProcessedCount
        result.First().ProcessedCount.Should().Be(3);
        result.First().AverageProcessingTime.Should().Be(10.0f);
        result.First().AverageSettlementAmount.Should().Be(150.0f);
    }

    private IEnumerable<ClaimDashboardSummaryModel> GetFakeClaimData()
    {
        return new List<ClaimDashboardSummaryModel>()
        {
            new ClaimDashboardSummaryModel()
            {
                LodgedTicksSinceEpoch = new LocalDate(2022, 12, 1).GetTicksAtStartOfDayInZone(DateTimeZone.Utc),
                SettledTicksSinceEpoch = new LocalDate(2022, 12, 4).GetTicksAtStartOfDayInZone(DateTimeZone.Utc),
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ClaimState = ClaimState.Complete,
                Amount = 100,
            },
            new ClaimDashboardSummaryModel() // Settled claim
            {
                LodgedTicksSinceEpoch = new LocalDate(2022, 12, 1).GetTicksAtStartOfDayInZone(DateTimeZone.Utc),
                SettledTicksSinceEpoch = new LocalDate(2022, 12, 5).GetTicksAtStartOfDayInZone(DateTimeZone.Utc),
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ClaimState = ClaimState.Complete,
                Amount = 200,
            },
            new ClaimDashboardSummaryModel() // Declined claim
            {
                LodgedTicksSinceEpoch = new LocalDate(2022, 12, 1).GetTicksAtStartOfDayInZone(DateTimeZone.Utc),
                DeclinedTicksSinceEpoch = new LocalDate(2022, 12, 24).GetTicksAtStartOfDayInZone(DateTimeZone.Utc),
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ClaimState = ClaimState.Declined,
                Amount = 200,
            },
        };
    }
}
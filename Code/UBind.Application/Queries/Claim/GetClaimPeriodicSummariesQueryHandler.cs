// <copyright file="GetClaimPeriodicSummariesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Claim;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UBind.Application.Dashboard;
using UBind.Application.Dashboard.Model;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel.Claim;

/// <summary>
/// Handler for the GetClaimDashboardSummariesQuery command for obtaining a list of claim summaries with the given filters.
/// </summary>
public class GetClaimPeriodicSummariesQueryHandler :
    IQueryHandler<GetClaimPeriodicSummariesQuery, List<ClaimPeriodicSummaryModel>>
{
    private readonly IClaimReadModelRepository claimReadModelRepository;
    private readonly ISummaryGeneratorFactory<ClaimDashboardSummaryModel, ClaimPeriodicSummaryModel> periodicSummaryGeneratorFactory;

    public GetClaimPeriodicSummariesQueryHandler(
        IClaimReadModelRepository claimReadModelRepository,
        ISummaryGeneratorFactory<ClaimDashboardSummaryModel, ClaimPeriodicSummaryModel> periodicSummaryGeneratorFactory)
    {
        this.claimReadModelRepository = claimReadModelRepository;
        this.periodicSummaryGeneratorFactory = periodicSummaryGeneratorFactory;
    }

    public async Task<List<ClaimPeriodicSummaryModel>> Handle(GetClaimPeriodicSummariesQuery request, CancellationToken cancellationToken)
    {
        var claims = await this.claimReadModelRepository.ListClaimsForPeriodicSummary(request.TenantId, request.Filters, cancellationToken);
        var includeProperties = request.Options.IncludeProperties ?? Enumerable.Empty<string>();
        var periodType = request.Options.SamplePeriodLength;
        var startDateTime = request.Options.FromDateTime;
        if (startDateTime == null && claims.Any())
        {
            var settledOrDeclinedTimestamp = claims.FirstOrDefault()?.DeclinedTimestamp
                ?? claims.FirstOrDefault()?.SettledTimestamp;
            request.Options.SetFromDateTime(settledOrDeclinedTimestamp);
            startDateTime = request.Options.FromDateTime;
        }

        var endDateTime = request.Options.ToDateTime;
        var result = new List<ClaimPeriodicSummaryModel>();
        if (periodType == null || startDateTime == null || endDateTime == null)
        {
            return result;
        }

        var periodicSummaryGenerator = this.periodicSummaryGeneratorFactory
            .WithIncludeProperties(includeProperties)
            .ForPeriodAndDates(
                periodType.Value,
                startDateTime,
                endDateTime,
                request.Options.TimeZoneId,
                request.Options.CustomSamplePeriodMinutes);
        result = periodicSummaryGenerator.GenerateSummary(claims);
        return result;
    }
}
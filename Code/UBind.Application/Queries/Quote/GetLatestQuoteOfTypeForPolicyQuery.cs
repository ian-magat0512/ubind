// <copyright file="GetLatestQuoteOfTypeForPolicyQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Quote;

using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;

/// <summary>
/// Query to get the latest quote of a given type for a given policy.
/// Ignores discarded quotes.
/// </summary>
public class GetLatestQuoteOfTypeForPolicyQuery : IQuery<NewQuoteReadModel?>
{
    public GetLatestQuoteOfTypeForPolicyQuery(Guid tenantId, Guid policyId, QuoteType quoteType)
    {
        this.TenantId = tenantId;
        this.PolicyId = policyId;
        this.QuoteType = quoteType;
    }

    public Guid TenantId { get; }

    public Guid PolicyId { get; }

    public QuoteType QuoteType { get; }
}

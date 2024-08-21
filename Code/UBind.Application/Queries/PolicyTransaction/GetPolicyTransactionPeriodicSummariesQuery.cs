// <copyright file="GetPolicyTransactionPeriodicSummariesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.PolicyTransaction
{
    using System;
    using System.Collections.Generic;
    using UBind.Application.Dashboard.Model;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Query for obtaining a list of policy transaction summaries with the given filters.
    /// </summary>
    public class GetPolicyTransactionPeriodicSummariesQuery : IQuery<List<PolicyTransactionPeriodicSummaryModel>>
    {
        public GetPolicyTransactionPeriodicSummariesQuery(Guid tenantId, PolicyReadModelFilters filters, IPeriodicSummaryQueryOptionsModel options)
        {
            this.TenantId = tenantId;
            this.Filters = filters;
            this.Options = options;
        }

        public Guid TenantId { get; }

        public PolicyReadModelFilters Filters { get; }

        public IPeriodicSummaryQueryOptionsModel Options { get; }
    }
}

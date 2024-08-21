// <copyright file="PolicyReadModelSummariesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Policy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Queries for policy summaries.
    /// </summary>
    public class PolicyReadModelSummariesQueryHandler
        : IQueryHandler<PolicyReadModelSummariesQuery, IEnumerable<IPolicyReadModelSummary>>
    {
        private readonly IPolicyReadModelRepository policyReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyReadModelSummariesQueryHandler"/> class.
        /// </summary>
        /// <param name="policyReadModelRepository">The policy read model repo.</param>
        public PolicyReadModelSummariesQueryHandler(
            IPolicyReadModelRepository policyReadModelRepository)
        {
            this.policyReadModelRepository = policyReadModelRepository;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<IPolicyReadModelSummary>> Handle(PolicyReadModelSummariesQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var policies = this.policyReadModelRepository.ListPolicies(request.TenantId, request.Filters).ToList();
            return Task.FromResult(policies.AsEnumerable());
        }
    }
}

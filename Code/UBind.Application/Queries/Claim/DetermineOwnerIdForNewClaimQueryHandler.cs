// <copyright file="DetermineOwnerIdForNewClaimQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Claim
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Patterns.Cqrs;

    public class DetermineOwnerIdForNewClaimQueryHandler : IQueryHandler<DetermineOwnerIdForNewClaimQuery, Guid?>
    {
        private readonly ICustomerAggregateRepository customerAggregateRepository;

        public DetermineOwnerIdForNewClaimQueryHandler(ICustomerAggregateRepository customerAggregateRepository)
        {
            this.customerAggregateRepository = customerAggregateRepository;
        }

        public async Task<Guid?> Handle(DetermineOwnerIdForNewClaimQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Guid? ownerId = default;

            if (query.PerformingUser.GetTenantIdOrNull() != query.TenantId)
            {
                return null;
            }

            var resolvedCustomerId = query.CustomerId ?? query.PerformingUser.GetCustomerId();
            if (query.PerformingUser.IsCustomer() || resolvedCustomerId.HasValue)
            {
                ownerId = this.customerAggregateRepository.GetById(query.TenantId, resolvedCustomerId.Value)?.OwnerUserId;
            }
            else
            {
                ownerId = query.PerformingUser.GetId();
            }

            return await Task.FromResult(ownerId);
        }
    }
}

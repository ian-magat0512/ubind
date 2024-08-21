// <copyright file="DetermineOwnerIdForNewQuoteQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Quote
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using StackExchange.Profiling;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class DetermineOwnerIdForNewQuoteQueryHandler : IQueryHandler<DetermineOwnerIdForNewQuoteQuery, Guid?>
    {
        private readonly ICustomerReadModelRepository customerReadModelRepository;

        public DetermineOwnerIdForNewQuoteQueryHandler(
            ICustomerReadModelRepository customerReadModelRepositor)
        {
            this.customerReadModelRepository = customerReadModelRepositor;
        }

        public async Task<Guid?> Handle(DetermineOwnerIdForNewQuoteQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (MiniProfiler.Current.Step("DetermineOwnerIdForNewQuoteQuery"))
            {
                Guid? ownerId = default;
                if (query.PerformingUser.GetTenantIdOrNull() != query.TenantId)
                {
                    return null;
                }

                if (query.PerformingUser.IsCustomer())
                {
                    var resolvedCustomerId = query.CustomerId ?? query.PerformingUser.GetCustomerId();
                    if (!resolvedCustomerId.HasValue)
                    {
                        return null;
                    }
                    ownerId = this.customerReadModelRepository.GetCustomerOwnerId(query.TenantId, resolvedCustomerId.Value);
                }
                else
                {
                    ownerId = query.PerformingUser.GetId();
                }

                return await Task.FromResult(ownerId);
            }
        }
    }
}

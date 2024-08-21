// <copyright file="GetCustomersMatchingFiltersQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Customer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;

    public class GetCustomersMatchingFiltersQueryHandler
        : IQueryHandler<GetCustomersMatchingFiltersQuery, IReadOnlyList<ICustomerReadModelSummary>>
    {
        private readonly ICustomerReadModelRepository customerReadModelRepository;

        public GetCustomersMatchingFiltersQueryHandler(ICustomerReadModelRepository customerReadModelRepository)
        {
            this.customerReadModelRepository = customerReadModelRepository;
        }

        public Task<IReadOnlyList<ICustomerReadModelSummary>> Handle(
            GetCustomersMatchingFiltersQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<ICustomerReadModelSummary> customers
                = this.customerReadModelRepository.GetCustomersSummaryMatchingFilters(
                    request.TenantId, request.Filters).ToList();
            return Task.FromResult(customers);
        }
    }
}

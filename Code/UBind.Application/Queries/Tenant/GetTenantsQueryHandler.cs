// <copyright file="GetTenantsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Tenant
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class GetTenantsQueryHandler : IQueryHandler<GetTenantsQuery, List<Domain.Tenant>>
    {
        private readonly ITenantRepository tenantRepository;

        public GetTenantsQueryHandler(ITenantRepository tenantRepository)
        {
            this.tenantRepository = tenantRepository;
        }

        public Task<List<Tenant>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.IncludeDeleted)
            {
                return Task.FromResult(this.tenantRepository.GetTenants(request.Filters, request.IncludeMasterTenant));
            }
            else
            {
                return Task.FromResult(this.tenantRepository.GetActiveTenants(request.Filters, request.IncludeMasterTenant));
            }
        }
    }
}

// <copyright file="GetTenantByCustomDomainQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Tenant
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Create Get Tenant by custom domain command Handler.
    /// </summary>
    public class GetTenantByCustomDomainQueryHandler : IQueryHandler<GetTenantByCustomDomainQuery, Tenant>
    {
        private readonly ITenantRepository tenantRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetTenantByCustomDomainQueryHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The Tenant repository.</param>
        public GetTenantByCustomDomainQueryHandler(
            ITenantRepository tenantRepository)
        {
            this.tenantRepository = tenantRepository;
        }

        /// <inheritdoc/>
        public async Task<Tenant> Handle(GetTenantByCustomDomainQuery command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenant = this.tenantRepository.GetTenantByCustomDomain(command.DomainName);
            return await Task.FromResult(tenant);
        }
    }
}

// <copyright file="TenantService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class TenantService : ITenantService
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantService"/> class.
        /// </summary>
        /// <param name="tenantRepository">The tenant repository.</param>
        /// <param name="clock">Clock for obtaining current time.</param>
        public TenantService(
            ITenantRepository tenantRepository,
            IClock clock)
        {
            this.tenantRepository = tenantRepository;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public Domain.Tenant GetTenant(Guid tenantId)
        {
            return this.tenantRepository.GetTenantById(tenantId);
        }

        public void ThrowIfTenantAliasIsNull(string alias)
        {
            if (!string.IsNullOrEmpty(alias) && alias.ToLower() == "null")
            {
                throw new ErrorException(
                    Errors.Tenant.AliasIsNull(alias));
            }
        }

        public void ThrowIfTenantAliasInUse(string alias)
        {
            if (this.tenantRepository.IsAliasInUse(alias))
            {
                throw new ErrorException(
                    Errors.Tenant.AliasInUse(alias));
            }
        }

        public void ThrowIfTenantNameInUse(string name)
        {
            if (this.tenantRepository.IsNameInUse(name))
            {
                throw new ErrorException(
                    Errors.Tenant.NameInUse(name));
            }
        }

        public void ThrowIfCustomDomainInUse(string domain)
        {
            if (domain.IsNotNullOrEmpty() && this.tenantRepository.IsCustomDomainInUse(domain))
            {
                throw new ErrorException(
                    Errors.Tenant.CustomDomainInuse(domain));
            }
        }
    }
}

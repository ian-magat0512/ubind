// <copyright file="GetTenantByAliasQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Tenant
{
    using UBind.Domain.Patterns.Cqrs;

    public class GetTenantByAliasQuery : IQuery<Domain.Tenant>
    {
        public GetTenantByAliasQuery(string tenantAlias, bool includeDeleted = false)
        {
            this.TenantAlias = tenantAlias;
            this.IncludeDeleted = includeDeleted;
        }

        public string TenantAlias { get; }

        public bool IncludeDeleted { get; }
    }
}

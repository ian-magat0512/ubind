// <copyright file="GetDkimSettingsByTenantIdAndOrganisationIdQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.DkimSettings
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Query to get DKIM settings by tenantId and organisation Id.
    /// </summary>
    public class GetDkimSettingsByTenantIdAndOrganisationIdQuery : IQuery<IEnumerable<DkimSettings>>
    {
        public GetDkimSettingsByTenantIdAndOrganisationIdQuery(Guid tenantId, Guid organisationId)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
        }

        public Guid OrganisationId { get; }

        public Guid TenantId { get; }
    }
}

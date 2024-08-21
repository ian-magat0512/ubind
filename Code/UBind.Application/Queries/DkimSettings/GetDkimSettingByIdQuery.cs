// <copyright file="GetDkimSettingByIdQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.DkimSettings
{
    using System;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Query to get DKIM settings by Id.
    /// </summary>
    public class GetDkimSettingByIdQuery : IQuery<DkimSettings>
    {
        public GetDkimSettingByIdQuery(Guid tenantId, Guid dkimSettingsId, Guid organisationId)
        {
            this.TenantId = tenantId;
            this.DkimSettingsId = dkimSettingsId;
            this.OrganisationId = organisationId;
        }

        public Guid DkimSettingsId { get; }

        public Guid TenantId { get; }

        public Guid OrganisationId { get; }
    }
}

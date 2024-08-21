// <copyright file="GetDkimSettingByOrganisationIdAndDomainNameQuery.cs" company="uBind">
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
    /// Query for getting applicable DKIM settings for organisation.
    /// </summary>
    public class GetDkimSettingByOrganisationIdAndDomainNameQuery : IQuery<DkimSettings?>
    {
        public GetDkimSettingByOrganisationIdAndDomainNameQuery(
            Guid tenantId,
            Guid organisationId,
            string domain)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.Domain = domain;
        }

        public Guid OrganisationId { get; }

        public Guid TenantId { get; }

        public string Domain { get; }
    }
}

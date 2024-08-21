// <copyright file="GetIdsOfOrganisationsManagedByQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Organisation
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    public class GetIdsOfOrganisationsManagedByQuery : IQuery<List<Guid>>
    {
        /// <summary>
        /// Returns the IDs of organisations managed by the managing organisation
        /// If the managing organisation is not specified, returns the IDs of all organisations under the tenancy.
        /// </summary>
        public GetIdsOfOrganisationsManagedByQuery(Guid tenantId, Guid? managingOrganisationId)
        {
            this.TenantId = tenantId;
            this.ManagingOrganisationId = managingOrganisationId;
        }

        public Guid TenantId { get; set; }

        public Guid? ManagingOrganisationId { get; set; }
    }
}

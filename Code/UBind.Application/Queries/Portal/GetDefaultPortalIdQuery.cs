// <copyright file="GetDefaultPortalIdQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Portal
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command to get the ID of the default portal for a given tenant, organisation and user type.
    /// </summary>
    public class GetDefaultPortalIdQuery : IQuery<Guid?>
    {
        public GetDefaultPortalIdQuery(Guid tenantId, Guid organisationId, PortalUserType userType)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.UserType = userType;
        }

        public Guid TenantId { get; }

        public Guid OrganisationId { get; }

        public PortalUserType UserType { get; }
    }
}

// <copyright file="GetPortalUrlQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Portal
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    public class GetPortalUrlQuery : IQuery<string>
    {
        public GetPortalUrlQuery(
            Guid tenantId,
            Guid? organisationId,
            Guid? portalId,
            DeploymentEnvironment environment,
            string? path = null)
        {
            this.TenantId = tenantId;
            this.PortalId = portalId;
            this.OrganisationId = organisationId;
            this.Environment = environment;
            this.Path = path;
        }

        public Guid TenantId { get; }

        public Guid? OrganisationId { get; }

        public Guid? PortalId { get; }

        public DeploymentEnvironment Environment { get; }

        public string? Path { get; }
    }
}

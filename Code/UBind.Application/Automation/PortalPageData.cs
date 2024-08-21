// <copyright file="PortalPageData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using UBind.Application.Automation.Enums;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    public class PortalPageData
    {
        public PortalPageData(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? userId,
            EntityType entityType,
            PageType pageType,
            string tab,
            EntityListFilters filters,
            Guid entityId)
        {
            this.UserId = userId;
            this.EntityType = entityType;
            this.PageType = pageType;
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = environment;
            this.OrganisationId = organisationId;
            this.Tab = tab;
            this.Filters = filters;
            this.EntityId = entityId;
        }

        public Guid? UserId { get; }

        public EntityType EntityType { get; }

        public PageType PageType { get; }

        public Guid TenantId { get; }

        public Guid ProductId { get; }

        public DeploymentEnvironment Environment { get; }

        public Guid OrganisationId { get; }

        public string Tab { get; }

        public Guid EntityId { get; }

        public EntityListFilters Filters { get; }
    }
}

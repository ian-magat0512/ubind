// <copyright file="ExecutePortalPageTriggerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.PortalExtensions
{
    using System;
    using System.Security.Claims;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers.File;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class ExecutePortalPageTriggerCommand : ICommand<(FileInfo file, string successMessage)>
    {
        public ExecutePortalPageTriggerCommand(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment,
            string automationAlias,
            string triggerAlias,
            EntityType entityType,
            PageType pageType,
            string tab,
            ClaimsPrincipal user,
            EntityListFilters filters,
            Guid entityId)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.ProductId = productId;
            this.Environment = environment;
            this.AutomationAlias = automationAlias;
            this.TriggerAlias = triggerAlias;
            this.EntityType = entityType;
            this.PageType = pageType;
            this.Tab = tab;
            this.User = user;
            this.EntityId = entityId;
            this.Filters = filters;
        }

        public Guid TenantId { get; }

        public Guid OrganisationId { get; }

        public Guid ProductId { get; }

        public DeploymentEnvironment Environment { get; }

        public string AutomationAlias { get; }

        public string TriggerAlias { get; }

        public EntityType EntityType { get; }

        public PageType PageType { get; }

        public string Tab { get; }

        public ClaimsPrincipal User { get; }

        public Guid EntityId { get; }

        public EntityListFilters Filters { get; }
    }
}

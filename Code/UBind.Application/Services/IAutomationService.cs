// <copyright file="IAutomationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.ComponentModel;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers.File;
    using UBind.Domain;
    using UBind.Domain.Attributes;
    using UBind.Domain.Events;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Service for building and validating automation request data.
    /// </summary>
    public interface IAutomationService
    {
        /// <summary>
        /// Runs the  automation/s that matches with the given data parameters.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="organisationId">The ID of the organisation the request is for.</param>
        /// <param name="productId">The ID of the product the request is for.</param>
        /// <param name="environment">The environment being requested.</param>
        /// <param name="triggerRequest">The details of the trigger to be used.</param>
        /// <returns>requestDetails.</returns>
        Task<AutomationData> TriggerHttpAutomation(
            ReleaseContext releaseContext,
            Guid organisationId,
            TriggerRequest triggerRequest,
            CancellationToken cancellationToken);

        /// <summary>
        /// Runs the  automation/s that matches with the given data parameters.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="organisationId">The ID of the organisation the request is for.</param>
        /// <param name="productId">The ID of the product the request is for.</param>
        /// <param name="environment">The environment being requested.</param>
        /// <param name="systemEvent">The system event that will trigger  the automation.</param>
        /// <param name="automationAlias">The ID of the automation to match.</param>
        /// <returns>Success.</returns>
        [DisplayName("Trigger System Event Automation | TENANT: {1}, ORGANISATION: {3}, PRODUCT: {5}, ENVIRONMENT: {6}, AUTOMATION: {8}, EVENT: {9}")]
        [RequestIntent(Domain.Patterns.Cqrs.RequestIntent.ReadOnly)]
        Task TriggerSystemEventAutomation(
            Guid tenantId,
            string tenantAlias,
            Guid organisationId,
            string? organisationAlias,
            Guid productId,
            string productAlias,
            DeploymentEnvironment environment,
            SystemEvent systemEvent,
            string automationAlias,
            string eventType,
            Guid? productReleaseId,
            CancellationToken cancellationToken);

        [RequestIntent(Domain.Patterns.Cqrs.RequestIntent.ReadOnly)]
        Task TriggerPeriodicAutomation(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment,
            string alias,
            CancellationToken cancellationToken);

        [RequestIntent(Domain.Patterns.Cqrs.RequestIntent.ReadOnly)]
        Task<(FileInfo file, string successMessage)> TriggerPortalPageAutomation(
            Guid tenantId,
            Guid organisationId,
            Guid? productId,
            DeploymentEnvironment environment,
            string automationAlias,
            string triggerAlias,
            EntityType entityType,
            PageType pageType,
            string tab,
            ClaimsPrincipal user,
            EntityListFilters filters,
            Guid entityId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Sets the automation data context using relationships of the systemEvent.
        /// </summary>
        /// <param name="automationData">The automation data.</param>
        /// <param name="systemEvent">The system event.</param>
        void SetAutomationDataContext(AutomationData automationData, SystemEvent systemEvent);
    }
}

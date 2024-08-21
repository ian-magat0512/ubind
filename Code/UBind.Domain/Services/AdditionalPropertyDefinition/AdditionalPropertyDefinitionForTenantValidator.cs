// <copyright file="AdditionalPropertyDefinitionForTenantValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyDefinition
{
    using System;
    using Humanizer;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Validator of tenant context before persisting its data to additional property definition.
    /// </summary>
    public class AdditionalPropertyDefinitionForTenantValidator : AdditionalPropertyDefinitionContextValidator
    {
        private readonly ITenantRepository tenantRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionForTenantValidator"/> class.
        /// </summary>
        /// <param name="tenantRepository">Repository for tenant.</param>
        /// <param name="additionalPropertyDefinitionRepository">Additional property repository.</param>
        public AdditionalPropertyDefinitionForTenantValidator(
            ITenantRepository tenantRepository,
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository)
            : base(additionalPropertyDefinitionRepository)
        {
            this.tenantRepository = tenantRepository;
        }

        /// <inheritdoc/>
        public override Task ThrowIfValidationFailsOnCreate(
            Guid tenantId,
            string name,
            string alias,
            Guid contextId,
            AdditionalPropertyEntityType entityType,
            Guid? parentContextId)
        {
            this.ThrowExceptionIfTenantNotExists(contextId, name, entityType);

            this.ThrowIfValidationFailsForPropertyAliasAndNameOnCreate(
                tenantId,
                contextId,
                parentContextId,
                entityType,
                AdditionalPropertyDefinitionContextType.Tenant,
                name,
                alias);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task ThrowIfValidationFailsOnUpdate(
            Guid tenantId,
            Guid id,
            string name,
            string alias,
            Guid contextId,
            AdditionalPropertyEntityType entityType,
            Guid? parentContextId)
        {
            this.ThrowIfValidationForNameAliasAndIdFailsOnUpdate(
                tenantId,
                id,
                name,
                alias,
                AdditionalPropertyDefinitionContextType.Tenant,
                contextId,
                entityType,
                parentContextId);

            this.ThrowExceptionIfTenantNotExists(
                contextId,
                name,
                entityType);

            return Task.CompletedTask;
        }

        private void ThrowExceptionIfTenantNotExists(
            Guid tenantId, string propertyName, AdditionalPropertyEntityType entityType)
        {
            var queriedTenant = this.tenantRepository.GetTenantById(tenantId);

            if (queriedTenant == null)
            {
                throw new ErrorException(Errors.AdditionalProperties.TenantNotFound(
                    tenantId, entityType.Humanize(), propertyName));
            }
        }
    }
}

// <copyright file="AdditionalPropertyDefinitionForProductValidator.cs" company="uBind">
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

    /// <summary>
    /// Validator for product.
    /// </summary>
    public class AdditionalPropertyDefinitionForProductValidator : AdditionalPropertyDefinitionContextValidator
    {
        private readonly ICachingResolver cachingResolver;

        public AdditionalPropertyDefinitionForProductValidator(
            ICachingResolver cachingResolver,
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository)
            : base(additionalPropertyDefinitionRepository)
        {
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public override async Task ThrowIfValidationFailsOnCreate(
            Guid tenantId,
            string name,
            string alias,
            Guid contextId,
            AdditionalPropertyEntityType entityType,
            Guid? parentContextId)
        {
            await this.ThrowExceptionIfProductNotExistsUnderTenant(parentContextId.Value, contextId, name, entityType);

            this.ThrowIfValidationFailsForPropertyAliasAndNameOnCreate(
                tenantId, contextId, parentContextId, entityType, AdditionalPropertyDefinitionContextType.Product, name, alias);
        }

        /// <inheritdoc/>
        public override async Task ThrowIfValidationFailsOnUpdate(
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
                AdditionalPropertyDefinitionContextType.Product,
                contextId,
                entityType,
                parentContextId);

            await this.ThrowExceptionIfProductNotExistsUnderTenant(
                parentContextId.Value,
                contextId,
                name,
                entityType);
        }

        private async Task ThrowExceptionIfProductNotExistsUnderTenant(
            Guid tenantId, Guid productId, string propertyName, AdditionalPropertyEntityType entityType)
        {
            var product = await this.cachingResolver.GetProductOrNull(tenantId, productId);
            if (product == null)
            {
                throw new ErrorException(Errors.AdditionalProperties.ProductNotFound(
                    tenantId, productId, entityType.Humanize(), propertyName));
            }
        }
    }
}

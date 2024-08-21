// <copyright file="AdditionalPropertyDefinitionForOrganisationValidator.cs" company="uBind">
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
    /// Validator for organisation.
    /// </summary>
    public class AdditionalPropertyDefinitionForOrganisationValidator : AdditionalPropertyDefinitionContextValidator
    {
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionForOrganisationValidator"/> class.
        /// </summary>
        /// <param name="organisationReadModelRepository">Organisation read model repository.</param>
        /// <param name="additionalPropertyDefinitionRepository">Additional property repository.</param>
        public AdditionalPropertyDefinitionForOrganisationValidator(
            IOrganisationReadModelRepository organisationReadModelRepository,
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository)
            : base(additionalPropertyDefinitionRepository)
        {
            this.organisationReadModelRepository = organisationReadModelRepository;
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
            this.ThrowExceptionIfValidateFailsForOrganisation(contextId, parentContextId.Value, name, entityType);

            this.ThrowIfValidationFailsForPropertyAliasAndNameOnCreate(
                tenantId, contextId, parentContextId, entityType, AdditionalPropertyDefinitionContextType.Organisation, name, alias);

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
                AdditionalPropertyDefinitionContextType.Organisation,
                contextId,
                entityType,
                parentContextId);

            this.ThrowExceptionIfValidateFailsForOrganisation(
                contextId,
                parentContextId.Value,
                name,
                entityType);

            return Task.CompletedTask;
        }

        private void ThrowExceptionIfValidateFailsForOrganisation(
            Guid contextId, Guid parentContextId, string propertyName, AdditionalPropertyEntityType entityType)
        {
            // TODO: Once the type of the parentContextId is changed to GUID then we can remove this.
            var tenant = parentContextId;
            if (tenant == default)
            {
                throw new ErrorException(Errors.Tenant.NotFound(parentContextId.ToString()));
            }

            var organisation = this.organisationReadModelRepository.Get(parentContextId, contextId);
            if (organisation == null)
            {
                throw new ErrorException(Errors.AdditionalProperties.OrganisationNotFound(
                    parentContextId, contextId, entityType.Humanize(), propertyName));
            }
        }
    }
}

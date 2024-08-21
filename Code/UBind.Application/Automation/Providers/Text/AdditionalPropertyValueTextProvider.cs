// <copyright file="AdditionalPropertyValueTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Humanizer;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Reads a text value from an additional property of an entity.
    /// </summary>
    /// <remarks>Schema Reference: #additionalPropertyValueText.</remarks>
    public class AdditionalPropertyValueTextProvider : IProvider<Data<string?>>
    {
        private readonly BaseEntityProvider entityProvider;
        private readonly IProvider<Data<string>> propertyAlias;
        private readonly IProvider<Data<string?>>? valueIfNotSet;
        private readonly IProvider<Data<bool>>? raiseErrorIfNotDefined;
        private readonly IProvider<Data<string?>>? valueIfNotDefined;
        private readonly IProvider<Data<bool>>? raiseErrorIfNotSet;
        private readonly IProvider<Data<bool>>? raiseErrorIfTypeMismatch;
        private readonly IProvider<Data<string?>>? valueIfTypeMismatch;
        private readonly IProvider<Data<string?>>? defaultValue;
        private readonly ITextAdditionalPropertyValueReadModelRepository textAdditionalPropertyValueReadModelRepository;
        private readonly IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository;
        private readonly IStructuredDataAdditionalPropertyValueReadModelRepository structuredDataAdditionalPropertyValueReadModelRepository;

        public AdditionalPropertyValueTextProvider(
            BaseEntityProvider entityProvider,
            IProvider<Data<string>> propertyAlias,
            IProvider<Data<string?>>? valueIfNotSet,
            IProvider<Data<bool>>? raiseErrorIfNotDefined,
            IProvider<Data<string?>>? valueIfNotDefined,
            IProvider<Data<bool>>? raiseErrorIfNotSet,
            IProvider<Data<bool>>? raiseErrorIfTypeMismatch,
            IProvider<Data<string?>>? valueIfTypeMismatch,
            IProvider<Data<string?>>? defaultValue,
            ITextAdditionalPropertyValueReadModelRepository textAdditionalPropertyValueReadModelRepository,
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository,
            IStructuredDataAdditionalPropertyValueReadModelRepository structuredDataAdditionalPropertyValueReadModelRepository)
        {
            Contract.Assert(entityProvider != null);
            Contract.Assert(propertyAlias != null);

            this.entityProvider = entityProvider;
            this.propertyAlias = propertyAlias;
            this.valueIfNotSet = valueIfNotSet;
            this.raiseErrorIfNotDefined = raiseErrorIfNotDefined;
            this.valueIfNotDefined = valueIfNotDefined;
            this.raiseErrorIfNotSet = raiseErrorIfNotSet;
            this.raiseErrorIfTypeMismatch = raiseErrorIfTypeMismatch;
            this.valueIfTypeMismatch = valueIfTypeMismatch;
            this.defaultValue = defaultValue;
            this.textAdditionalPropertyValueReadModelRepository = textAdditionalPropertyValueReadModelRepository;
            this.additionalPropertyDefinitionRepository = additionalPropertyDefinitionRepository;
            this.structuredDataAdditionalPropertyValueReadModelRepository = structuredDataAdditionalPropertyValueReadModelRepository;
        }

        public string SchemaReferenceKey => "additionalPropertyValueText";

        public async ITask<IProviderResult<Data<string?>>> Resolve(IProviderContext providerContext)
        {
            var entity = (await this.entityProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var propertyAlias = (await this.propertyAlias.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            JObject errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);

            if (string.IsNullOrEmpty(propertyAlias))
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "propertyAlias",
                    this.SchemaReferenceKey));
            }

            errorData.Merge(GenericErrorDataHelper.RetrieveErrorData(entity));
            var isAdditionalPropertyEntityType = Enum.TryParse(entity.GetType().Name, out AdditionalPropertyEntityType additionalPropertyEntityType);
            var additionalErrorDetails = new List<string>
            {
                $"Additional Property: {propertyAlias}",
            };

            if (!entity.SupportsAdditionalProperties || !isAdditionalPropertyEntityType)
            {
                throw new ErrorException(
                    Errors.Automation.AdditionalPropertiesNotSupportedOnEntityType(
                        errorData,
                        entity.DomainEntityType.Name,
                        additionalErrorDetails));
            }

            // TODO: change this to use mediator once UB-9225 is completed, then use
            // GetAdditionalPropertyDefinitionByEntityTypeAndAliasQuery
            AdditionalPropertyDefinitionReadModel? additionalPropertyDefinition = this.additionalPropertyDefinitionRepository
                .GetAdditionalPropertyDefinitionByEntityTypeAndPropertyAlias(tenantId, propertyAlias, additionalPropertyEntityType);
            var doesEntityAdditionalPropertyDefinitionsContainPropertyAlias = additionalPropertyDefinition != null;
            if (!doesEntityAdditionalPropertyDefinitionsContainPropertyAlias)
            {
                var valueIfNotDefined = await this.ResolveValueIfNotDefined(
                    providerContext,
                    propertyAlias,
                    additionalPropertyEntityType.Humanize().ToCamelCase(),
                    additionalErrorDetails,
                    errorData);
                return ProviderResult<Data<string?>>.Success(valueIfNotDefined);
            }

            var additionalPropertyValue = this.GetAdditionalPropertyValue(
                tenantId, entity.Id, propertyAlias, additionalPropertyDefinition!.PropertyType);

            if (additionalPropertyValue == null && !string.IsNullOrEmpty(additionalPropertyDefinition.DefaultValue))
            {
                additionalPropertyValue = new Domain.Dto.AdditionalPropertyValueDto
                {
                    Value = additionalPropertyDefinition.DefaultValue,
                    EntityId = entity.Id,
                };
            }

            if (additionalPropertyValue == null)
            {
                var valueIfNotSet = await this.ResolveValueIfNotSet(
                    providerContext,
                    propertyAlias,
                    additionalPropertyEntityType.Humanize().ToCamelCase(),
                    additionalErrorDetails,
                    errorData);
                return ProviderResult<Data<string?>>.Success(valueIfNotSet);
            }

            if (!(additionalPropertyValue.Value is string))
            {
                var valueIfTypeMismatch = await this.ResolveValueIfTypeMismatch(
                    providerContext,
                    propertyAlias,
                    additionalPropertyDefinition.PropertyType.Humanize().ToCamelCase(),
                    "text",
                    errorData);
                return ProviderResult<Data<string?>>.Success(valueIfTypeMismatch);
            }

            return ProviderResult<Data<string?>>.Success(additionalPropertyValue.Value);
        }

        private async Task<string?> ResolveValueIfNotDefined(
            IProviderContext providerContext,
            string propertyAlias,
            string entityTypeName,
            List<string> additionalErrorDetails,
            JObject errorData)
        {
            if (this.valueIfNotDefined == null && this.defaultValue == null)
            {
                var raiseErrorIfNotDefined = (await this.raiseErrorIfNotDefined.ResolveValueIfNotNull(providerContext))?.DataValue ?? true;
                if (raiseErrorIfNotDefined)
                {
                    throw new ErrorException(
                    Errors.Automation.AdditionalPropertyAliasInvalid(
                        entityTypeName,
                        propertyAlias,
                        additionalErrorDetails,
                        errorData));
                }
            }

            if (this.valueIfNotDefined != null)
            {
                return (await this.valueIfNotDefined.Resolve(providerContext)).GetValueOrThrowIfFailed();
            }
            else
            {
                return await this.ResolveDefaultValue(providerContext);
            }
        }

        private async Task<string?> ResolveValueIfNotSet(
            IProviderContext providerContext,
            string propertyAlias,
            string entityTypeName,
            List<string> additionalErrorDetails,
            JObject errorData)
        {
            if (this.valueIfNotSet != null)
            {
                return (await this.valueIfNotSet.Resolve(providerContext)).GetValueOrThrowIfFailed();
            }
            else
            {
                var raiseErrorIfNotSet = (await this.raiseErrorIfNotSet.ResolveValueIfNotNull(providerContext))?.DataValue ?? false;
                if (raiseErrorIfNotSet)
                {
                    throw new ErrorException(
                    Errors.Automation.AdditionalPropertyValueNotSet(
                        entityTypeName,
                        propertyAlias,
                        additionalErrorDetails,
                        errorData));
                }
                else
                {
                    return await this.ResolveDefaultValue(providerContext);
                }
            }
        }

        private async Task<string?> ResolveValueIfTypeMismatch(
            IProviderContext providerContext,
            string propertyAlias,
            string resultType,
            string supportedType,
            JObject errorData)
        {
            if (this.valueIfTypeMismatch == null && this.defaultValue == null)
            {
                var raiseErrorIfTypeMismatch = (await this.raiseErrorIfTypeMismatch.ResolveValueIfNotNull(providerContext))?.DataValue ?? true;
                if (raiseErrorIfTypeMismatch)
                {
                    throw new ErrorException(
                    Errors.Automation.AdditionalPropertyValueInvalidType(
                        propertyAlias,
                        resultType,
                        supportedType,
                        errorData));
                }
            }

            if (this.valueIfTypeMismatch != null)
            {
                return (await this.valueIfTypeMismatch.Resolve(providerContext)).GetValueOrThrowIfFailed();
            }
            else
            {
                return await this.ResolveDefaultValue(providerContext);
            }
        }

        private async Task<string?> ResolveDefaultValue(IProviderContext providerContext)
        {
            if (this.defaultValue != null)
            {
                return (await this.defaultValue.Resolve(providerContext)).GetValueOrThrowIfFailed();
            }

            return null;
        }

        private Domain.Dto.AdditionalPropertyValueDto GetAdditionalPropertyValue(
            Guid tenantId,
            Guid entityId,
            string propertyAlias,
            AdditionalPropertyDefinitionType propertyType)
        {
            switch (propertyType)
            {
                case AdditionalPropertyDefinitionType.Text:
                    return this.textAdditionalPropertyValueReadModelRepository
                        .GetAdditionalPropertyValueByEntityIdAndPropertyAlias(tenantId, entityId, propertyAlias);
                case AdditionalPropertyDefinitionType.StructuredData:
                    return this.structuredDataAdditionalPropertyValueReadModelRepository
                        .GetAdditionalPropertyValueByEntityIdAndPropertyAlias(tenantId, entityId, propertyAlias);
                default:
                    throw new ErrorException(
                        Errors.AdditionalProperties.PropertyTypeNotYetSupported(propertyType.ToString()));
            }
        }
    }
}

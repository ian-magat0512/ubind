// <copyright file="AdditionalPropertyValueService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyValue;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using UBind.Domain.Aggregates.AdditionalPropertyValue;
using UBind.Domain.Aggregates.Claim;
using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Aggregates.Organisation;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Commands.AdditionalPropertyValue;
using UBind.Domain.Dto;
using UBind.Domain.Enums;
using UBind.Domain.Exceptions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Queries.AdditionalPropertyValue;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Claim;
using UBind.Domain.Services.AdditionalPropertyDefinition;

/// <summary>
/// Service responsible for populating values from additional property definitions into Entity-Attribute-Value (EAV) tables.
/// </summary>
/// <remarks>
/// This service interacts with various repositories and aggregates to manage additional property values for different entity types,
/// including quotes, policies, claims, customers, users, and organizations. It supports batch processing and handles both aggregate
/// and non-aggregate entities. The service also provides methods for querying, updating, and upserting additional property values.
/// </remarks>
public class AdditionalPropertyValueService : IAdditionalPropertyValueService
{
    private readonly ICqrsMediator mediator;
    private readonly ICustomerReadModelRepository customerReadModelRepository;
    private readonly IQuoteReadModelRepository quoteReadModelRepository;
    private readonly IPolicyTransactionReadModelRepository policyTransactionReadModelRepository;
    private readonly IQuoteVersionReadModelRepository quoteVersionReadModelRepository;
    private readonly IClaimVersionReadModelRepository claimVersionReadModelRepository;
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IClaimAggregateRepository claimAggregateRepository;
    private readonly PropertyTypeEvaluatorService propertyEvaluatorService;
    private readonly IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly ICustomerAggregateRepository customerAggregateRepository;
    private readonly IUserAggregateRepository userAggregateRepository;
    private readonly IOrganisationAggregateRepository organisationAggregateRepository;
    private readonly IClock clock;
    private readonly IAdditionalPropertyDefinitionJsonValidator additionalPropertyDefinitionJsonValidator;
    private readonly IAdditionalPropertyTransformHelper additionalPropertyTransformHelper;

    public AdditionalPropertyValueService(
        ICqrsMediator mediator,
        ICustomerReadModelRepository customerReadModelRepository,
        IQuoteReadModelRepository quoteReadModelRepository,
        IPolicyTransactionReadModelRepository policyTransactionReadModelRepository,
        IQuoteVersionReadModelRepository quoteVersionReadModelRepository,
        IClaimVersionReadModelRepository claimVersionReadModelRepository,
        IAdditionalPropertyDefinitionRepository additionalPropertyRepository,
        PropertyTypeEvaluatorService evaluatorService,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        IQuoteAggregateRepository quoteAggregateRepository,
        IClaimAggregateRepository claimAggregateRepository,
        ICustomerAggregateRepository customerAggregateRepository,
        IUserAggregateRepository userAggregateRepository,
        IOrganisationAggregateRepository organisationAggregateRepository,
        IClock clock,
        IAdditionalPropertyDefinitionJsonValidator additionalPropertyDefinitionJsonValidator,
        IAdditionalPropertyTransformHelper additionalPropertyTransformHelper)
    {
        this.mediator = mediator;
        this.customerReadModelRepository = customerReadModelRepository;
        this.quoteReadModelRepository = quoteReadModelRepository;
        this.policyTransactionReadModelRepository = policyTransactionReadModelRepository;
        this.quoteVersionReadModelRepository = quoteVersionReadModelRepository;
        this.claimVersionReadModelRepository = claimVersionReadModelRepository;
        this.propertyEvaluatorService = evaluatorService;
        this.additionalPropertyDefinitionRepository = additionalPropertyRepository;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.clock = clock;
        this.quoteAggregateRepository = quoteAggregateRepository;
        this.claimAggregateRepository = claimAggregateRepository;
        this.customerAggregateRepository = customerAggregateRepository;
        this.userAggregateRepository = userAggregateRepository;
        this.organisationAggregateRepository = organisationAggregateRepository;
        this.additionalPropertyDefinitionJsonValidator = additionalPropertyDefinitionJsonValidator;
        this.additionalPropertyTransformHelper = additionalPropertyTransformHelper;
    }

    /// <inheritdoc/>
    public async Task<List<AdditionalPropertyValueDto>> GetAdditionalPropertyValuesByEntityTypeAndEntityId(
        Guid tenantId,
        AdditionalPropertyEntityType entityType,
        Guid entityId)
    {
        List<AdditionalPropertyValueDto> result = new List<AdditionalPropertyValueDto>();
        foreach (AdditionalPropertyDefinitionType propertyType in Enum.GetValues(typeof(AdditionalPropertyDefinitionType)))
        {
            var queryModel = new GetAdditionalPropertyValuesQuery(
            tenantId,
            entityType,
            entityId,
            propertyType,
            null,
            null);
            result.AddRange(await this.mediator.Send(queryModel));
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task UpsertSetAdditionalPropertyValuesForNonAggregateEntity(
        Guid tenantId,
        Guid entityId,
        List<AdditionalPropertyValueUpsertModel> additionalPropertyValueModels)
    {
        additionalPropertyValueModels = this.StandardizeAdditionalPropertyStructuredDataValues(additionalPropertyValueModels);
        await this.ValidateAdditionalPropertyStructuredDataValues(tenantId, additionalPropertyValueModels);
        var commandModel = new UpsertValuesToEntityCommand(tenantId, entityId, additionalPropertyValueModels);
        await this.mediator.Send(commandModel);
    }

    public async Task<string> UpsertPropertiesForPolicyThenReturnPolicyNumber(
        QuoteAggregate quoteAggregate, List<AdditionalPropertyValueUpsertModel> properties)
    {
        properties = this.StandardizeAdditionalPropertyStructuredDataValues(properties);
        await this.UpsertAdditionalPropertyValues(
            quoteAggregate.TenantId,
            properties,
            quoteAggregate.Id,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.GetCurrentInstant(),
            quoteAggregate);
        return quoteAggregate.Policy.PolicyNumber;
    }

    /// <inheritdoc/>
    public async Task UpdateAdditionalPropertyValueForEntity(
        Guid tenantId,
        Guid entityId,
        DeploymentEnvironment environment,
        AdditionalPropertyDefinitionReadModel definition,
        string newValue)
    {
        var additionalPropertyValueIdModel = new AdditionalPropertyValueUpsertModel()
        {
            DefinitionId = definition.Id,
            Type = definition.PropertyType,
            Value = newValue,
        };
        var additionalPropertyValueIdModels = new List<AdditionalPropertyValueUpsertModel>() { additionalPropertyValueIdModel };

        var entityType = definition.EntityType;
        if (IsAnAggregateEntity(entityType))
        {
            await this.UpdatePropertiesOfEntityAggregate(tenantId, entityType, additionalPropertyValueIdModels, entityId, environment);
        }
        else
        {
            await this.UpsertSetAdditionalPropertyValuesForNonAggregateEntity(tenantId, entityId, additionalPropertyValueIdModels);
        }
    }

    /// <inheritdoc/>
    public async Task UpsertValuesForCustomer(
        CustomerAggregate customerAggregate,
        List<AdditionalPropertyValueUpsertModel> properties,
        Guid? performingUserId,
        Instant createdTimestamp)
    {
        properties = this.StandardizeAdditionalPropertyStructuredDataValues(properties);
        await this.UpsertAdditionalPropertyValues(
            customerAggregate.TenantId,
            properties,
            customerAggregate.Id,
            performingUserId,
            createdTimestamp,
            customerAggregate);
    }

    /// <inheritdoc/>
    public async Task UpsertValuesForCustomerFromPerson(
        Guid customerId,
        PersonAggregate personAggregate,
        List<AdditionalPropertyValueUpsertModel> properties,
        Guid? performingUserId,
        Instant createdTimestamp)
    {
        properties = this.StandardizeAdditionalPropertyStructuredDataValues(properties);
        await this.UpsertAdditionalPropertyValues(
             personAggregate.TenantId,
             properties,
             customerId,
             performingUserId,
             createdTimestamp,
             personAggregate);
    }

    /// <inheritdoc/>
    public async Task UpsertValuesForUser(
        List<AdditionalPropertyValueUpsertModel> properties,
        Guid? performingUserId,
        Instant createdTimestamp,
        UserAggregate userAggregate)
    {
        properties = this.StandardizeAdditionalPropertyStructuredDataValues(properties);
        await this.UpsertAdditionalPropertyValues(
            userAggregate.TenantId,
            properties,
            userAggregate.Id,
            performingUserId,
            createdTimestamp,
            userAggregate);
    }

    /// <inheritdoc/>
    public async Task UpsertValuesForOrganisation(
        List<AdditionalPropertyValueUpsertModel> properties,
        Organisation organisationAggregate,
        Guid? performingUserId,
        Instant createdTimestamp)
    {
        properties = this.StandardizeAdditionalPropertyStructuredDataValues(properties);
        await this.UpsertAdditionalPropertyValues(
            organisationAggregate.TenantId,
            properties,
            organisationAggregate.Id,
            performingUserId,
            createdTimestamp,
            organisationAggregate);
    }

    /// <inheritdoc/>
    public async Task<List<AdditionalPropertyValueDto>> GetAdditionalPropertyValuesByQueryModel(
         Guid tenantId,
         Guid? additionalPropertyDefinitionId,
         string value,
         Guid entityId,
         AdditionalPropertyDefinitionType propertyDefinitionType,
         AdditionalPropertyEntityType entityType)
    {
        var query = new GetAdditionalPropertyValuesQuery(
            tenantId,
            entityType,
            entityId,
            propertyDefinitionType,
            additionalPropertyDefinitionId,
            value);
        var result = await this.mediator.Send(query);
        return result;
    }

    /// <inheritdoc/>
    public async Task<string> UpdatePropertiesOfEntityAggregate(
        Guid tenantId,
        AdditionalPropertyEntityType entityType,
        List<AdditionalPropertyValueUpsertModel> upsertModels,
        Guid entityId,
        DeploymentEnvironment environment)
    {
        upsertModels = this.StandardizeAdditionalPropertyStructuredDataValues(upsertModels);
        await this.ValidateAdditionalPropertyStructuredDataValues(tenantId, upsertModels);
        await this.ValidateEntityUpdateAttemptAndThrowIfInvalid(tenantId, entityType, entityId, upsertModels);
        switch (entityType)
        {
            case AdditionalPropertyEntityType.Quote:
            case AdditionalPropertyEntityType.NewBusinessQuote:
            case AdditionalPropertyEntityType.RenewalQuote:
            case AdditionalPropertyEntityType.AdjustmentQuote:
            case AdditionalPropertyEntityType.CancellationQuote:
                return await this.UpsertPropertiesForQuoteThenReturnTheQuoteNumber(
                    tenantId, entityId, upsertModels);
            case AdditionalPropertyEntityType.QuoteVersion:
                return await this.UpsertPropertiesForQuoteVersionThenReturnTheQuoteNumber(
                    tenantId, upsertModels, entityId, environment);
            case AdditionalPropertyEntityType.Policy:
                return await this.UpsertPropertiesForPolicyThenReturnPolicyNumber(
                    tenantId, entityId, upsertModels);
            case AdditionalPropertyEntityType.NewBusinessPolicyTransaction:
            case AdditionalPropertyEntityType.RenewalPolicyTransaction:
            case AdditionalPropertyEntityType.AdjustmentPolicyTransaction:
            case AdditionalPropertyEntityType.CancellationPolicyTransaction:
            case AdditionalPropertyEntityType.PolicyTransaction:
                return await this.UpsertPropertiesForPolicyTransactionThenReturnPolicyNumberWithTransactionType(
                    tenantId, environment, upsertModels, entityId);
            case AdditionalPropertyEntityType.Claim:
                return await this.UpsertForClaimThenReturnClaimNumber(tenantId, entityId, upsertModels);
            case AdditionalPropertyEntityType.ClaimVersion:
                return await this.UpsertForClaimVersionThenReturnClaimNumberWithVersion(
                    tenantId, environment, upsertModels, entityId);
            case AdditionalPropertyEntityType.Customer:
                return await this.UpsertCustomerThenReturnFullName(tenantId, entityId, upsertModels);
            case AdditionalPropertyEntityType.User:
                await this.UpsertPropertiesForUser(tenantId, entityId, upsertModels);
                return string.Empty;
            case AdditionalPropertyEntityType.Organisation:
                await this.UpsertPropertiesForOrganisation(tenantId, entityId, upsertModels);
                return string.Empty;
            default:
                throw new ErrorException(
                    Errors
                        .AdditionalProperties
                        .EntityTypeNotSupportedForUpdatingAdditionalPropertyValueToEntityAggregate(
                            entityType.ToString()));
        }
    }

    private static bool IsAnAggregateEntity(AdditionalPropertyEntityType entityType)
    {
        switch (entityType)
        {
            case AdditionalPropertyEntityType.Quote:
            case AdditionalPropertyEntityType.NewBusinessQuote:
            case AdditionalPropertyEntityType.RenewalQuote:
            case AdditionalPropertyEntityType.AdjustmentQuote:
            case AdditionalPropertyEntityType.CancellationQuote:
            case AdditionalPropertyEntityType.QuoteVersion:
            case AdditionalPropertyEntityType.Policy:
            case AdditionalPropertyEntityType.NewBusinessPolicyTransaction:
            case AdditionalPropertyEntityType.RenewalPolicyTransaction:
            case AdditionalPropertyEntityType.AdjustmentPolicyTransaction:
            case AdditionalPropertyEntityType.CancellationPolicyTransaction:
            case AdditionalPropertyEntityType.PolicyTransaction:
            case AdditionalPropertyEntityType.Claim:
            case AdditionalPropertyEntityType.ClaimVersion:
            case AdditionalPropertyEntityType.Customer:
            case AdditionalPropertyEntityType.User:
            case AdditionalPropertyEntityType.Organisation:
                return true;
            default:
                return false;
        }
    }

    private async Task ValidateStructuredDataValue(
        Guid tenantId,
        Guid definitionId,
        string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        var propertyDefinition = await this.additionalPropertyDefinitionRepository.GetById(tenantId, definitionId);
        if (propertyDefinition.PropertyType == AdditionalPropertyDefinitionType.StructuredData
            && propertyDefinition.SchemaType != AdditionalPropertyDefinitionSchemaType.None
            && !string.IsNullOrEmpty(value))
        {
            this.additionalPropertyDefinitionJsonValidator.ThrowIfValueFailsSchemaAssertion(
                propertyDefinition.SchemaType.Value,
                propertyDefinition.Name,
                value,
                propertyDefinition.CustomSchema);
        }
    }

    private async Task ValidateAdditionalPropertyStructuredDataValues(Guid tenantId, List<AdditionalPropertyValueUpsertModel> upsertModels)
    {
        var structuredDataItems = upsertModels
            .Where(u => u.Type == AdditionalPropertyDefinitionType.StructuredData);
        foreach (var valueItems in structuredDataItems)
        {
            await this.ValidateStructuredDataValue(
                tenantId,
                valueItems.DefinitionId,
                valueItems.Value);
        }
    }

    /// This is called when saving an entity with additional properties. It validates that
    /// all of the required properties have been provided, and that any properties that are supposed to be unique
    /// are actually unique.
    private async Task ValidateEntityUpdateAttemptAndThrowIfInvalid(
        Guid tenantId,
        AdditionalPropertyEntityType entityType,
        Guid entityId,
        List<AdditionalPropertyValueUpsertModel> additionalProperties,
        Guid? organisationId = null)
    {
        var definitions = await this.additionalPropertyTransformHelper.GetAdditionalPropertyDefinitions(tenantId, organisationId, entityType);
        foreach (var definition in definitions)
        {
            var propertyItem = additionalProperties.FirstOrDefault(x => x.DefinitionId == definition.Id);

            if (definition.IsRequired)
            {
                // The definition is required but it doesn't have a value.
                if (propertyItem == null)
                {
                    var error = Errors.AdditionalProperties.AdditionalPropertyIsRequired(definition.Alias);
                    throw new ErrorException(error);
                }

                if (string.IsNullOrEmpty(propertyItem.Value))
                {
                    var error = Domain.Errors.AdditionalProperties.RequiredAdditionalPropertyIsEmpty(
                        definition.Alias);
                    throw new ErrorException(error);
                }
            }

            if (definition.IsUnique && !string.IsNullOrEmpty(propertyItem.Value))
            {
                var query = new IsAdditionalPropertyValueUniqueQuery(
                    tenantId,
                    entityType,
                    entityId,
                    AdditionalPropertyDefinitionType.Text,
                    definition.Id,
                    propertyItem.Value);
                var isUnique = await this.mediator.Send(query);

                if (!isUnique)
                {
                    // The property value is not unique; it's already in use.
                    var error = Errors.AdditionalProperties.UniqueAdditionalPropertyValueAlreadyUsed(
                        entityType,
                        definition.Alias,
                        propertyItem.Value);
                    throw new ErrorException(error);
                }
            }
        }
    }

    private List<AdditionalPropertyValueUpsertModel> StandardizeAdditionalPropertyStructuredDataValues(
        List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels)
    {
        if (additionalPropertyValueIdPropertyTypeModels == null)
        {
            return new List<AdditionalPropertyValueUpsertModel>();
        }

        // If the value is structured data, standardize it to JSON.
        // This is necessary because the value is stored as a string in the database.
        // During checking if the value is unique, the value is also standardized to JSON.
        // So we need to standardize it here to ensure that the value is unique.
        // If the value is not structured data, it will be left as is.
        foreach (var valueItems in additionalPropertyValueIdPropertyTypeModels)
        {
            if (valueItems.Type == AdditionalPropertyDefinitionType.StructuredData)
            {
                valueItems.Value = JsonHelper.StandardizeJson(valueItems.Value);
            }
        }

        return additionalPropertyValueIdPropertyTypeModels;
    }

    private async Task<string> UpsertForClaimVersionThenReturnClaimNumberWithVersion(
        Guid tenantId,
        DeploymentEnvironment environment,
        List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels,
        Guid id)
    {
        var claimVersion = this.claimVersionReadModelRepository
            .GetClaimVersionWithRelatedEntities(tenantId, environment, id, Enumerable.Empty<string>());
        var claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimVersion.ClaimVersion.AggregateId);
        await this.UpsertAdditionalPropertyValues(
            claimAggregate.TenantId,
            additionalPropertyValueIdPropertyTypeModels,
            id,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.GetCurrentInstant(),
            claimAggregate);
        await this.claimAggregateRepository.Save(claimAggregate);
        return $"{claimAggregate.Claim.ClaimReference}-{claimVersion.ClaimVersion.ClaimVersionNumber}";
    }

    private async Task<string> UpsertForClaimThenReturnClaimNumber(
        Guid tenantId, Guid id, List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels)
    {
        var claimAggregate = this.claimAggregateRepository.GetById(tenantId, id);
        await this.UpsertAdditionalPropertyValues(
            claimAggregate.TenantId,
            additionalPropertyValueIdPropertyTypeModels,
            id,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.GetCurrentInstant(),
            claimAggregate);
        await this.claimAggregateRepository.Save(claimAggregate);
        return claimAggregate.Claim.ClaimReference;
    }

    private async Task<string> UpsertPropertiesForPolicyTransactionThenReturnPolicyNumberWithTransactionType(
        Guid tenantId,
        DeploymentEnvironment environment,
        List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels,
        Guid id)
    {
        var policyTransactionDetails = this.policyTransactionReadModelRepository
            .GetPolicyTransactionWithRelatedEntities(tenantId, environment, id, Enumerable.Empty<string>());
        var eventTypeSummary = policyTransactionDetails.PolicyTransaction.GetEventTypeSummary();
        var aggregate = this.quoteAggregateRepository.GetById(tenantId, policyTransactionDetails.PolicyTransaction.PolicyId);
        await this.UpsertAdditionalPropertyValues(
            aggregate.TenantId,
            additionalPropertyValueIdPropertyTypeModels,
            id,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.GetCurrentInstant(),
            aggregate);
        await this.quoteAggregateRepository.Save(aggregate);
        return $"{aggregate.Policy.PolicyNumber} {eventTypeSummary.ToLowerInvariant()} transaction";
    }

    private async Task<string> UpsertPropertiesForPolicyThenReturnPolicyNumber(
        Guid tenantId, Guid id, List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels)
    {
        var quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, id);
        await this.UpsertAdditionalPropertyValues(
            quoteAggregate.TenantId,
            additionalPropertyValueIdPropertyTypeModels,
            id,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.GetCurrentInstant(),
            quoteAggregate);
        await this.quoteAggregateRepository.Save(quoteAggregate);
        return quoteAggregate.Policy.PolicyNumber;
    }

    private async Task<string> UpsertPropertiesForQuoteThenReturnTheQuoteNumber(
        Guid tenantId,
        Guid id,
        List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels)
    {
        var aggregateId = this.quoteReadModelRepository.GetQuoteAggregateId(id);
        var quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, aggregateId.Value);
        await this.UpsertAdditionalPropertyValues(
            quoteAggregate.TenantId,
            additionalPropertyValueIdPropertyTypeModels,
            id,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.GetCurrentInstant(),
            quoteAggregate);
        await this.quoteAggregateRepository.Save(quoteAggregate);
        var quote = quoteAggregate.GetQuoteOrThrow(id);
        return quote.QuoteNumber;
    }

    private async Task<string> UpsertPropertiesForQuoteVersionThenReturnTheQuoteNumber(
        Guid tenantId,
        List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels,
        Guid id,
        DeploymentEnvironment environment)
    {
        var version = this.quoteVersionReadModelRepository.GetQuoteVersionWithRelatedEntities(
            tenantId, environment, id, Enumerable.Empty<string>());
        var quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, version.QuoteVersion.AggregateId);
        await this.UpsertAdditionalPropertyValues(
            quoteAggregate.TenantId,
            additionalPropertyValueIdPropertyTypeModels,
            id,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.GetCurrentInstant(),
            quoteAggregate);
        await this.quoteAggregateRepository.Save(quoteAggregate);
        var quote = quoteAggregate.GetQuoteOrThrow(version.QuoteVersion.QuoteId);
        return $"{quote.QuoteNumber}-{version.QuoteVersion.QuoteVersionNumber}";
    }

    private async Task<string> UpsertCustomerThenReturnFullName(
        Guid tenantId,
        Guid id,
        List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels)
    {
        var customerAggregate = this.customerAggregateRepository.GetById(tenantId, id);
        await this.UpsertAdditionalPropertyValues(
            customerAggregate.TenantId,
            additionalPropertyValueIdPropertyTypeModels,
            id,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.GetCurrentInstant(),
            customerAggregate);
        await this.customerAggregateRepository.Save(customerAggregate);

        var customer = this.customerReadModelRepository.GetCustomerById(tenantId, customerAggregate.Id);
        return customer.FullName;
    }

    private async Task UpsertPropertiesForUser(
        Guid tenantId,
        Guid id,
        List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels)
    {
        var userAggregate = this.userAggregateRepository.GetById(tenantId, id);
        await this.UpsertAdditionalPropertyValues(
            userAggregate.TenantId,
            additionalPropertyValueIdPropertyTypeModels,
            id,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.GetCurrentInstant(),
            userAggregate);
        await this.userAggregateRepository.Save(userAggregate);
    }

    private async Task UpsertPropertiesForOrganisation(
        Guid tenantId,
        Guid id,
        List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels)
    {
        var orgAggregate = this.organisationAggregateRepository.GetById(tenantId, id);
        await this.UpsertAdditionalPropertyValues(
            orgAggregate.TenantId,
            additionalPropertyValueIdPropertyTypeModels,
            id,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.GetCurrentInstant(),
            orgAggregate);
        await this.organisationAggregateRepository.Save(orgAggregate);
    }

    private async Task UpsertAdditionalPropertyValues(
        Guid tenantId,
        List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels,
        Guid entityId,
        Guid? performingUserId,
        Instant createdTimestamp,
        IAdditionalPropertyValueEntityAggregate entityAggregate)
    {
        if (additionalPropertyValueIdPropertyTypeModels == null)
        {
            return;
        }

        foreach (var valueTypeIdModel in additionalPropertyValueIdPropertyTypeModels)
        {
            var evaluator = this.propertyEvaluatorService.GeneratePropertyTypeValueProcessorBasedOnPropertyType(
                valueTypeIdModel.Type);
            var additionalPropertyValueReadModel = await evaluator.GetAdditionalPropertyValue(
                tenantId, valueTypeIdModel.DefinitionId, entityId);
            if (additionalPropertyValueReadModel != null && additionalPropertyValueReadModel.Id.HasValue)
            {
                entityAggregate.UpdateAdditionalPropertyValue(
                    tenantId,
                    entityId,
                    valueTypeIdModel.Type,
                    additionalPropertyValueReadModel.AdditionalPropertyDefinition.Id,
                    additionalPropertyValueReadModel.Id.Value,
                    valueTypeIdModel.Value,
                    performingUserId,
                    createdTimestamp);
            }
            else
            {
                entityAggregate.AddAdditionalPropertyValue(
                    tenantId,
                    entityId,
                    valueTypeIdModel.DefinitionId,
                    valueTypeIdModel.Value,
                    valueTypeIdModel.Type,
                    performingUserId,
                    createdTimestamp);
            }
        }
    }
}

// <copyright file="IAdditionalPropertyValueService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyValue
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Contract template for additional property value service.
    /// </summary>
    public interface IAdditionalPropertyValueService
    {
        /// <summary>
        /// Gets the additional property values for all related eav tables.
        /// </summary>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="entityType">Entity type.</param>
        /// <param name="entityId">The primary id of the particular entity.</param>
        /// <returns>List of additional property values <see cref="AdditionalPropertyValueDto"/>.</returns>
        Task<List<AdditionalPropertyValueDto>> GetAdditionalPropertyValuesByEntityTypeAndEntityId(
            Guid tenantId, AdditionalPropertyEntityType entityType, Guid entityId);

        /// <summary>
        /// Apply the set values defined by the portal user. This is applicable for entities which has an edit form.
        /// </summary>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="entityId">The primary id of an entity.</param>
        /// <param name="additionalPropertyValueIdModels">Properties model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpsertSetAdditionalPropertyValuesForNonAggregateEntity(
            Guid tenantId,
            Guid entityId,
            List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdModels);

        /// <summary>
        /// Set a value for the given definition on the specified entity.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateAdditionalPropertyValueForEntity(
            Guid tenantId,
            Guid entityId,
            DeploymentEnvironment environment,
            AdditionalPropertyDefinitionReadModel definition,
            string newValue);

        /// <summary>
        /// Updates or adds additional property value for <see cref="Organisation"/>.
        /// </summary>
        /// <param name="properties">List of additional property values set in the web domain.</param>
        /// <param name="organisationAggregate">Instance of <see cref="Organisation"/>.</param>
        /// <param name="performingUserId">The ID of the performing user.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        Task UpsertValuesForOrganisation(
            List<AdditionalPropertyValueUpsertModel> properties,
            Organisation organisationAggregate,
            Guid? performingUserId,
            Instant createdTimestamp);

        /// <summary>
        /// Adds default values for customer from edit form.
        /// </summary>
        /// <param name="customerAggregate">Instance of <see cref="CustomerAggregate"/>.</param>
        /// <param name="additionalPropertyValueIdPropertyTypeModels">List of additional property and the values
        /// that usually comes from the edit form. It is set to null just in case the creation of customer was invoked
        /// from the web form app.
        /// </param>
        /// <param name="performingUserId">The ID of the user.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        Task UpsertValuesForCustomer(
            CustomerAggregate customerAggregate,
            List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels,
            Guid? performingUserId,
            Instant createdTimestamp);

        /// <summary>
        /// Adds default values for customer from edit form via person aggregate.
        /// </summary>
        /// <param name="customerId">ID of the customer entity.</param>
        /// <param name="personAggregate">Instance of <see cref="PersonAggregate"/>.</param>
        /// <param name="additionalPropertyValueIdPropertyTypeModels">List of additional property and the values
        /// that usually comes from the edit form. It is set to null just in case the creation of customer was invoked
        /// from the web form app.
        /// </param>
        /// <param name="performingUserId">The ID of the user.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        Task UpsertValuesForCustomerFromPerson(
            Guid customerId,
            PersonAggregate personAggregate,
            List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels,
            Guid? performingUserId,
            Instant createdTimestamp);

        /// <summary>
        /// Add or update additional property values associated with the <see cref="UserAggregate"/>.
        /// </summary>
        /// <param name="properties">List of additional property values set for <see cref="UserAggregate"/>.</param>
        /// <param name="performingUserId">The ID of the performing user.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        /// <param name="userAggregate">Instance of <see cref="UserAggregate"/>.</param>
        Task UpsertValuesForUser(
            List<AdditionalPropertyValueUpsertModel> properties,
            Guid? performingUserId,
            Instant createdTimestamp,
            UserAggregate userAggregate);

        /// <summary>
        /// Gets the list of <see cref="AdditionalPropertyValueDto"/> by query model.
        /// </summary>
        /// <param name="additionalPropertyDefinitionId">ID of the additional property definition.</param>
        /// <param name="value">updated value set by the user.</param>
        /// <param name="entityId">The ID of an entity.</param>
        /// <param name="propertyDefinitionType"><see cref="AdditionalPropertyDefinitionType"/>.</param>
        /// <param name="tenantId">Tenants Id.</param>
        /// <returns>List of <see cref="AdditionalPropertyValueDto"/>.</returns>
        Task<List<AdditionalPropertyValueDto>> GetAdditionalPropertyValuesByQueryModel(
            Guid tenantId,
            Guid? additionalPropertyDefinitionId,
            string value,
            Guid entityId,
            AdditionalPropertyDefinitionType propertyDefinitionType,
            AdditionalPropertyEntityType entityType);

        /// <summary>
        /// Update the list of additional property values which are associated to an aggregate.
        /// This is being invoked or called from entities which has its own additional property value form such as
        /// claim, policy, policy transaction, quote, quote version and claim version.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="entityType"><see cref="AdditionalPropertyEntityType"/>.</param>
        /// <param name="upsertModel">List of instance that implements
        /// <see cref="AdditionalPropertyValueUpsertModel"/>.</param>
        /// <param name="id">The entity Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<string> UpdatePropertiesOfEntityAggregate(
            Guid tenantId,
            AdditionalPropertyEntityType entityType,
            List<AdditionalPropertyValueUpsertModel> upsertModel,
            Guid id,
            DeploymentEnvironment environment);

        /// <summary>
        /// Upserts the additional properties to a policy.
        /// Note: this doesnt save the changes, an external call should save changes.
        /// </summary>
        /// <param name="quoteAggregate">The quote aggregate.</param>
        /// <param name="additionalPropertyValueIdPropertyTypeModels">The additional property models.</param>
        /// <returns>Returns a policy number.</returns>
        Task<string> UpsertPropertiesForPolicyThenReturnPolicyNumber(
            QuoteAggregate quoteAggregate, List<AdditionalPropertyValueUpsertModel> additionalPropertyValueIdPropertyTypeModels);
    }
}

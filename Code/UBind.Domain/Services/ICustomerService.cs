// <copyright file="ICustomerService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;

    /// <summary>
    /// Service for serving and manipulating customer and customer-related models.
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Retrieves the customer aggregate associated to the customer ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The ID of the customer to be retrieved.</param>
        /// <returns>The customer aggregate with the given ID, if any, otherwise null.</returns>
        CustomerAggregate? GetCustomerAggregateById(Guid tenantId, Guid id);

        /// <summary>
        /// Retrieves the customer record associated to the customer ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The ID of the customer to be retrieved.</param>
        /// <returns>The customer read model with the given ID, if any, otherwise null.</returns>
        ICustomerReadModelSummary GetCustomerById(Guid tenantId, Guid id);

        /// <summary>
        /// Gets/retrieves the related persons of a given customer by customer ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="customerId">The ID of the customer.</param>
        /// <returns>A list of customer related people.</returns>
        IEnumerable<IPersonReadModelSummary> GetPersonsForCustomer(Guid tenantId, Guid customerId);

        /// <summary>
        /// Creates a new customer for a new person.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant of the customer.</param>
        /// <param name="environment">The environment for which the customer belongs to.</param>
        /// <param name="customerDetails">The personal details for the customer.</param>
        /// <param name="ownerId">The ID of the customer referrer if any, otherwise default.</param>
        /// <param name="portalId">The ID of the portal the customer would be expected to login to,
        /// if they end up having a user account.</param>
        /// <param name="isTestData">A value indicating whether to return a test data.</param>
        /// <param name="additionalProperties">Collection of additional property values.</param>
        /// <returns>A new customer aggregate.</returns>
        Task<CustomerAggregate> CreateCustomerForNewPerson(
            Guid tenantId,
            DeploymentEnvironment environment,
            IPersonalDetails customerDetails,
            Guid? ownerId,
            Guid? portalId,
            bool isTestData = false,
            List<AdditionalPropertyValueUpsertModel>? additionalProperties = null);

        /// <summary>
        /// Creates a new customer for an existing person.
        /// </summary>
        /// <param name="person">The person the customer refers to.</param>
        /// <param name="environment">The data environment the customer is associated with.</param>
        /// <param name="ownerId">The ID of the customer referrer if any, otherwise default.</param>
        /// <param name="portalId">The ID of the portal the customer would be expected to login to,
        /// if they end up having a user account.</param>
        /// <param name="isTestData">A value indicating whether to return a test data.</param>
        /// <param name="additionalProperties">List of additional property and the values
        /// that usually comes from the edit form. It is set to null just in case the creation of customer was invoked
        /// from the web form app.</param>
        /// <returns>A new customer aggregate.</returns>
        Task<CustomerAggregate> CreateCustomerForExistingPerson(
            PersonAggregate person,
            DeploymentEnvironment environment,
            Guid? ownerId,
            Guid? portalId,
            bool isTestData = false,
            List<AdditionalPropertyValueUpsertModel>? additionalProperties = null);

        Task UpdateCustomerDetails(
            Guid tenantId,
            Guid customerId,
            IPersonalDetails details,
            Guid? portalId,
            List<AdditionalPropertyValueUpsertModel>? additionalPropertyValueUpsertModels = null);
    }
}

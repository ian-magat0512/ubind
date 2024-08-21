// <copyright file="IPersonService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Person
{
    using System;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Service for serving and manipulating person-related functionality.
    /// </summary>
    public interface IPersonService
    {
        /// <summary>
        /// Retrieves the person record associated to the person ID.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the record.</param>
        /// <param name="id">The ID of the person to be retrieved.</param>
        /// <returns>The person read model record with the given ID, if any, otherwise null.</returns>
        IPersonReadModelSummary Get(Guid tenantId, Guid id);

        /// <summary>
        /// Retrieves the person record associated to the customer ID.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="customerId">The ID of the customer.</param>
        /// <returns>The person read model record related to the given customer id if any, otherwise null.</returns>
        PersonAggregate GetByCustomerId(Guid tenantId, Guid customerId);

        /// <summary>
        /// Create a new person.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="personDetails">The details of the person to be created.</param>
        /// <param name="isTestData">A value indicating if the created user is a test data.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<PersonAggregate> CreateNewPerson(Guid tenantId, IPersonalDetails personDetails, bool isTestData = false);

        /// <summary>
        /// Copies existing persons from customer and user read model tables into the new person read model table .
        /// </summary>
        void RecreateExistingPeopleToPersonReadModelTable();
    }
}

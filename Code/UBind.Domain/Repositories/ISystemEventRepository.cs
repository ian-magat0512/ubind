// <copyright file="ISystemEventRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Events;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Repository for storing event data.
    /// </summary>
    public interface ISystemEventRepository
    {
        /// <summary>
        /// Create the event record and its metadata.
        /// </summary>
        /// <param name="systemEvent">The system event.</param>
        void Insert(SystemEvent systemEvent);

        /// <summary>
        /// Retrieves an event by id.
        /// </summary>
        /// <param name="id">The event id.</param>
        /// <returns>The event.</returns>
        SystemEvent? GetById(Guid id);

        /// <summary>
        /// Checks if the system event exists.
        /// </summary>
        bool Exists(Guid eventId);

        /// <summary>
        /// Get all records.
        /// </summary>
        /// <returns>List of events.</returns>
        IEnumerable<SystemEvent> GetAll();

        /// <summary>
        /// Retrieve relationships for the event.
        /// </summary>
        /// <param name="id">The event id.</param>
        /// <param name="entityType">The entity type to retrieve.</param>
        /// <returns>The associated relationships.</returns>
        IEnumerable<Relationship> GetRelationships(Guid id, EntityType? entityType = null);

        /// <summary>
        /// Creates a query for getting system events with tags that can be further filtered
        /// by automation entity query list providers.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A query which can be further filtered.</returns>
        IQueryable<UBind.Domain.Automation.Event> CreateQueryForAutomations(Guid tenantId);

        /// <summary>
        /// Saves the changes after doing an insert. This needs to be done explicitly, because if the save happens
        /// as part of the insert, is that it will save other objects in the db context, without retries.
        /// </summary>
        void SaveChanges();

        void AddRange(IEnumerable<SystemEvent> systemEvents);

        /// <summary>
        /// Updates top X records without expired ticks epoch value.
        /// Note: this is specifically used for migration associated with renaming date fields.
        /// </summary>
        /// <param name="batch">How many records to update.</param>
        /// <returns>Return rows affected.</returns>
        int UpdateTopWithoutExpiredTicksEpochValue(int batch);

        /// <summary>
        /// Updates top X records without created ticks epoch value.
        /// Note: this is specifically used for migration associated with renaming date fields.
        /// </summary>
        /// <param name="batch">How many records to update.</param>
        /// <returns>Return rows affected.</returns>
        int UpdateTopWithoutCreatedTicksEpochValue(int batch);

        /// <summary>
        /// Gets the system event with related entities or the eventId supplied.
        /// </summary>
        /// <param name="tenantId">The tenant.</param>
        /// <param name="eventId">The eventId.</param>
        /// <param name="relatedEntities">The related entities to the event.</param>
        /// <returns>The system event with related entites.</returns>
        ISystemEventWithRelatedEntities? GetSystemEventWithRelatedEntities(Guid tenantId, Guid eventId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Creates a  query for the system event with related entities or the eventId supplied.
        /// </summary>
        /// <param name="tenantId">The tenant.</param>
        /// <param name="relatedEntities">The related entities to the event.</param>
        /// <returns>The system event with related entites.</returns>
        IQueryable<ISystemEventWithRelatedEntities> CreateQueryForSystemEventDetailsWithRelatedEntities(Guid tenantId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Checks if the system event exists.
        /// </summary>
        /// <param name="tenantId">The tenant.</param>
        /// <param name="id">The event id.</param>
        bool Exists(Guid tenantId, Guid id);
    }
}

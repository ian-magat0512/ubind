// <copyright file="ISmsRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Sms;
    using UBind.Domain.ReadWriteModel;

    public interface ISmsRepository : IRepository
    {
        /// <summary>
        /// Get the sms messages.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="filter">The entity filter.</param>
        /// <returns>List of sms messages.</returns>
        IEnumerable<Sms> GetAll(Guid tenantId, EntityListFilters filter);

        /// <summary>
        /// Insert new sms into the database.
        /// </summary>
        /// <param name="sms">The sms.</param>
        void Insert(Sms sms);

        /// <summary>
        /// Gets the sms by id.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="smsId">The sms id.</param>
        /// <returns>The sms.</returns>
        ISmsDetails GetById(Guid tenantId, Guid smsId);

        /// <summary>
        /// Insert tags related to sms.
        /// </summary>
        /// <param name="tag">The tag entity.</param>
        void InsertSmsTag(Tag tag);

        /// <summary>
        /// Get the sms relationships.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="smsId">The sms id.</param>
        /// <param name="entityType">The entity type.</param>
        /// <returns>List of relationships.</returns>
        IEnumerable<Relationship> GetSmsRelationships(Guid tenantId, Guid smsId, EntityType? entityType = null);

        /// <summary>
        /// Insert relationship of sms.
        /// </summary>
        /// <param name="relationship">The relationship entity.</param>
        void InsertSmsRelationship(Relationship relationship);

        /// <summary>
        /// Remove the sms relationship.
        /// </summary>
        /// <param name="relationship">The relationship entity.</param>
        void RemoveSmsRelationship(Relationship relationship);

        /// <summary>
        /// Gets the sms and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="smsId">The sms id.</param>
        /// <param name="includeProperties">The included properties.</param>
        /// <returns>The sms.</returns>
        ISmsReadModelWithRelatedEntities GetSmsWithRelatedEntities(Guid tenantId, Guid smsId, List<string> includeProperties);

        /// <summary>
        /// Check wether the sms exists.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="smsId">The smsId.</param>
        /// <returns>The value wether the sms exists.</returns>
        bool DoesSmsExists(Guid tenantId, Guid smsId);

        /// <summary>
        /// Method for creating IQueryable method that retrieve sms and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="includedProperties">The related entities to retrieve.</param>
        /// <returns>IQueryable for emails.</returns>
        IQueryable<ISmsReadModelWithRelatedEntities> CreateQueryForSmsDetailsWithRelatedEntities(
            Guid tenantId, IEnumerable<string> includedProperties);

        /// <summary>
        /// Gets all SMS within the tenant, matching a given filter.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>All the SMS owned by a given user and matching a given filter.</returns>
        IEnumerable<ISmsReadModelWithRelatedEntities> GetSmsListWithRelatedEntities(
            Guid tenantId, EntityListFilters filters, IEnumerable<string> relatedEntities);
    }
}

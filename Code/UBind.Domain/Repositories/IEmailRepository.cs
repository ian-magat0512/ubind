// <copyright file="IEmailRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Email;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReadWriteModel.Email;

    /// <summary>
    /// Repository for storing emails.
    /// </summary>
    public interface IEmailRepository : IRepository
    {
        /// <summary>
        /// Create the relationship from entity to the email.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="fromEntityType">The from entity type.</param>
        /// <param name="fromEntityId">The from entity id.</param>
        /// <param name="relationshipType">The relationship type.</param>
        /// <param name="emailId">The email id.</param>
        void CreateRelationshipFromEntityToEmail(
            Guid tenantId, EntityType fromEntityType, Guid fromEntityId, RelationshipType relationshipType, Guid emailId);

        /// <summary>
        /// Create the relationships from email to the entity.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="emailId">The email id.</param>
        /// <param name="type">The type of the relationship.</param>
        /// <param name="toEntitytype">The to entity type.</param>
        /// <param name="toEntityId">The to enttiy id.</param>
        void CreateRelationshipFromEmailToEntity(
            Guid tenantId, Guid emailId, RelationshipType type, EntityType toEntitytype, Guid toEntityId);

        /// <summary>
        /// Retrieve all quote email summaries from the repository.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Filters to apply.</param>
        /// <param name="overrideIncludeNonEnvironmentSpecificData">overrides Includes non environment specific data filter.</param>
        /// <returns>All the quote emails in the repository.</returns>
        IEnumerable<IEmailSummary> GetSummaries(
            Guid tenantId, EntityListFilters filters, bool? overrideIncludeNonEnvironmentSpecificData = null);

        /// <summary>
        /// Retrieve all quote emails from the repository.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Filters to apply.</param>
        /// <param name="overrideIncludeNonEnvironmentSpecificData">overrides Includes non environment specific data filter.</param>
        /// <returns>All the quote emails in the repository.</returns>
        IEnumerable<Email> GetAll(
            Guid tenantId, EntityListFilters filters, bool? overrideIncludeNonEnvironmentSpecificData = null);

        /// <summary>
        /// Check whether the sms exists.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="emailId">The smsId.</param>
        /// <returns>The value wether the sms exists.</returns>
        bool DoesEmailExists(Guid tenantId, Guid emailId);

        /// <summary>
        /// Retrieve all policy emails from the repository by quote Id.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Filters to apply.</param>
        /// <returns>All the quote emails in the repository.</returns>
        IEnumerable<Email> GetByPolicyId(
            Guid tenantId, Guid policyId, EntityListFilters filters);

        /// <summary>
        /// Retrieve all quote emails from the repository by quote Id.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Filters to apply.</param>
        /// <returns>All the quote emails in the repository.</returns>
        IEnumerable<Email> GetByQuoteId(
            Guid tenantId, Guid quoteId, EntityListFilters filters);

        /// <summary>
        /// Retrieve all claim emails from the repository by claim Id.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Filters to apply.</param>
        /// <returns>All the quote emails in the repository.</returns>
        IEnumerable<Email> GetByClaimId(
            Guid tenantId, Guid claimId, EntityListFilters filters);

        /// <summary>
        /// Retrieve all policy transaction emails from the repository by policy transaction Id.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Filters to apply.</param>
        /// <returns>All the quote emails in the repository.</returns>
        IEnumerable<Email> GetByPolicyTransactionId(
            Guid tenantId,
            Guid policyTransctionId,
            EntityListFilters filters);

        /// <summary>
        /// Retrieve all quote version emails from the repository by quote version Id.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Filters to apply.</param>
        /// <returns>All the quote emails in the repository.</returns>
        IEnumerable<Email> GetByQuoteVersionId(
            Guid tenantId,
            Guid quoteVersionId,
            EntityListFilters filters);

        /// <summary>
        /// Retrieve all quote emails from the repository by customer id.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>All the quote emails in the repository.</returns>
        IEnumerable<Email> GetEmailsForCustomer(
            Guid tenantId,
            Guid customerId,
            EntityListFilters filters);

        /// <summary>
        /// Retrieve all quote emails from the repository by customer id.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Filters to apply.</param>
        /// <returns>All the quote emails in the repository.</returns>
        IEnumerable<Email> GetByUserId(
            Guid tenantId, Guid userId, EntityListFilters filters);

        /// <summary>
        /// Retrieve the quote email model based on its unique identifier.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The unique identifier for quote email model.</param>
        /// <returns>The quote email in the repository.</returns>
        Email GetById(Guid tenantId, Guid id);

        /// <summary>
        /// Retrieve the quote email model based on its unique identifier with emailAttachments.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="id">The unique identifier for quote email model.</param>
        /// <returns>The quote email in the repository.</returns>
        Email GetWithFilesById(Guid tenantId, Guid id);

        /// <summary>
        /// Insert the email and all of its connected relationships and tags into the database.
        /// </summary>
        /// <param name="metadata">The email metadata.</param>
        void InsertEmailAndMetadata(EmailAndMetadata metadata);

        /// <summary>
        /// Retrieves the relationships of a specific type.
        /// Retrieve the system emails.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="fromTimestamp">The from date.</param>
        /// <param name="toTimestamp">The to date.</param>
        /// <returns>The the system emails.</returns>
        IEnumerable<IEmailDetails> GetSystemEmailForReport(
            Guid tenantId,
            Guid organisationId,
            DeploymentEnvironment environment,
            Instant? fromTimestamp,
            Instant? toTimestamp);

        /// <summary>
        /// Retrieve the product emails.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="productIds">The product ids.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="fromTimestamp">The from date.</param>
        /// <param name="toTimestamp">The to date.</param>
        /// <param name="includeTestData">The value indicating whether the data is for testing.</param>
        /// <returns>The the system emails.</returns>
        IEnumerable<IEmailDetails> GetProductEmailForReport(
            Guid tenantId,
            Guid organisationId,
            IEnumerable<Guid> productIds,
            DeploymentEnvironment environment,
            Instant fromTimestamp,
            Instant toTimestamp,
            bool includeTestData);

        /// <summary>
        /// Insert a new quote email model in the repository.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="emailId">The email id.</param>
        /// <param name="entityType">The entity type to retrieve.</param>
        /// <returns>A list of relationships to the email.</returns>
        IEnumerable<Relationship> GetRelationships(Guid tenantId, Guid emailId, EntityType? entityType = null);

        /// <summary>
        /// get the tags of a specific email.
        /// Note: this was used in a migration.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="id">email id.</param>
        /// <returns>the tags of an email.</returns>
        IEnumerable<Tag> GetTags(Guid tenantId, Guid id);

        /// <summary>
        /// Checks if a specific email has an attachment.
        /// Note: this was used in a migration.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="id">email id.</param>
        /// <returns>True if the email has an attachment.</returns>
        bool CheckIfHasAttachments(Guid tenantId, Guid id);

        /// <summary>
        /// get detail projection via email id with attachment.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="id">email id.</param>
        /// <returns>the email details.</returns>
        IEmailDetails GetEmailDetailsWithAttachments(Guid tenantId, Guid id);

        /// <summary>
        /// Gets email with related entities by id.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="id">The email id.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The email with related entities.</returns>
        IEmailReadModelWithRelatedEntities GetEmailWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, Guid id, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Method for creating IQueryable method that retrieve emails and its related entities.
        /// </summary>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for emails.</returns>
        IQueryable<EmailReadModelWithRelatedEntities> CreateQueryForEmailDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Saves the changes to the db.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Gets all emails within the tenant, matching a given filter.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>All the emails owned by a given user and matching a given filter.</returns>
        IEnumerable<EmailReadModelWithRelatedEntities> GetEmailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, EntityListFilters filters, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Insert the email relationship.
        /// </summary>
        /// <param name="relationship">The relationship to insert to the database.</param>
        void InsertEmailRelationship(Relationship relationship);

        /// <summary>
        /// Remove the email relationship.
        /// </summary>
        /// <param name="relationship">The relationship to insert to the database.</param>
        void RemoveEmailRelationship(Relationship relationship);

        /// <summary>
        /// Deletes all emails and attachments associated with an entity.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="environment">The deployment environment of the customer.</param>
        /// <param name="entityType">The entity type.</param>
        /// <param name="entitiyId">The entity's Id.</param>
        void DeleteEmailsAndAttachmentsForEntity(
            Guid tenantId,
            DeploymentEnvironment environment,
            EntityType entityType,
            Guid entitiyId);
    }
}

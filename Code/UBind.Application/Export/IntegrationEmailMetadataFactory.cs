// <copyright file="IntegrationEmailMetadataFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Factory
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.Quote;
    using UBind.Domain.ReadWriteModel.Email;
    using UBind.Domain.Repositories;

    /// <summary>
    /// The integration email factory that will get all metadata about an email, that includes relationships and tags.
    /// </summary>
    public static class IntegrationEmailMetadataFactory
    {
        /// <summary>
        /// Create a quot email for the user.
        /// </summary>
        /// <param name="emailModel">The email model.</param>
        /// <param name="policyId">The policyId related with that email.</param>
        /// <param name="quoteId">The quoteId related with that email.</param>
        /// <param name="quoteVersionId">The quoteVersionId related with that email.</param>
        /// <param name="organisationId">The organisationId related with that email.</param>
        /// <param name="customerId">The customerId related with that email.</param>
        /// <param name="personId">The personId related with that email.</param>
        /// <param name="emailType">The email type.</param>
        /// <param name="timestamp">The current timestamp.</param>
        /// <returns>The email metadata.</returns>
        public static EmailAndMetadata CreateForQuote(
            EmailModel emailModel,
            Guid policyId,
            Guid quoteId,
            Guid quoteVersionId,
            Guid organisationId,
            Guid? customerId,
            Guid personId,
            EmailType emailType,
            Instant timestamp,
            IFileContentRepository fileContentRepository)
        {
            var tags = new List<string>();
            tags.Add(DefaultEmailTags.Quote);
            EmailAndMetadata metadata = new EmailAndMetadata(emailModel, timestamp, fileContentRepository);
            metadata.CreateRelationshipFromEntityToEmail(EntityType.Quote, quoteId, RelationshipType.QuoteMessage);

            if (policyId != default)
            {
                metadata.CreateRelationshipFromEntityToEmail(EntityType.Policy, policyId, RelationshipType.PolicyMessage);
            }

            if (quoteVersionId != default)
            {
                metadata.CreateRelationshipFromEntityToEmail(EntityType.QuoteVersion, quoteVersionId, RelationshipType.QuoteVersionMessage);
            }

            if (organisationId != default)
            {
                metadata.CreateRelationshipFromEmailToEntity(RelationshipType.MessageSender, EntityType.Organisation, organisationId);
            }

            if (personId != default)
            {
                metadata.CreateRelationshipFromEmailToEntity(RelationshipType.MessageRecipient, EntityType.Person, personId);
            }

            if (customerId != null && customerId != default)
            {
                metadata.CreateRelationshipFromEntityToEmail(EntityType.Customer, customerId.Value, RelationshipType.CustomerMessage);
            }

            metadata.CreateUserDefinedTags(tags);
            metadata.CreateTagFromEnvironment(emailModel.Environment);
            metadata.CreateTagFromEmailType(emailType);
            return metadata;
        }

        /// <summary>
        /// Create an policy email for the user.
        /// </summary>
        /// <param name="emailModel">The email model.</param>
        /// <param name="policyId">The policyId related with that email.</param>
        /// <param name="quoteId">The quoteId related with that email.</param>
        /// <param name="policyTransactionId">The transactionId related with that email.</param>
        /// <param name="quoteVersionId">The quoteVersionId related with that email.</param>
        /// <param name="organisationId">The organisationId related with that email.</param>
        /// <param name="customerId">The customerId related with that email.</param>
        /// <param name="personId">The personId related with that email.</param>
        /// <param name="applicationEventType">The application event type to convert to tag.</param>
        /// <param name="emailType">The email type.</param>
        /// <param name="timestamp">The current timestamp.</param>
        /// <returns>The email metadata.</returns>
        public static EmailAndMetadata CreateForPolicy(
            EmailModel emailModel,
            Guid policyId,
            Guid? quoteId,
            Guid policyTransactionId,
            Guid quoteVersionId,
            Guid organisationId,
            Guid? customerId,
            Guid personId,
            ApplicationEventType applicationEventType,
            EmailType emailType,
            Instant timestamp,
            IFileContentRepository fileContentRepository)
        {
            var tags = new List<string>();
            tags.Add(DefaultEmailTags.Policy);
            tags.Add(GetEventTag(applicationEventType));

            EmailAndMetadata metadata = new EmailAndMetadata(emailModel, timestamp, fileContentRepository);
            metadata.CreateRelationshipFromEntityToEmail(EntityType.Policy, policyId, RelationshipType.PolicyMessage);
            metadata.CreateRelationshipFromEntityToEmail(EntityType.Quote, quoteId, RelationshipType.QuoteMessage);
            metadata.CreateRelationshipFromEntityToEmail(EntityType.PolicyTransaction, policyTransactionId, RelationshipType.PolicyTransactionMessage);

            if (quoteVersionId != default)
            {
                metadata.CreateRelationshipFromEntityToEmail(EntityType.QuoteVersion, quoteVersionId, RelationshipType.QuoteVersionMessage);
            }

            if (organisationId != default)
            {
                metadata.CreateRelationshipFromEmailToEntity(RelationshipType.MessageSender, EntityType.Organisation, organisationId);
            }

            if (personId != default)
            {
                metadata.CreateRelationshipFromEmailToEntity(
                 RelationshipType.MessageRecipient, EntityType.Person, personId);
            }

            if (customerId != null && customerId != default)
            {
                metadata.CreateRelationshipFromEntityToEmail(
                    Domain.EntityType.Customer, customerId.Value, RelationshipType.CustomerMessage);
            }

            metadata.CreateUserDefinedTags(tags);
            metadata.CreateTagFromEnvironment(emailModel.Environment);
            metadata.CreateTagFromEmailType(emailType);
            return metadata;
        }

        /// <summary>
        /// Create an email for renewal invitation.
        /// </summary>
        /// <param name="emailModel">The email model.</param>
        /// <param name="policyId">The policyId related with that email.</param>
        /// <param name="quoteId">The quoteId related with that email.</param>
        /// <param name="policyTransactionId">The policyTransactionId related with that email.</param>
        /// <param name="organisationId">The senderUserId related with that email.</param>
        /// <param name="customerId">The customerId related with that email.</param>
        /// <param name="personId">The personId related with that email.</param>
        /// <param name="emailType">The email type.</param>
        /// <param name="timestamp">The current timestamp.</param>
        /// <returns>The email metadata.</returns>
        public static EmailAndMetadata CreateForRenewalInvitation(
            EmailModel emailModel,
            Guid policyId,
            Guid? quoteId,
            Guid policyTransactionId,
            Guid organisationId,
            Guid? customerId,
            Guid personId,
            EmailType emailType,
            Instant timestamp,
            IFileContentRepository fileContentRepository)
        {
            var tags = new List<string>();
            tags.Add(DefaultEmailTags.Policy);
            tags.Add(DefaultEmailTags.Renewal);
            tags.Add(DefaultEmailTags.Invitation);

            EmailAndMetadata metadata = new EmailAndMetadata(emailModel, timestamp, fileContentRepository);
            metadata.CreateRelationshipFromEntityToEmail(EntityType.Policy, policyId, RelationshipType.PolicyMessage);
            metadata.CreateRelationshipFromEntityToEmail(EntityType.PolicyTransaction, policyTransactionId, RelationshipType.PolicyTransactionMessage);

            if (quoteId.HasValue)
            {
                metadata.CreateRelationshipFromEntityToEmail(EntityType.Quote, quoteId.Value, RelationshipType.QuoteMessage);
            }

            if (organisationId != default)
            {
                metadata.CreateRelationshipFromEmailToEntity(RelationshipType.MessageSender, EntityType.Organisation, organisationId);
            }

            if (personId != default)
            {
                metadata.CreateRelationshipFromEmailToEntity(RelationshipType.MessageRecipient, EntityType.Person, personId);
            }

            if (customerId != null && customerId.Value != default)
            {
                metadata.CreateRelationshipFromEntityToEmail(Domain.EntityType.Customer, customerId.Value, RelationshipType.CustomerMessage);
            }

            metadata.CreateUserDefinedTags(tags);
            metadata.CreateTagFromEnvironment(emailModel.Environment);
            metadata.CreateTagFromEmailType(emailType);
            return metadata;
        }

        private static string GetEventTag(ApplicationEventType applicationEventType)
        {
            return applicationEventType == ApplicationEventType.PolicyRenewed ? DefaultEmailTags.Renewal :
                    applicationEventType == ApplicationEventType.PolicyAdjusted ? DefaultEmailTags.Adjustment :
                    applicationEventType == ApplicationEventType.PolicyCancelled ? DefaultEmailTags.Cancellation :
                    applicationEventType == ApplicationEventType.PolicyIssued ? DefaultEmailTags.Purchase : null;
        }
    }
}

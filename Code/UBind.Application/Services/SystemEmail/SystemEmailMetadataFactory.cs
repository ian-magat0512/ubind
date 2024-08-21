// <copyright file="SystemEmailMetadataFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.SystemEmail
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Quote;
    using UBind.Domain.ReadWriteModel.Email;
    using UBind.Domain.Repositories;

    /// <summary>
    /// The system email factory that will get all metadata about an email, that includes relationships and tags.
    /// </summary>
    public static class SystemEmailMetadataFactory
    {
        private static EmailAndMetadata CreateUser(
            EmailModel model,
            Guid userId,
            Guid personId,
            Guid organisationId,
            List<string> tags,
            EmailType emailType,
            Instant timestamp,
            IFileContentRepository fileContentRepository)
        {
            tags = tags ?? new List<string>();

            EmailAndMetadata metadata = new EmailAndMetadata(model, timestamp, fileContentRepository);
            metadata.CreateRelationshipFromEmailToEntity(
                RelationshipType.MessageRecipient, EntityType.Person, personId);
            metadata.CreateRelationshipFromEmailToEntity(
                RelationshipType.MessageSender, EntityType.Organisation, organisationId);
            metadata.CreateRelationshipFromEntityToEmail(EntityType.User, userId, RelationshipType.UserMessage);

            metadata.CreateUserDefinedTags(tags);
            metadata.CreateTagFromEnvironment(model.Environment);
            metadata.CreateTagFromEmailType(emailType);
            return metadata;
        }

        private static EmailAndMetadata CreateCustomer(
            EmailModel model,
            Guid customerId,
            Guid personId,
            Guid organisationId,
            List<string> tags,
            EmailType emailType,
            Instant timestamp,
            IFileContentRepository fileContentRepository)
        {
            tags = tags ?? new List<string>();

            EmailAndMetadata metadata = new EmailAndMetadata(model, timestamp, fileContentRepository);
            metadata.CreateRelationshipFromEmailToEntity(
                RelationshipType.MessageRecipient, EntityType.Person, personId);
            metadata.CreateRelationshipFromEmailToEntity(
                RelationshipType.MessageSender, EntityType.Organisation, organisationId);
            metadata.CreateRelationshipFromEntityToEmail(
                EntityType.Customer, customerId, RelationshipType.CustomerMessage);

            metadata.CreateUserDefinedTags(tags);
            metadata.CreateTagFromEnvironment(model.Environment);
            metadata.CreateTagFromEmailType(emailType);
            return metadata;
        }

        /// <summary>
        /// Class for creating password reset email metadata.
        /// </summary>
        public static class PasswordReset
        {
            /// <summary>
            /// Create an email for user for password reset.
            /// </summary>
            /// <param name="emailModel">The email model.</param>
            /// <param name="userId">The userId related with that email.</param>
            /// <param name="personId">The personId related with that email.</param>
            /// <param name="organisationId">The organisationId related with that email.</param>
            /// <param name="senderUserId">The senderUserId related with that email.</param>
            /// <param name="emailType">The email type.</param>
            /// <param name="timestamp">The current timestamp.</param>
            /// <returns>The email metadata.</returns>
            public static EmailAndMetadata CreateForUser(
                EmailModel emailModel,
                Guid userId,
                Guid personId,
                Guid organisationId,
                EmailType emailType,
                Instant timestamp,
                IFileContentRepository fileContentRepository)
            {
                var tags = new List<string> { DefaultEmailTags.Invitation, DefaultEmailTags.PasswordReset };

                return CreateUser(
                    emailModel, userId, personId, organisationId, tags, emailType, timestamp, fileContentRepository);
            }

            /// <summary>
            /// Create an email for user for password reset.
            /// </summary>
            /// <param name="emailModel">The email model.</param>
            /// <param name="customerId">The customerId related with that email.</param>
            /// <param name="personId">The personId related with that email.</param>
            /// <param name="organisationId">The organisationId related with that email.</param>
            /// <param name="senderUserId">The senderUserId related with that email.</param>
            /// <param name="emailType">The email type.</param>
            /// <param name="timestamp">The current timestamp.</param>
            /// <returns>The email metadata.</returns>
            public static EmailAndMetadata CreateForCustomer(
                EmailModel emailModel,
                Guid customerId,
                Guid personId,
                Guid organisationId,
                EmailType emailType,
                Instant timestamp,
                IFileContentRepository fileContentRepository)
            {
                var tags = new List<string> { DefaultEmailTags.Invitation, DefaultEmailTags.PasswordReset };

                return CreateCustomer(
                    emailModel, customerId, personId, organisationId, tags, emailType, timestamp, fileContentRepository);
            }
        }

        /// <summary>
        /// Class for creating account activation email metadata.
        /// </summary>
        public static class AccountActivation
        {
            /// <summary>
            /// Create an email for user for account activation.
            /// </summary>
            /// <param name="emailModel">The email model.</param>
            /// <param name="userId">The userId related with the email.</param>
            /// <param name="personId">The personId related with the email.</param>
            /// <param name="organisationId">The organisationId related with the email.</param>
            /// <param name="performingUserId">The senderUserId related with the email.</param>
            /// <param name="emailType">The email type.</param>
            /// <param name="timestamp">The current timestamp.</param>
            /// <returns>The email metadata.</returns>
            public static EmailAndMetadata CreateForUser(
                EmailModel emailModel,
                Guid userId,
                Guid personId,
                Guid organisationId,
                EmailType emailType,
                Instant timestamp,
                IFileContentRepository fileContentRepository)
            {
                var tags = new List<string> { DefaultEmailTags.Invitation, DefaultEmailTags.AccountActivation };

                return CreateUser(
                    emailModel, userId, personId, organisationId, tags, emailType, timestamp, fileContentRepository);
            }

            /// <summary>
            /// Create an email for user for account activation.
            /// </summary>
            /// <param name="emailModel">The email model.</param>
            /// <param name="customerId">The customer id of the customer related with that email.</param>
            /// <param name="personId">The person id of the customer related with that email.</param>
            /// <param name="organisationId">The organisation id of the customer related with that email.</param>
            /// <param name="performingUserId">The senderUserId related with that email.</param>
            /// <param name="emailType">The email type.</param>
            /// <param name="timestamp">The current timestamp.</param>
            /// <returns>The email metadata.</returns>
            public static EmailAndMetadata CreateForCustomer(
                EmailModel emailModel,
                Guid customerId,
                Guid personId,
                Guid organisationId,
                EmailType emailType,
                Instant timestamp,
                IFileContentRepository fileContentRepository)
            {
                var tags = new List<string> { DefaultEmailTags.Invitation, DefaultEmailTags.AccountActivation };

                return CreateCustomer(
                    emailModel, customerId, personId, organisationId, tags, emailType, timestamp, fileContentRepository);
            }
        }

        /// <summary>
        /// Quote association.
        /// </summary>
        public static class QuoteAssociation
        {
            /// <summary>
            /// Create an email for quote association.
            /// </summary>
            /// <param name="emailModel">The email model.</param>
            /// <param name="userOrCustomerId">The customerId/userId related with that email.</param>
            /// <param name="personId">The personId related with that email.</param>
            /// <param name="organisationId">The organisationId related with that email.</param>
            /// <param name="quoteId">The quote ID.</param>
            /// <param name="emailType">The email type.</param>
            /// <param name="timestamp">The current timestamp.</param>
            /// <returns>The email metadata.</returns>
            public static EmailAndMetadata Create(
                EmailModel emailModel,
                Guid userOrCustomerId,
                Guid personId,
                Guid organisationId,
                Guid quoteId,
                EmailType emailType,
                Instant timestamp,
                IFileContentRepository fileContentRepository)
            {
                var tags = new List<string> { DefaultEmailTags.Invitation, DefaultEmailTags.QuoteAssociation };

                var metadata = emailType == EmailType.Customer
                    ? CreateCustomer(
                        emailModel, userOrCustomerId, personId, organisationId, tags, emailType, timestamp, fileContentRepository)
                    : CreateUser(
                        emailModel, userOrCustomerId, personId, organisationId, tags, emailType, timestamp, fileContentRepository);

                metadata.CreateRelationshipFromEntityToEmail(EntityType.Quote, quoteId, RelationshipType.QuoteMessage);

                return metadata;
            }
        }
    }
}

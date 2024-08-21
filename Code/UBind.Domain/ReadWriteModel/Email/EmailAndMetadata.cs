// <copyright file="EmailAndMetadata.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel.Email
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.Extensions.Domain;
    using UBind.Domain.Quote;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// The email factory that will set all relationships and tags for the email.
    /// </summary>
    public class EmailAndMetadata
    {
        /// <summary>
        /// Gets or sets the current timestamp.
        /// </summary>
        private Instant timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAndMetadata"/> class.
        /// </summary>
        /// <param name="emailModel">The email model to be transformed into an email entity.</param>
        /// <param name="timestamp">The current timestamp.</param>
        public EmailAndMetadata(EmailModel emailModel, Instant timestamp, IFileContentRepository fileContentRepository)
        {
            var emailAttachments =
                emailModel.Attachments.ToEmailAttachmentEntities(emailModel.TenantId, emailModel.Id, timestamp, fileContentRepository);

            var email = new Email(
              emailModel.TenantId,
              emailModel.OrganisationId,
              emailModel.ProductId,
              emailModel.Environment,
              emailModel.Id,
              emailModel.GetRecipientList(),
              emailModel.From,
              emailModel.GetCCList(),
              emailModel.GetBCCList(),
              emailModel.GetReplyToList(),
              emailModel.Subject,
              emailModel.HtmlBody,
              emailModel.PlainTextBody,
              emailAttachments,
              timestamp);

            this.Email = email;
            this.timestamp = timestamp;

            this.CreateProductRelationship();
        }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public UBind.Domain.ReadWriteModel.Email.Email Email { get; set; }

        /// <summary>
        /// Gets or sets the relationship of the email.
        /// </summary>
        public List<Relationship> Relationships { get; set; } = new List<Relationship>();

        /// <summary>
        /// Gets or sets the tags of the email.
        /// </summary>
        public List<Tag> Tags { get; set; } = new List<Tag>();

        /// <summary>
        /// Create the product relationship.
        /// </summary>
        public void CreateProductRelationship()
        {
            this.Validate();

            if (this.Email.ProductId.HasValue)
            {
                var relationship = new Relationship(
                    this.Email.TenantId,
                    EntityType.Product,
                    this.Email.ProductId.Value,
                    RelationshipType.ProductMessage,
                    Domain.EntityType.Message,
                    this.Email.Id,
                    this.timestamp);
                this.AddRelationship(relationship);
            }
        }

        /// <summary>
        /// Create the relationships from email to the entity.
        /// </summary>
        /// <param name="type">The type of the relationship.</param>
        /// <param name="toEntitytype">The to entity type.</param>
        /// <param name="toEntityId">The to enttiy id.</param>
        public void CreateRelationshipFromEmailToEntity(
            RelationshipType type,
            EntityType toEntitytype,
            System.Guid toEntityId)
        {
            this.Validate();

            var relationship = new Relationship(this.Email.TenantId, EntityType.Message, this.Email.Id, type, toEntitytype, toEntityId, this.timestamp);
            this.AddRelationship(relationship);
        }

        /// <summary>
        /// Create the relationship from entity to the email.
        /// </summary>
        /// <param name="fromEntityType">The from entity type.</param>
        /// <param name="fromEntityId">The from entity id.</param>
        /// <param name="relationshipType">The relationship type.</param>
        public void CreateRelationshipFromEntityToEmail(EntityType fromEntityType, Guid? fromEntityId, RelationshipType relationshipType)
        {
            if (fromEntityId.HasValue)
            {
                this.Validate();

                var relationship = new Relationship(this.Email.TenantId, fromEntityType, fromEntityId.Value, relationshipType, Domain.EntityType.Message, this.Email.Id, this.timestamp);
                this.AddRelationship(relationship);
            }
        }

        /// <summary>
        /// Create user defined tags.
        /// </summary>
        /// <param name="tags">The tags.</param>
        public void CreateUserDefinedTags(IEnumerable<string> tags)
        {
            this.Validate();

            foreach (var tag in tags)
            {
                if (tag != null)
                {
                    var tagEntity = new Tag(EntityType.Message, this.Email.Id, TagType.UserDefined, tag, this.timestamp);
                    this.Tags.Add(tagEntity);
                }
            }
        }

        /// <summary>
        /// Creates a tag for the entity type.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        public void CreateTagFromEntityType(EntityType entityType)
        {
            this.Validate();
            var emailTypeTag = DefaultEmailTags.Admin;
            switch (entityType)
            {
                case EntityType.User:
                    emailTypeTag = DefaultEmailTags.User;
                    break;
                case EntityType.Customer:
                    emailTypeTag = DefaultEmailTags.Customer;
                    break;
                default:
                    break;
            }

            var tag = new Tag(EntityType.Message, this.Email.Id, TagType.EmailType, emailTypeTag, this.timestamp);
            this.Tags.Add(tag);
        }

        /// <summary>
        /// Creates a environment tag for the environment.
        /// </summary>
        /// <param name="environment">The environment.</param>
        public void CreateTagFromEnvironment(DeploymentEnvironment? environment)
        {
            this.Validate();

            if (environment.HasValue)
            {
                var tag = new Tag(EntityType.Message, this.Email.Id, TagType.Environment, environment.Value.ToString(), this.timestamp);
                this.Tags.Add(tag);
            }
        }

        /// <summary>
        /// Creates a tag for the email type.
        /// </summary>
        /// <param name="emailType">The email type.</param>
        public void CreateTagFromEmailType(EmailType emailType)
        {
            this.Validate();

            var emailTypeTag = emailType == EmailType.User ? DefaultEmailTags.User :
                  emailType == EmailType.Customer ? DefaultEmailTags.Customer :
                  DefaultEmailTags.Admin;

            var tag = new Tag(EntityType.Message, this.Email.Id, TagType.EmailType, emailTypeTag, this.timestamp);

            this.Tags.Add(tag);
        }

        /// <summary>
        /// validates the model if it has all the necessary data.
        /// </summary>
        private void Validate()
        {
            if (this.Email == null)
            {
                throw new InvalidOperationException("The email is required to create a relationship against it.");
            }

            if (this.timestamp == default)
            {
                throw new InvalidOperationException("The timestamp is required.");
            }
        }

        private void AddRelationship(Relationship relationship)
        {
            if (!this.Relationships.Any(x => x.Equals(relationship)))
            {
                this.Relationships.Add(relationship);
            }
        }
    }
}

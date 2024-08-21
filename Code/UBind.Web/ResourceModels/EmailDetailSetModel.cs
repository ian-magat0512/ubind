// <copyright file="EmailDetailSetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Humanizer;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Email;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Resource model for serving quote email details.
    /// </summary>
    public class EmailDetailSetModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailDetailSetModel"/> class.
        /// </summary>
        /// <param name="model">The dto model.</param>
        public EmailDetailSetModel(IEmailDetails model)
        {
            this.TenantId = model.TenantId;
            this.Id = model.Id;
            this.Recipient = model.Recipient;
            this.From = string.Join(" , ", (model.From ?? string.Empty).ExtractEmails());
            this.CC = model.CC.ExtractEmails();
            this.BCC = model.BCC.ExtractEmails();
            this.Subject = model.Subject;
            this.HtmlMessage = model.HtmlMessage;
            this.PlainMessage = model.PlainMessage;
            this.EmailType = model.EmailType.ToString() ?? " - ";
            this.Documents = model.EmailAttachments?.Select(x => new EmailDocuments(x)) ?? new List<EmailDocuments>();
            this.CreatedDateTime = model.CreatedTimestamp.ToExtendedIso8601String();
            this.Customer = model.Customer;
            this.User = model.User;
            this.Quote = model.Quote;
            this.Claim = model.Claim;
            this.Policy = model.Policy;
            this.Organisation = model.Organisation;
            this.PolicyTransaction = model.PolicyTransaction;
            this.Tags = model.Tags?.Select(x => new TagSummary(x.TagType, x.Value));
            this.Relationship = model.Relationships.Select(x => new RelationshipSummary(this.Id, x));
        }

        /// <summary>
        /// Gets string Tenant Id of the email.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the email Id.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the email's recipient.
        /// </summary>
        [JsonProperty]
        public string Recipient { get; private set; }

        /// <summary>
        /// Gets the emails sender.
        /// </summary>
        [JsonProperty]
        public string From { get; private set; }

        /// <summary>
        /// Gets the email's cc.
        /// </summary>
        [JsonProperty]
        public List<string> CC { get; private set; }

        /// <summary>
        /// Gets the email's BCC.
        /// </summary>
        [JsonProperty]
        public List<string> BCC { get; private set; }

        /// <summary>
        /// Gets the email's subject.
        /// </summary>
        [JsonProperty]
        public string Subject { get; private set; }

        /// <summary>
        /// Gets the email's html message.
        /// </summary>
        [JsonProperty]
        public string HtmlMessage { get; private set; }

        /// <summary>
        /// Gets the email's plain message.
        /// </summary>
        [JsonProperty]
        public string PlainMessage { get; private set; }

        /// <summary>
        /// Gets the email's customer record.
        /// </summary>
        [JsonProperty]
        public CustomerData Customer { get; private set; }

        /// <summary>
        /// Gets the email's user record.
        /// </summary>
        [JsonProperty]
        public UserData User { get; private set; }

        /// <summary>
        /// Gets the email's policy record.
        /// </summary>
        [JsonProperty]
        public PolicyData Policy { get; private set; }

        /// <summary>
        /// Gets the email's organisation record.
        /// </summary>
        [JsonProperty]
        public OrganisationData Organisation { get; private set; }

        /// <summary>
        /// Gets the list of tags associated with this email.
        /// </summary>
        public IEnumerable<TagSummary> Tags { get; }

        /// <summary>
        /// Gets the list of relationships associated with this email.
        /// </summary>
        public IEnumerable<RelationshipSummary> Relationship { get; }

        /// <summary>
        /// Gets the email's quote record.
        /// </summary>
        [JsonProperty]
        public QuoteData Quote { get; private set; }

        /// <summary>
        /// Gets the email's claim record.
        /// </summary>
        [JsonProperty]
        public ClaimData Claim { get; private set; }

        /// <summary>
        /// Gets the email's policy transaction record.
        /// </summary>
        [JsonProperty]
        public PolicyTransactionData PolicyTransaction { get; private set; }

        /// <summary>
        /// Gets the attachment.
        /// </summary>
        public IEnumerable<EmailDocuments> Documents { get; private set; }

        /// <summary>
        /// Gets the email's type.
        /// </summary>
        [JsonProperty]
        public string EmailType { get; private set; }

        /// <summary>
        /// Gets or sets the date the quote email was created.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; set; }

        /// <summary>
        /// Email Attachments of the EmailDetailSetModel model.
        /// </summary>
        public class EmailDocuments
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EmailDocuments"/> class.
            /// Initialize emailattachment subclass.
            /// </summary>
            /// <param name="emailAttachment">the email attachment.</param>
            public EmailDocuments(EmailAttachment emailAttachment)
            {
                this.AttachmentId = emailAttachment.Id;
                this.Id = emailAttachment.DocumentFile.Id;
                this.Name = emailAttachment.DocumentFile.Name;
                this.Type = emailAttachment.DocumentFile.Type;
                var bytes = emailAttachment.DocumentFile.FileContent.Content.Length;

                // i did this to match the protype
                var isKB = Math.Floor(Math.Log(bytes) / Math.Log(1024)) == 1;
                var format = isKB ? "0" : "0.0";
                this.Size = bytes.Bytes().Humanize(format).Replace(" ", string.Empty);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="EmailDocuments"/> class.
            /// </summary>
            public EmailDocuments()
            {
            }

            /// <summary>
            /// Gets or sets attachhment id of the file attachment.
            /// </summary>
            public Guid AttachmentId { get; set; }

            /// <summary>
            /// Gets or sets id of the file attachment.
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Gets or sets the filename.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the file type.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets the size of the file.
            /// </summary>
            public string Size { get; set; }
        }

        /// <summary>
        /// Email Tags of the EmailDetailSetModel model.
        /// </summary>
        public class TagSummary
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TagSummary"/> class.
            /// </summary>
            /// <param name="type">The tag type.</param>
            /// <param name="value">The tag value.</param>
            public TagSummary(TagType type, string value)
            {
                this.TagType = type;
                this.Value = value;
            }

            /// <summary>
            /// Gets or sets the value of the tag.
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the value of the tag type.
            /// </summary>
            public TagType TagType { get; set; }
        }

        /// <summary>
        /// Email relationships of the EmailDetailSetModel model.
        /// </summary>
        public class RelationshipSummary
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RelationshipSummary"/> class.
            /// </summary>
            /// <param name="emailId">The email id.</param>
            /// <param name="relationship">The relationship.</param>
            public RelationshipSummary(Guid emailId, Relationship relationship)
            {
                this.RelationshipType = relationship.Type;
                this.EntityId = relationship.FromEntityId == emailId ? relationship.ToEntityId : relationship.FromEntityId;
                this.EntityType = relationship.FromEntityId == emailId ? relationship.ToEntityType : relationship.FromEntityType;
            }

            /// <summary>
            /// Gets the relationship type.
            /// </summary>
            public RelationshipType RelationshipType { get; }

            /// <summary>
            /// Gets the entity id.
            /// </summary>
            public Guid EntityId { get; }

            /// <summary>
            /// Gets the entity type.
            /// </summary>
            public EntityType EntityType { get; }
        }
    }
}

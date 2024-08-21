// <copyright file="EmailMessage.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// This class is needed because we need to generate json representation of email entity that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class EmailMessage : Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailMessage"/> class.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        public EmailMessage(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailMessage"/> class.
        /// </summary>
        /// <param name="model">The email read model with related entities.</param>
        public EmailMessage(ReadWriteModel.Email.Email model)
            : base(model.Id, model.CreatedTicksSinceEpoch, null)
        {
            this.Type = MessageType.Email.ToString().ToCamelCase();
            this.TenantId = model.TenantId.ToString();
            this.From = model.From;
            this.To = this.GetEmails(model.To).ToList();
            this.ReplyTo = this.GetEmails(model.ReplyTo).ToList();
            var cc = this.GetEmails(model.CC).ToList();
            this.Cc = !cc.Any() ? null : cc;
            var bcc = this.GetEmails(model.BCC).ToList();
            this.Bcc = !bcc.Any() ? null : cc;
            this.Subject = model.Subject;
            this.TextBody = model.PlainTextBody;
            this.HtmlBody = model.HtmlBody;
            this.OrganisationId = model.OrganisationId.ToString();
        }

        public EmailMessage(
            string from,
            IEnumerable<string> replyTo,
            IEnumerable<string> to,
            IEnumerable<string> cc,
            IEnumerable<string> bcc,
            string subject,
            string textBody,
            string htmlBody,
            string comments,
            IEnumerable<string> keywords,
            Dictionary<string, object> headers,
            List<Document> attachments)
        {
            this.From = from;
            this.ReplyTo = replyTo?.ToList();
            this.To = to?.ToList();
            this.Cc = cc?.ToList();
            this.Bcc = bcc?.ToList();
            this.Subject = subject;
            this.TextBody = textBody;
            this.HtmlBody = htmlBody;
            this.Comments = comments;
            this.Keywords = new JArray(keywords);
            this.Headers = new JObject(headers);
            this.Attachments = attachments;
        }

        public EmailMessage(IEmailReadModelWithRelatedEntities model, string baseApiUrl)
            : this(model.Email)
        {
            if (model.Tenant != null)
            {
                this.Tenant = new Tenant(model.Tenant);
            }

            if (model.Organisation != null)
            {
                this.Organisation = new Organisation(model.Organisation);
            }

            if (model.Tags != null)
            {
                this.Tags = model.Tags.Select(e => e.Value).ToList();
            }

            if (model.FromRelationships != null)
            {
                this.Relationships = model.FromRelationships.Select(e => new Relationship(e)).ToList();
            }

            if (model.ToRelationships != null)
            {
                if (this.Relationships == null)
                {
                    this.Relationships = new List<Relationship>();
                }

                var relationships = model.ToRelationships.Select(e => new Relationship(e)).ToList();
                this.Relationships.AddRange(relationships);
            }

            if (model.Attachments != null)
            {
                this.Attachments = model.Email.EmailAttachments.Select(e => new Document(
                    model.Email.TenantId, model.Email.OrganisationId, e, baseApiUrl)).ToList();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailMessage"/> class.
        /// </summary>
        [JsonConstructor]
        private EmailMessage()
        {
        }

        /// <summary>
        /// Gets or sets the tenant Id.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 30)]
        [Required]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant associated with the email.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 31)]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the organization id.
        /// </summary>
        [JsonProperty(PropertyName = "organisationId", Order = 32)]
        [Required]
        public string OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organization associated with the email.
        /// </summary>
        [JsonProperty(PropertyName = "organisation", Order = 33)]
        public Organisation Organisation { get; set; }

        /// <summary>
        /// Gets or sets the from address.
        /// </summary>
        [JsonProperty(PropertyName = "from", Order = 34)]
        [Required]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the from address.
        /// </summary>
        [JsonProperty(PropertyName = "replyTo", Order = 35)]
        public List<string> ReplyTo { get; set; }

        /// <summary>
        /// Gets or sets the to address.
        /// </summary>
        [JsonProperty(PropertyName = "to", Order = 36)]
        [Required]
        public List<string> To { get; set; }

        /// <summary>
        /// Gets or sets the cc address.
        /// </summary>
        [JsonProperty(PropertyName = "cc", Order = 37)]
        public List<string> Cc { get; set; }

        /// <summary>
        /// Gets or sets the bcc address.
        /// </summary>
        [JsonProperty(PropertyName = "bcc", Order = 38)]
        public List<string> Bcc { get; set; }

        /// <summary>
        /// Gets or sets the subject of the email.
        /// </summary>
        [JsonProperty(PropertyName = "subject", Order = 39)]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body of the email in text format.
        /// </summary>
        [JsonProperty(PropertyName = "textBody", Order = 40)]
        [Required]
        public string TextBody { get; set; }

        /// <summary>
        /// Gets or sets the body of the email in html format.
        /// </summary>
        [JsonProperty(PropertyName = "htmlBody", Order = 41)]
        public string HtmlBody { get; set; }

        /// <summary>
        /// Gets or sets the comments on the email.
        /// </summary>
        [JsonProperty(PropertyName = "comments", Order = 42)]
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the keywords in the email.
        /// </summary>
        [JsonProperty(PropertyName = "keywords", Order = 43)]
        public JArray Keywords { get; set; }

        /// <summary>
        /// Gets or sets the list of headers in the email.
        /// </summary>
        [JsonProperty(PropertyName = "headers", Order = 44)]
        public JObject Headers { get; set; }

        /// <summary>
        ///  Gets or sets the list of file Ids attached in the email.
        /// </summary>
        [JsonProperty(PropertyName = "attachmentIds", Order = 45)]
        public List<string> AttachmentIds { get; set; }

        /// <summary>
        ///  Gets or sets the list of files attached in the email.
        /// </summary>
        [JsonProperty(PropertyName = "attachments", Order = 50)]
        public List<Document> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the list of tags in the email.
        /// </summary>
        [JsonProperty(PropertyName = "tags", Order = 51)]
        public List<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the relationship Ids of the email.
        /// </summary>
        [JsonProperty(PropertyName = "relationshipIds", Order = 52)]
        public List<string> RelationshipIds { get; set; }

        /// <summary>
        /// Gets or sets the relationships of the email.
        /// </summary>
        [JsonProperty(PropertyName = "relationships", Order = 53)]
        public List<Relationship> Relationships { get; set; }

        private IEnumerable<string> GetEmails(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                yield break;
            }

            MatchCollection matches = Regex.Matches(input, @"[^\s<]+@[^\s,>]+");
            foreach (Match match in matches)
            {
                yield return match.Value;
            }
        }
    }
}

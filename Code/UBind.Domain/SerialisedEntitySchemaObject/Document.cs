// <copyright file="Document.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using Humanizer;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// This class is needed because we need to generate json representation of document entity that conforms with serialized-entity-schema.json file.
    /// <remarks>Schema reference #file</remarks>
    /// </summary>
    public class Document : BaseEntity<DocumentEntity>
    {
        public Document(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class.
        /// </summary>
        /// <param name="model">The document read model with related entities.</param>
        /// <param name="hostUrl">The host URL.</param>
        public Document(QuoteDocumentReadModel model, string hostUrl)
            : base(model.Id, model.CreatedTicksSinceEpoch, null)
        {
            this.FileName = model.Name;
            this.FileSizeBytes = model.SizeInBytes;
            this.ContentId = model.FileContentId.ToString();
            this.FileSizeString = ((int)model.SizeInBytes).Bytes().Humanize("#.##");

            this.TenantId = model.TenantId.ToString();
            this.OrganisationId = model.OrganisationId.ToString();

            this.DownloadUrl = $"{hostUrl}/api/v1/{model.Environment}/quote/document/{model.Id}?quoteId={model.QuoteOrPolicyTransactionId}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class.
        /// </summary>
        /// <param name="model">The document read model with related entities.</param>
        /// <param name="hostUrl">The host URL.</param>
        public Document(ClaimAttachmentReadModel model, string hostUrl)
            : base(model.Id, model.CreatedTicksSinceEpoch, null)
        {
            this.FileName = model.Name;
            this.FileSizeBytes = model.FileSize;
            this.ContentId = model.FileContentId.ToString();
            this.FileSizeString = ((int)model.FileSize).Bytes().Humanize("#.##");

            this.TenantId = model.TenantId.ToString();
            this.OrganisationId = model.OrganisationId.ToString();

            this.DownloadUrl = $"{hostUrl}/api/v1/{model.Environment}/claim/document/{model.Id}?claimId={model.ClaimOrClaimVersionId}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="model">The document read model with related entities.</param>
        /// <param name="hostUrl">The host URL.</param>
        public Document(Guid tenantId, Guid organisationId, EmailAttachment model, string hostUrl)
            : base(model.Id, model.CreatedTicksSinceEpoch, null)
        {
            if (model.DocumentFile != null)
            {
                this.FileName = model.DocumentFile.Name;
                this.FileSizeBytes = model.DocumentFile.FileContent.Content.LongLength;
                this.ContentId = model.DocumentFile.Id.ToString();
                this.FileSizeString = ((int)model.DocumentFile.FileContent.Content.LongLength).Bytes().Humanize("#.##");
            }

            this.TenantId = tenantId.ToString();
            this.OrganisationId = organisationId.ToString();
            this.DownloadUrl = $"{hostUrl}/api/v1/email/{model.EmailId}/attachment/{model.Id}";
        }

        public Document(string filename, long fileSizeBytes)
        {
            this.FileName = filename;
            this.FileSizeBytes = fileSizeBytes;
        }

        public Document(IDocumentReadModelWithRelatedEntities model, string baseApiUrl)
            : this(model.Document, baseApiUrl)
        {
            if (model.Tenant != null)
            {
                this.Tenant = new SerialisedEntitySchemaObject.Tenant(model.Tenant);
            }

            if (model.Organisation != null)
            {
                this.Organisation = new SerialisedEntitySchemaObject.Organisation(model.Organisation);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class.
        /// </summary>
        [JsonConstructor]
        private Document()
        {
        }

        /// <summary>
        /// Gets or sets the tenant id of the user.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 21)]
        [Required]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant of the user.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 22)]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the organization id.
        /// </summary>
        [JsonProperty(PropertyName = "organisationId", Order = 23)]
        [Required]
        public string OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organization associated with the quote.
        /// </summary>
        [JsonProperty(PropertyName = "organisation", Order = 24)]
        public Organisation Organisation { get; set; }

        /// <summary>
        /// Gets or sets the document name.
        /// </summary>
        [JsonProperty(PropertyName = "fileName", Order = 25)]
        [Required]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the document size.
        /// </summary>
        [JsonProperty(PropertyName = "fileSizeBytes", Order = 26)]
        [Required]
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the document size in string.
        /// </summary>
        [JsonProperty(PropertyName = "fileSizeString", Order = 27)]
        [Required]
        public string FileSizeString { get; set; }

        /// <summary>
        /// Gets or sets the download url of the document.
        /// </summary>
        [JsonProperty(PropertyName = "downloadUrl", Order = 28)]
        [Required]
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets the ID of the file content.
        /// </summary>
        [JsonProperty(PropertyName = "contentId", Order = 29)]
        [Required]
        public string ContentId { get; set; }

        /// <summary>
        /// Gets or sets the file content.
        /// </summary>
        [JsonProperty(PropertyName = "content", Order = 30)]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the list of tags in the email.
        /// </summary>
        [JsonProperty(PropertyName = "tags", Order = 31)]
        public List<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the relationship Ids of the email.
        /// </summary>
        [JsonProperty(PropertyName = "relationshipIds", Order = 32)]
        public List<string> RelationshipIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether claim is a test data.
        /// </summary>
        [JsonProperty(PropertyName = "testData", Order = 33)]
        [DefaultValue(false)]
        public bool? IsTestData { get; set; }
    }
}

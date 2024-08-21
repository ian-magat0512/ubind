// <copyright file="DocumentSetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Globalization;
    using Humanizer;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Resource model for serving document records with the latest version as associated resource property.
    /// </summary>
    public class DocumentSetModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSetModel"/> class.
        /// </summary>
        /// <param name="document">The document record.</param>
        public DocumentSetModel(QuoteDocumentReadModel document)
        {
            this.Id = document.Id;
            this.QuoteOrPolicyTransactionId = document.QuoteOrPolicyTransactionId;
            this.FileName = document.Name;
            this.MIMEType = document.Type;
            this.FileSize = document.SizeInBytes.Bytes().Humanize("00.00");
            this.CreatedDateTime = document.CreatedTimestamp.ToString();
            this.DateGroupHeader = document.CreatedTimestamp.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSetModel"/> class.
        /// </summary>
        /// <param name="document">The claim document record.</param>
        public DocumentSetModel(ClaimAttachmentReadModel document)
        {
            this.Id = document.Id;
            this.QuoteOrPolicyTransactionId = document.ClaimOrClaimVersionId;
            this.FileName = document.Name;
            this.MIMEType = document.Type;
            this.FileSize = document.FileSize.Bytes().Humanize("00.00");
            this.CreatedDateTime = document.CreatedTimestamp.ToString();
            this.DateGroupHeader = document.CreatedTimestamp.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the ID of the document.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the ID of the quote or policy transaction the document relates to.
        /// </summary>
        public Guid QuoteOrPolicyTransactionId { get; private set; }

        /// <summary>
        /// Gets the file name of the document.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the content type of the document.
        /// </summary>
        public string MIMEType { get; private set; }

        /// <summary>
        /// Gets the file size of the document.
        /// </summary>
        public string FileSize { get; private set; }

        /// <summary>
        /// Gets the created date of the document record.
        /// </summary>
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the creation group date header of the document record.
        /// </summary>
        public string DateGroupHeader { get; private set; }
    }
}

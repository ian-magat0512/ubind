// <copyright file="FileAttachmentResultModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Model for responses to file attachment requests.
    /// </summary>
    public class FileAttachmentResultModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileAttachmentResultModel"/> class.
        /// </summary>
        /// <param name="fileAttachmentId">The ID of the new file attachment.</param>
        /// <param name="quoteId">The ID of the quote the attachment belongs to.</param>
        /// <param name="claimId">The ID of the claim the attachment belongs to.</param>
        private FileAttachmentResultModel(Guid fileAttachmentId, Guid? quoteId, Guid? claimId)
        {
            this.AttachmentId = fileAttachmentId;
            this.QuoteId = quoteId;
            this.ClaimId = claimId;
        }

        /// <summary>
        /// Gets the ID of the quote this file is for.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? QuoteId { get; private set; }

        /// <summary>
        /// Gets the ID of the claim this file is for.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? ClaimId { get; private set; }

        /// <summary>
        /// Gets a value indicating the outcome of file attachment.
        /// </summary>
        [JsonProperty]
        public Guid AttachmentId { get; private set; }

        // TODO: All properties below is trying to mimic credit card payment error flows.
        // All properties are having dummy values.
        // Replace logic once we have a flow for checking if errors upon persisting records on db occurs.

        /// <summary>
        /// Gets a value indicating the outcome of file attachment.
        /// </summary>
        [JsonProperty]
        public string Outcome { get; private set; } = "Success";

        /// <summary>
        /// Gets any errors returned for persisting file attachment records.
        /// </summary>
        [JsonProperty]
        public IEnumerable<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Gets a value indicating whether the file attachment succeeded.
        /// </summary>
        public bool Succeeded { get; } = true;

        /// <summary>
        /// Create claim file attachment result.
        /// </summary>
        /// <param name="fileAttachmentId">The ID of the new file attachment.</param>
        /// <param name="claimId">The claim Id.</param>
        /// <returns>File Attachment Result Model.</returns>
        public static FileAttachmentResultModel CreateClaimFileAttachmentResult(Guid fileAttachmentId, Guid claimId)
        {
            return new FileAttachmentResultModel(fileAttachmentId, null, claimId);
        }

        /// <summary>
        /// Create quote file attachment result.
        /// </summary>
        /// <param name="fileAttachmentId">The ID of the new file attachment.</param>
        /// <param name="quoteId">The quote Id.</param>
        /// <returns>File Attachment Result Model.</returns>
        public static FileAttachmentResultModel CreateQuoteFileAttachmentResult(Guid fileAttachmentId, Guid quoteId)
        {
            return new FileAttachmentResultModel(fileAttachmentId, quoteId, null);
        }
    }
}

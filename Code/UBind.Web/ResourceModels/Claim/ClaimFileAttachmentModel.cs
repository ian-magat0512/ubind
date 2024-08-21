// <copyright file="ClaimFileAttachmentModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Claim
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Form data for an application POSTed from client.
    /// </summary>
    public class ClaimFileAttachmentModel : IClaimResourceModel
    {
        /// <summary>
        /// Gets or sets the Claim of the quote when the resource model submitted is WebFormType.Claim.
        /// </summary>
        [Required]
        public Guid ClaimId { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        [Required]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the mime type.
        /// </summary>
        [Required]
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the file blob content.
        /// </summary>
        [Required]
        public string FileData { get; set; }
    }
}

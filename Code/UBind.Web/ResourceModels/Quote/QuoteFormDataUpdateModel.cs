// <copyright file="QuoteFormDataUpdateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Resource model used for quote state change requests POSTed from client.
    /// </summary>
    public class QuoteFormDataUpdateModel : IQuoteResourceModel
    {
        /// <inheritdoc/>
        [Required]
        public Guid QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the form data.
        /// </summary>
        [Required]
        public JObject FormDataJson { get; set; }

        public Guid? ProductReleaseId { get; set; }
    }
}

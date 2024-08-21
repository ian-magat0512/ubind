// <copyright file="IQumulateFundingModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using UBind.Application.Funding.Iqumulate.Response;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Iqumulate Premium Funding response.
    /// </summary>
    public class IQumulateFundingModel : IQuoteResourceModel
    {
        /// <inheritdoc/>
        [Required]
        public Guid QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the page response from Iqumulate Iframe.
        /// </summary>
        [Required]
        public IqumulateFundingPageResponse PageResponse { get; set; }

        [Required]
        public Guid CalculationResultId { get; set; }

        public Guid? ProductReleaseId { get; set; }
    }
}

// <copyright file="IqumulateFundingPageResponseModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.Iqumulate.Response
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// The Form model.
    /// </summary>
    public class IqumulateFundingPageResponseModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IqumulateFundingPageResponseModel"/> class.
        /// </summary>
        [JsonConstructor]
        public IqumulateFundingPageResponseModel()
        {
        }

        /// <summary>
        /// Gets or sets the actual response from Iqumulate Premium Funding.
        /// </summary>
        [JsonProperty]
        public IqumulateFundingPageResponse MPFResponse { get; set; }

        /// <summary>
        /// Gets or sets the QuoteId.
        /// </summary>
        [JsonProperty]
        public Guid QuoteId { get; set; }
    }
}

// <copyright file="QuotDataPatchTarget.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Specification for a quote patch target.
    /// </summary>
    public class QuotDataPatchTarget : DataPatchTargetEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuotDataPatchTarget"/> class.
        /// </summary>
        /// <param name="quoteId">The ID of the quote to patch.</param>
        public QuotDataPatchTarget(Guid quoteId)
        {
            this.QuoteId = quoteId;
        }

        /// <summary>
        /// Gets the ID of the quote to patch.
        /// </summary>
        [JsonProperty]
        public Guid QuoteId { get; private set; }
    }
}

// <copyright file="QuoteVersionDataPatchTarget.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Specification of a quote version to patch.
    /// </summary>
    public class QuoteVersionDataPatchTarget : QuotDataPatchTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteVersionDataPatchTarget"/> class.
        /// </summary>
        /// <param name="quoteId">The ID fo the quote.</param>
        /// <param name="versionNumber">The number of the version.</param>
        public QuoteVersionDataPatchTarget(Guid quoteId, int versionNumber)
            : base(quoteId)
        {
            this.VersionNumber = versionNumber;
        }

        /// <summary>
        /// Gets the version number.
        /// </summary>
        [JsonProperty]
        public int VersionNumber { get; private set; }
    }
}

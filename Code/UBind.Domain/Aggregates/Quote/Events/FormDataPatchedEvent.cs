// <copyright file="FormDataPatchedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when a quote's form data has been patched. Patching form data is better for saving space,
        /// since it only stores the changes to the form data, not a whole new copy.
        /// </summary>
        public class FormDataPatchedEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FormDataPatchedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="quoteId">The ID of the quote.</param>
            /// <param name="formDataPatch">The json patch to apply to the existing form data, as a string.</param>
            /// <param name="performingUserId">The userId who updates form data.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public FormDataPatchedEvent(Guid tenantId, Guid aggregateId, Guid quoteId, JsonPatchDocument formDataPatch, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.FormUpdateId = Guid.NewGuid();
                this.QuoteId = quoteId;
                this.FormDataPatch = formDataPatch;
            }

            [JsonConstructor]
            private FormDataPatchedEvent()
            {
            }

            /// <summary>
            /// Gets an ID uniquely identifying the quote whose form data was updated.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId { get; private set; }

            /// <summary>
            /// Gets an Id uniquely identifying the form update.
            /// </summary>
            [JsonProperty]
            public Guid FormUpdateId { get; private set; }

            /// <summary>
            /// Gets the patch for form data.
            /// </summary>
            [JsonProperty]
            [JsonConverter(typeof(PatchDocumentConverter))]
            public JsonPatchDocument FormDataPatch { get; private set; }
        }
    }
}

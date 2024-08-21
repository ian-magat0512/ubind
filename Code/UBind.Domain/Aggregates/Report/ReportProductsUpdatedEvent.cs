// <copyright file="ReportProductsUpdatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Report
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for report.
    /// </summary>
    public partial class ReportAggregate
    {
        /// <summary>
        /// The report's products has beend updated.
        /// </summary>
        public class ReportProductsUpdatedEvent : Event<ReportAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReportProductsUpdatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="reportId">The Id of the report.</param>
            /// <param name="productIds">The product ids of the report.</param>
            /// <param name="performingUserId">The userId who updates the product.</param>
            /// <param name="timestamp">The timestamp.</param>
            public ReportProductsUpdatedEvent(Guid tenantId, Guid reportId, IEnumerable<Guid> productIds, Guid? performingUserId, Instant timestamp)
                : base(tenantId, reportId, performingUserId, timestamp)
            {
                this.ProductNewIds = productIds.ToList();
            }

            [JsonConstructor]
            private ReportProductsUpdatedEvent()
            {
            }

            /// <summary>
            /// Gets the all product Alias ( before it was Id ) .
            /// not backward compatible of the new id format, so ProductNewIds is created.
            /// NOTE: please use the ProductNewIds parameter as this is obsolete.
            /// </summary>
            [JsonProperty]
            public ICollection<string> ProductIds { get; private set; }

            /// <summary>
            /// Gets the all product Id.
            /// </summary>
            [JsonProperty]
            public ICollection<Guid> ProductNewIds { get; private set; }
        }
    }
}

// <copyright file="IContextEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Configuration
{
    /// <summary>
    /// Gets the context entities section for the current product.json configuration.
    /// </summary>
    public interface IContextEntities
    {
        /// <summary>
        /// Gets or sets the context entities default configuration for all quote types.
        /// </summary>
        ContextEntitySettings? Quotes { get; set; }

        /// <summary>
        /// Gets or sets the context entities configuration for new business quotes.
        /// </summary>
        ContextEntitySettings? NewBusinessQuotes { get; set; }

        /// <summary>
        /// Gets or sets the context entities configuration for adjustment quotes.
        /// </summary>
        ContextEntitySettings? AdjustmentQuotes { get; set; }

        /// <summary>
        /// Gets or sets the context entities configuration for renewal quotes.
        /// </summary>
        ContextEntitySettings? RenewalQuotes { get; set; }

        /// <summary>
        /// Gets or sets the context entities configuration for cancellation quotes.
        /// </summary>
        ContextEntitySettings? CancellationQuotes { get; set; }

        /// <summary>
        /// Gets or sets the context entities configuration for claims.
        /// </summary>
        ContextEntitySettings? Claims { get; set; }
    }
}

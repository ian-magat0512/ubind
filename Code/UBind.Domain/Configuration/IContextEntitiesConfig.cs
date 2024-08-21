// <copyright file="IContextEntitiesConfig.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Configuration
{
    /// <summary>
    /// Provides access to context entities configuration from product.json file.
    /// </summary>
    public interface IContextEntitiesConfig
    {
        /// <summary>
        /// Gets or sets the contextEntities property setting that should be applied to all types as the default option.
        /// </summary>
        ContextEntities ContextEntities { get; set; }

        /// <summary>
        /// Gets or sets the contextEntities that will override the contextEntities property settings, but only for newBusiness quotes.
        /// </summary>
        ContextEntities AdjustmentContextEntities { get; set; }

        /// <summary>
        /// Gets or sets the contextEntities that will override the contextEntities property settings, but only for adjustment quotes.
        /// </summary>
        ContextEntities CancellationContextEntities { get; set; }

        /// <summary>
        /// Gets or sets the contextEntities that will override the contextEntities property settings, but only for renewal quotes.
        /// </summary>
        ContextEntities NewBusinessContextEntities { get; set; }

        /// <summary>
        /// Gets or sets the contextEntities that will override the contextEntities property settings, but only for cancellation quotes.
        /// </summary>
        ContextEntities RenewalContextEntities { get; set; }
    }
}

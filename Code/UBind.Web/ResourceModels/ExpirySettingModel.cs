// <copyright file="ExpirySettingModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using UBind.Domain.Product;

    /// <summary>
    /// Resource model for product expiry settings.
    /// </summary>
    public class ExpirySettingModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether check if it is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether the value of expiry days to be appended to quotes.
        /// </summary>
        public int ExpiryDays { get; set; }

        /// <summary>
        /// Gets or sets the update type that will propagate.
        /// </summary>
        public ProductQuoteExpirySettingUpdateType UpdateType { get; set; }
    }
}

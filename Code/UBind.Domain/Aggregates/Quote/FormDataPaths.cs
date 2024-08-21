// <copyright file="FormDataPaths.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    /// <summary>
    /// Specifies paths to specific data in the form data json.
    /// </summary>
    public class FormDataPaths : IFormDataPaths
    {
        /// <summary>
        /// Default path for inception date in form data.
        /// </summary>
        public const string DefaultEffectiveDatePath = "policyStartDate";

        /// <summary>
        /// Default path for expiry date in form data.
        /// </summary>
        public const string DefaultExpiryDatePath = "policyEndDate";

        public FormDataPaths(
            string inceptionDatePath,
            string expiryDatePath)
        {
            this.EffectiveDatePath = inceptionDatePath;
            this.ExpiryDatePath = expiryDatePath;
        }

        /// <summary>
        /// Gets default form data paths.
        /// </summary>
        public static FormDataPaths Default => new FormDataPaths(DefaultEffectiveDatePath, DefaultExpiryDatePath);

        /// <summary>
        /// Gets the inception date path.
        /// </summary>
        public string EffectiveDatePath { get; }

        /// <summary>
        /// Gets the expiry date path.
        /// </summary>
        public string ExpiryDatePath { get; }
    }
}

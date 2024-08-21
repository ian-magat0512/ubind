// <copyright file="ReleaseCalculationOutput.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FlexCel
{
    using System;

    /// <summary>
    /// Holds the outcome of a calculation using a given release.
    /// </summary>
    public class ReleaseCalculationOutput
    {
        /// <summary>
        /// Gets or sets the resulting calculation output as a json string.
        /// </summary>
        public string CalculationJson { get; set; }

        /// <summary>
        /// Gets or sets the ID of the release used to create the calculation.
        /// </summary>
        public Guid ReleaseId { get; set; }
    }
}

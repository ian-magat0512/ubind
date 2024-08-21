// <copyright file="VehicleMake.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ThirdPartyDataSets.RedBook
{
    /// <summary>
    /// Represents the vehicle makes entity class.
    /// </summary>
    public class VehicleMake
    {
        /// <summary>
        /// Gets or sets the first four characters of the manufacturers name.
        /// </summary>
        public string MakeCode { get; set; }

        /// <summary>
        /// Gets or sets the vehicle manufacturers name.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the first year that this manufacturer produced a car.
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        /// Gets or sets the latest year that this manufacturer produced a car.
        /// </summary>
        public int LatestYear { get; set; }
    }
}

// <copyright file="VehicleFamily.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ThirdPartyDataSets.RedBook
{
    /// <summary>
    /// Represents the vehicle family entity class.
    /// </summary>
    public class VehicleFamily
    {
        /// <summary>
        /// Gets or sets the make code of the vehicle family.
        /// </summary>
        public string MakeCode { get; set; }

        /// <summary>
        /// Gets or sets the vehicle family code.
        /// </summary>
        public string FamilyCode { get; set; }

        /// <summary>
        /// Gets or sets the type of vehicle.
        /// </summary>
        public string VehicleTypeCode { get; set; }

        /// <summary>
        /// Gets or sets the family name in full.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the year that this family started.
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        /// Gets or sets the latest year that this family ran to.
        /// </summary>
        public int LatestYear { get; set; }
    }
}

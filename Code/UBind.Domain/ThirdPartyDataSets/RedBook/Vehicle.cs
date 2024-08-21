// <copyright file="Vehicle.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ThirdPartyDataSets.RedBook
{
    /// <summary>
    /// Represents the vehicle entity class.
    /// </summary>
    public class Vehicle
    {
        /// <summary>
        /// Gets or sets the vehicle key.
        /// </summary>
        public string VehicleKey { get; set; }

        /// <summary>
        /// Gets or sets the vehicle description.
        /// </summary>
        public string VehicleDescription { get; set; }

        /// <summary>
        /// Gets or sets the vehicle make code.
        /// </summary>
        public string MakeCode { get; set; }

        /// <summary>
        /// Gets or sets the vehicle make description.
        /// </summary>
        public string MakeDescription { get; set; }

        /// <summary>
        /// Gets or sets the vehicle family code.
        /// </summary>
        public string FamilyCode { get; set; }

        /// <summary>
        /// Gets or sets the vehicle family description.
        /// </summary>
        public string FamilyDescription { get; set; }

        /// <summary>
        /// Gets or sets the vehicle year group.
        /// </summary>
        public int YearGroup { get; set; }

        /// <summary>
        /// Gets or sets the vehicle month group.
        /// </summary>
        public int MonthGroup { get; set; }
    }
}

// <copyright file="MaterializedAddressView.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ThirdPartyDataSets.Gnaf
{
    /// <summary>
    /// The materialized view for Gnaf adddress.
    /// </summary>
    public class MaterializedAddressView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterializedAddressView"/> class.
        /// A protected, parameterless constructor for EF, allowing proxy generation for lazy loading.
        /// </summary>
        public MaterializedAddressView()
        {
        }

        /// <summary>
        /// Gets or sets the detail id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the number first.
        /// </summary>
        public string NumberFirst { get; set; }

        /// <summary>
        /// Gets or sets the building name.
        /// </summary>
        public string BuildingName { get; set; }

        /// <summary>
        /// Gets or sets the lot number.
        /// </summary>
        public string LotNumber { get; set; }

        /// <summary>
        /// Gets or sets the flat number.
        /// </summary>
        public string FlatNumber { get; set; }

        /// <summary>
        /// Gets or sets the street name.
        /// </summary>
        public string StreetName { get; set; }

        /// <summary>
        /// Gets or sets the flat type.
        /// </summary>
        public string FlatType { get; set; }

        /// <summary>
        /// Gets or sets the level type.
        /// </summary>
        public string LevelType { get; set; }

        /// <summary>
        /// Gets or sets the level number.
        /// </summary>
        public string LevelNumber { get; set; }

        /// <summary>
        /// Gets or sets the street type code.
        /// </summary>
        public string StreetTypeCode { get; set; }

        /// <summary>
        /// Gets or sets the street type code.
        /// </summary>
        public string StreetTypeShortName { get; set; }

        /// <summary>
        /// Gets or sets the locality name.
        /// </summary>
        public string LocalityName { get; set; }

        /// <summary>
        /// Gets or sets the state abbreviation.
        /// </summary>
        public string StateAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the state name.
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        /// Gets or sets the Post Code.
        /// </summary>
        public string PostCode { get; set; }

        /// <summary>
        /// Gets or sets the Full Address.
        /// </summary>
        public string FullAddress { get; set; }

        /// <summary>
        /// Gets or sets the Latitude.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the Longitude.
        /// </summary>
        public double Longitude { get; set; }
    }
}

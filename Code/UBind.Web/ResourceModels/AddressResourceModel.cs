// <copyright file="AddressResourceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain.Extensions;
    using UBind.Domain.ThirdPartyDataSets.Gnaf;

    /// <summary>
    /// For representing addresses in requests.
    /// </summary>
    public class AddressResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressResourceModel"/> class.
        /// </summary>
        /// <param name = "address">The address to represent.</param>
        public AddressResourceModel(MaterializedAddressView address)
        {
            if (address == null)
            {
                return;
            }

            var splitFullAddress = address.FullAddress?.ToTitleCase().Split(',');
            var fullAddress = address.FullAddress?.ToTitleCase().Replace(" ,", ",").Replace(splitFullAddress.Last(), splitFullAddress.Last().ToUpper());

            this.Id = address.Id;
            this.StreetNumber = address.NumberFirst?.ToTitleCase().Trim();
            this.LotNumber = address.LotNumber?.ToTitleCase();
            this.FlatNumber = address.FlatNumber?.ToTitleCase();
            this.StreetName = address.StreetName?.ToTitleCase();
            this.StreetTypeCode = address.StreetTypeCode?.ToTitleCase();
            this.LocalityName = address.LocalityName?.ToTitleCase();
            this.StateAbbreviation = address.StateAbbreviation?.ToTitleCase();
            this.StateName = address.StateName?.ToTitleCase();
            this.PostCode = address.PostCode?.ToTitleCase();
            this.FlatType = address.FlatType?.ToTitleCase();
            this.FullAddress = fullAddress;
            this.Latitude = address.Latitude;
            this.Longitude = address.Longitude;
        }

        /// <summary>
        /// Gets the full address.
        /// </summary>
        [JsonProperty]
        public string FullAddress { get; }

        /// <summary>
        /// Gets the Gnaf address detail id.
        /// </summary>
        [JsonProperty]
        public string Id { get; }

        /// <summary>
        /// Gets the number first.
        /// </summary>
        [JsonProperty]
        public string StreetNumber { get; }

        /// <summary>
        /// Gets the lot number.
        /// </summary>
        [JsonProperty]
        public string LotNumber { get; }

        /// <summary>
        /// Gets the flat number.
        /// </summary>
        [JsonProperty]
        public string FlatNumber { get; }

        /// <summary>
        /// Gets the flat type.
        /// </summary>
        [JsonProperty]
        public string FlatType { get; }

        /// <summary>
        /// Gets the level number.
        /// </summary>
        [JsonProperty]
        public string LevelNumber { get; }

        /// <summary>
        /// Gets the Level type.
        /// </summary>
        [JsonProperty]
        public string LevelType { get; }

        /// <summary>
        /// Gets the street name.
        /// </summary>
        [JsonProperty]
        public string StreetName { get; }

        /// <summary>
        /// Gets the street Type Code.
        /// </summary>
        [JsonProperty]
        public string StreetTypeCode { get; }

        /// <summary>
        /// Gets the Locality Name.
        /// </summary>
        [JsonProperty]
        public string LocalityName { get; }

        /// <summary>
        /// Gets the State Abbreviation.
        /// </summary>
        [JsonProperty]
        public string StateAbbreviation { get; }

        /// <summary>
        /// Gets the State Name.
        /// </summary>
        [JsonProperty]
        public string StateName { get; }

        /// <summary>
        /// Gets the Post Code.
        /// </summary>
        [JsonProperty]
        public string PostCode { get; }

        /// <summary>
        /// Gets the Latitude.
        /// </summary>
        [JsonProperty]
        public double Latitude { get; }

        /// <summary>
        /// Gets the Longitude.
        /// </summary>
        [JsonProperty]
        public double Longitude { get; }
    }
}

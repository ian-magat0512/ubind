// <copyright file="StreetAddress.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel.Person.Fields;

    /// <summary>
    /// This class is needed because we need to generate json representation of a street address object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class StreetAddress : OrderedField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreetAddress"/> class.
        /// </summary>
        /// <param name="addressReadModel">The address model.</param>
        public StreetAddress(StreetAddressReadModel addressReadModel)
        {
            this.Address = addressReadModel.Address;
            this.Suburb = addressReadModel.Suburb;
            this.State = addressReadModel.State;
            this.Postcode = addressReadModel.Postcode;
            this.IsDefault = addressReadModel.IsDefault;
            this.SetLabel(addressReadModel.Label, addressReadModel.CustomLabel);
        }

        public StreetAddress(StreetAddress streetAddress)
        {
            this.Address = streetAddress.Address;
            this.Suburb = streetAddress.Suburb;
            this.State = streetAddress.State;
            this.Postcode = streetAddress.Postcode;
            this.IsDefault = streetAddress.IsDefault;
            this.Label = streetAddress.Label;
        }

        /// <summary>
        /// Gets or sets the first line of the address, i.e. street and number etc.
        /// </summary>
        [JsonProperty("address", Order = 1)]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the suburb or city.
        /// </summary>
        [JsonProperty("suburb", Order = 2)]
        public string Suburb { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        [JsonProperty("state", Order = 3)]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postcode.
        /// </summary>
        [JsonProperty("postcode", Order = 4)]
        public string Postcode { get; set; }
    }
}

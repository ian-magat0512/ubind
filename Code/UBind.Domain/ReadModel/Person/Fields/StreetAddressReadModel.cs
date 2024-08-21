// <copyright file="StreetAddressReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Person.Fields
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Person.Fields;

    /// <summary>
    /// Read model for addresses.
    /// </summary>
    public class StreetAddressReadModel : LabelledOrderedField, IReadModel<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreetAddressReadModel"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="streetAddressField">The street address field.</param>
        public StreetAddressReadModel(Guid tenantId, StreetAddressField streetAddressField)
            : base(
                  tenantId,
                  Guid.NewGuid(),
                  streetAddressField.Label,
                  streetAddressField.CustomLabel,
                  streetAddressField.SequenceNo,
                  streetAddressField.IsDefault)
        {
            this.Address = streetAddressField.StreetAddressValueObject.Line1;
            this.Suburb = streetAddressField.StreetAddressValueObject.Suburb;
            this.Postcode = streetAddressField.StreetAddressValueObject.Postcode;
            this.State = streetAddressField.StreetAddressValueObject.State.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreetAddressReadModel"/> class.
        /// Parameterless Constructor for AddressReadModel.
        /// </summary>
        protected StreetAddressReadModel()
        {
        }

        /// <summary>
        /// Gets or sets the first line of the address, i.e. street and number etc.
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the suburb or city.
        /// </summary>
        [JsonProperty("suburb")]
        public string Suburb { get; set; }

        /// <summary>
        /// Gets or sets the postcode.
        /// </summary>
        [JsonProperty("postcode")]
        public string Postcode { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonIgnore]
        protected override IEnumerable<string> LabelOptions => new List<string> { "home", "work", "postal", "other", };
    }
}

// <copyright file="StreetAddressField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person.Fields
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel.Person.Fields;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// The field for street address.
    /// </summary>
    public class StreetAddressField : LabelledOrderedField
    {
        private Address addressValueObject;

        public StreetAddressField(string address, string suburb, string state, string postCode, int sequence, string label, bool isDefault)
        {
            this.Address = address;
            this.Suburb = suburb;
            this.State = state.ToUpper();
            this.Postcode = postCode;
            this.SequenceNo = sequence;
            this.Label = label;
            this.IsDefault = isDefault;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreetAddressField"/> class.
        /// </summary>
        /// <param name="label">The label of the address.</param>
        /// <param name="customLabel">the custom label of the address.</param>
        /// <param name="address">The address value object.</param>
        public StreetAddressField(string label, string customLabel, Address address)
            : base(label, customLabel)
        {
            this.addressValueObject = address;
            this.Address = address.Line1;
            this.Suburb = address.Suburb;
            this.Postcode = address.Postcode;
            this.State = address.State.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreetAddressField"/> class.
        /// </summary>
        /// <param name="addressReadModel">The address read model.</param>
        public StreetAddressField(StreetAddressReadModel addressReadModel)
           : base(
                  addressReadModel.TenantId,
                  addressReadModel.Id,
                  addressReadModel.Label,
                  addressReadModel.CustomLabel,
                  addressReadModel.SequenceNo,
                  addressReadModel.IsDefault)
        {
            this.Id = addressReadModel.Id;
            this.addressValueObject = new Address(
                addressReadModel.Address, addressReadModel.Suburb, addressReadModel.Postcode, addressReadModel.State);
            this.Address = this.addressValueObject.Line1;
            this.Suburb = this.addressValueObject.Suburb;
            this.Postcode = this.addressValueObject.Postcode;
            this.State = this.addressValueObject.State.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreetAddressField"/> class.
        /// </summary>
        /// <remarks>
        /// Used for retrieving the aggregate events.
        /// </remarks>
        [JsonConstructor]
        public StreetAddressField()
        {
        }

        /// <summary>
        /// Gets or sets the first line of the address, i.e. street and number etc.
        /// </summary>
        [JsonProperty]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the suburb or city.
        /// </summary>
        [JsonProperty]
        public string Suburb { get; set; }

        /// <summary>
        /// Gets or sets the postcode.
        /// </summary>
        [JsonProperty]
        public string Postcode { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        [JsonProperty]
        public string State { get; set; }

        /// <summary>
        /// Gets the value of the address.
        /// </summary>
        [JsonIgnore]
        public Address StreetAddressValueObject
        {
            get
            {
                if (this.addressValueObject != null)
                {
                    this.Address = this.addressValueObject.Line1;
                    this.Suburb = this.addressValueObject.Suburb;
                    this.Postcode = this.addressValueObject.Postcode;
                    this.State = this.addressValueObject.State.ToString();
                }
                else
                {
                    this.addressValueObject = this.Address != null
                        ? new Address(this.Address, this.Suburb, this.Postcode, this.State) : null;
                }

                return this.addressValueObject;
            }
        }

        [JsonIgnore]
        protected override IEnumerable<string> LabelOptions => new List<string> { "home", "work", "postal", "other", };
    }
}

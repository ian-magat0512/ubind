// <copyright file="StreetAddressModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Person
{
    using UBind.Domain.Aggregates.Person.Fields;

    /// <summary>
    /// Resource model for person's street address.
    /// </summary>
    public class StreetAddressModel : FieldResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreetAddressModel"/> class.
        /// </summary>
        /// <param name="streetAddressField">The street address field.</param>
        public StreetAddressModel(StreetAddressField streetAddressField)
            : base(streetAddressField.Id, streetAddressField.Label, streetAddressField.CustomLabel)
        {
            this.SequenceNo = streetAddressField.SequenceNo;
            this.IsDefault = streetAddressField.IsDefault;
            this.Address = streetAddressField.StreetAddressValueObject.Line1;
            this.Suburb = streetAddressField.StreetAddressValueObject.Suburb;
            this.Postcode = streetAddressField.StreetAddressValueObject.Postcode;
            this.State = streetAddressField.StreetAddressValueObject.State.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreetAddressModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for street address model.
        /// </remarks>
        protected StreetAddressModel()
        {
        }

        /// <summary>
        /// Gets or sets the first line of the address, i.e. street and number etc.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the suburb or city.
        /// </summary>
        public string Suburb { get; set; }

        /// <summary>
        /// Gets or sets the postcode.
        /// </summary>
        public string Postcode { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public string State { get; set; }
    }
}

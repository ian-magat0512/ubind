// <copyright file="Address.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ValueTypes
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// For representing addresses.
    /// </summary>
    public class Address : ValueObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Address"/> class.
        /// </summary>
        /// <param name="line1">The street line 1 address.</param>
        /// <param name="suburb">The suburb of the address.</param>
        /// <param name="postcode">The postcode of the address.</param>
        /// <param name="state">The state of the address.</param>
        public Address(string line1, string suburb, string postcode, string state)
        {
            this.Line1 = line1;
            this.Suburb = suburb;
            this.Postcode = postcode;
            this.State = Enum.TryParse(state, out State newState) ? newState : State.Unspecified;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Address"/> class.
        /// </summary>
        public Address()
        {
        }

        /// <summary>
        /// Gets or sets the first line of the address, i.e. street and number etc.
        /// </summary>
        public string Line1 { get; set; }

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
        public State State { get; set; }

        /// <inheritdoc/>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.Line1;
            yield return this.Suburb;
            yield return this.Postcode;
            yield return this.State;
        }
    }
}

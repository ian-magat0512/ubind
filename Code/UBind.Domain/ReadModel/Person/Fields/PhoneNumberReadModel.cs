// <copyright file="PhoneNumberReadModel.cs" company="uBind">
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
    /// The read model for phone number.
    /// </summary>
    public class PhoneNumberReadModel : LabelledOrderedField, IReadModel<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberReadModel"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="phoneNumberField">The phone field.</param>
        public PhoneNumberReadModel(Guid tenantId, PhoneNumberField phoneNumberField)
            : base(
                  tenantId,
                  Guid.NewGuid(),
                  phoneNumberField.Label,
                  phoneNumberField.CustomLabel,
                  phoneNumberField.SequenceNo,
                  phoneNumberField.IsDefault)
        {
            this.PhoneNumber = phoneNumberField.PhoneNumberValueObject?.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberReadModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// </remarks>
        protected PhoneNumberReadModel()
        {
        }

        /// <summary>
        /// Gets the phone number.
        /// </summary>
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; private set; }

        [JsonIgnore]
        protected override IEnumerable<string> LabelOptions => new List<string> { "mobile", "home", "work", "other", };
    }
}

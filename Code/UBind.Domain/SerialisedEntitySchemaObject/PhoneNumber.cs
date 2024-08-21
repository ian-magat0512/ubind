// <copyright file="PhoneNumber.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel.Person.Fields;

    /// <summary>
    /// This class is needed because we need to generate json representation of a phone number object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class PhoneNumber : OrderedField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumber"/> class.
        /// </summary>
        /// <param name="readModel">The phone number read model.</param>
        public PhoneNumber(PhoneNumberReadModel readModel)
        {
            this.ContactNumber = readModel.PhoneNumber;
            this.IsDefault = readModel.IsDefault;
            this.SetLabel(readModel.Label, readModel.CustomLabel);
        }

        public PhoneNumber(PhoneNumber phoneNumber)
        {
            this.ContactNumber = phoneNumber.ContactNumber;
            this.IsDefault = phoneNumber.IsDefault;
            this.Label = phoneNumber.Label;
        }

        /// <summary>
        /// Gets or  sets the phone number.
        /// </summary>
        [JsonProperty("phoneNumber", Order = 1)]
        public string ContactNumber { get; set; }
    }
}

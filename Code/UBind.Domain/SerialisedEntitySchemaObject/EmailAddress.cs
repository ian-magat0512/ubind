// <copyright file="EmailAddress.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel.Person.Fields;

    /// <summary>
    /// This class is needed because we need to generate json representation of an email address object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class EmailAddress : OrderedField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddress"/> class.
        /// </summary>
        /// <param name="readModel">The read model for the email address.</param>
        public EmailAddress(EmailAddressReadModel readModel)
        {
            this.Email = readModel.EmailAddress;
            this.IsDefault = readModel.IsDefault;
            this.SetLabel(readModel.Label, readModel.CustomLabel);
        }

        public EmailAddress(EmailAddress emailAddress)
        {
            this.Email = emailAddress.Email;
            this.IsDefault = emailAddress.IsDefault;
            this.Label = emailAddress.Label;
        }

        /// <summary>
        /// Gets the email address.
        /// </summary>
        [JsonProperty("emailAddress", Order = 1)]
        public string Email { get; private set; }
    }
}

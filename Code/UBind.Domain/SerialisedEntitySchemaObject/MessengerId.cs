// <copyright file="MessengerId.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel.Person.Fields;

    /// <summary>
    /// This class is needed because we need to generate json representation of a messenger Id object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class MessengerId : OrderedField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessengerId"/> class.
        /// </summary>
        /// <param name="readModel">The messenger Id read model.</param>
        public MessengerId(MessengerIdReadModel readModel)
        {
            this.MessengerIdentifier = readModel.MessengerId;
            this.IsDefault = readModel.IsDefault;
            this.SetLabel(readModel.Label, readModel.CustomLabel);
        }

        public MessengerId(MessengerId messengerId)
        {
            this.MessengerIdentifier = messengerId.MessengerIdentifier;
            this.IsDefault = messengerId.IsDefault;
            this.Label = messengerId.Label;
        }

        /// <summary>
        /// Gets or sets the messenger ID.
        /// </summary>
        [JsonProperty("messengerId", Order = 1)]
        public string MessengerIdentifier { get; set; }
    }
}

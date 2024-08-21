// <copyright file="MessengerIdField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person.Fields
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel.Person.Fields;

    /// <summary>
    /// The Messenger field.
    /// </summary>
    public class MessengerIdField : LabelledOrderedField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessengerIdField"/> class.
        /// </summary>
        /// <param name="label">The label of the social field.</param>
        /// <param name="customLabel">the custom label of the social.</param>
        /// <param name="messengerId">The messenger field value.</param>
        public MessengerIdField(string label, string customLabel, string messengerId)
            : base(label, customLabel)
        {
            this.MessengerId = messengerId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessengerIdField"/> class.
        /// </summary>
        /// <param name="messengerReadModel">The messenger read model.</param>
        public MessengerIdField(MessengerIdReadModel messengerReadModel)
            : base(
                  messengerReadModel.TenantId,
                  messengerReadModel.Id,
                  messengerReadModel.Label,
                  messengerReadModel.CustomLabel,
                  messengerReadModel.SequenceNo,
                  messengerReadModel.IsDefault)
        {
            this.Id = messengerReadModel.Id;
            this.MessengerId = messengerReadModel.MessengerId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessengerIdField"/> class.
        /// </summary>
        /// <remarks>
        /// Used for retrieving the aggregate events.
        /// </remarks>
        [JsonConstructor]
        public MessengerIdField()
        {
        }

        /// <summary>
        /// Gets or sets the value of the messenger field.
        /// </summary>
        [JsonProperty]
        public string MessengerId { get; set; }

        [JsonIgnore]
        protected override IEnumerable<string> LabelOptions => new List<string> { "skype", "whatsapp", "other", };
    }
}

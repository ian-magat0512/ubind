// <copyright file="MessengerIdReadModel.cs" company="uBind">
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
    /// Read model for messenger.
    /// </summary>
    public class MessengerIdReadModel : LabelledOrderedField, IReadModel<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessengerIdReadModel"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="messengerIdField">The messenger Id field.</param>
        public MessengerIdReadModel(Guid tenantId, MessengerIdField messengerIdField)
            : base(
                  tenantId,
                  Guid.NewGuid(),
                  messengerIdField.Label,
                  messengerIdField.CustomLabel,
                  messengerIdField.SequenceNo,
                  messengerIdField.IsDefault)
        {
            this.MessengerId = messengerIdField.MessengerId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessengerIdReadModel"/> class.
        /// Parameterless Constructor for MessengerReadModel.
        /// </summary>
        protected MessengerIdReadModel()
        {
        }

        /// <summary>
        /// Gets or sets the Messenger.
        /// </summary>
        [JsonProperty("messengerId")]
        public string MessengerId { get; set; }

        [JsonIgnore]
        protected override IEnumerable<string> LabelOptions => new List<string> { "skype", "whatsapp", "other", };
    }
}

// <copyright file="SendEmailActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Email;
    using UBind.Application.Automation.Enums;

    /// <summary>
    /// Represents the data of an action of type <see cref="SendEmailAction"/>.
    /// </summary>
    public class SendEmailActionData : ActionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailActionData"/> class.
        /// </summary>
        /// <param name="alias">The alias of the action this data is for.</param>
        public SendEmailActionData(string name, string alias, IClock clock)
            : base(name, alias, ActionType.SendEmailAction, clock)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailActionData"/> class.
        /// </summary>
        [JsonConstructor]
        public SendEmailActionData()
            : base(ActionType.SendEmailAction)
        {
        }

        /// <summary>
        /// Gets or sets the email made by the send email action.
        /// </summary>
        [JsonProperty(PropertyName = "email", NullValueHandling = NullValueHandling.Ignore)]
        public Email Email { get; set; }

        /// <summary>
        /// Gets or sets the tags added by send email action.
        /// </summary>
        [JsonProperty(PropertyName = "tags", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the relationships added by send email action.
        /// </summary>
        [JsonProperty(PropertyName = "relationships", NullValueHandling = NullValueHandling.Ignore)]
        public List<UBind.Domain.SerialisedEntitySchemaObject.Relationship> Relationships { get; set; }
    }
}

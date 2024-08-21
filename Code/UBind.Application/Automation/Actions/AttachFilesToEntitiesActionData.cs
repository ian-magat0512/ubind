// <copyright file="AttachFilesToEntitiesActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Enums;

    /// <summary>
    /// Represents the data of an action of type <see cref="AttachFilesToEntitiesActionData"/>.
    /// </summary>
    public class AttachFilesToEntitiesActionData : ActionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttachFilesToEntitiesActionData"/> class.
        /// </summary>
        /// <param name="alias">The alias of the action this data is for.</param>
        public AttachFilesToEntitiesActionData(string name, string alias, IClock clock)
            : base(name, alias, ActionType.AttachFilesToEntitiesAction, clock)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachFilesToEntitiesActionData"/> class.
        /// </summary>
        [JsonConstructor]
        public AttachFilesToEntitiesActionData()
            : base(ActionType.AttachFilesToEntitiesAction)
        {
        }

        /// <summary>
        /// Gets or sets the key-value pairs of entity IDs to entity type where the file/s were attached.
        /// </summary>
        [JsonProperty("entities")]
        public Dictionary<string, string> EntityReferences { get; set; }

        /// <summary>
        /// Gets or sets the list of the names of the file attachment/s.
        /// </summary>
        [JsonProperty("attachments")]
        public List<string> Attachments { get; set; }
    }
}

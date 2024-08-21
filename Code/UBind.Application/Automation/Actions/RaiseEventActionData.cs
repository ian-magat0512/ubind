// <copyright file="RaiseEventActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Enums;

    /// <summary>
    /// Represents the data of an action of type <see cref="RaiseEventActionData"/>.
    /// </summary>
    public class RaiseEventActionData : ActionData
    {
        /// <param name="alias">The alias of the action this data is for.</param>
        public RaiseEventActionData(string name, string alias, IClock clock)
            : base(name, alias, ActionType.RaiseEventAction, clock)
        {
        }

        [JsonConstructor]
        public RaiseEventActionData()
            : base(ActionType.RaiseEventAction)
        {
        }

        /// <summary>
        /// Gets or sets the event data.
        /// </summary>
        [JsonProperty("eventData")]
        public object? EventData { get; set; }

        /// <summary>
        /// Gets or sets the duration provider.
        /// </summary>
        [JsonProperty("period")]
        public Period? Period { get; set; }

        /// <summary>
        /// Gets or sets the event tags.
        /// </summary>
        [JsonProperty("tags")]
        public IEnumerable<string>? Tags { get; set; }

        /// <summary>
        /// Gets or sets the event tags.
        /// </summary>
        [JsonProperty("eventTags")]
        [Obsolete("Please use Tags instead.")]
        public IEnumerable<string>? EventTags { get; set; }
    }
}

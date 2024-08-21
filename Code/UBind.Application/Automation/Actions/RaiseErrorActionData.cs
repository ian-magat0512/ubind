// <copyright file="RaiseErrorActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Enums;
    using UBind.Application.JsonConverters;

    /// <summary>
    /// This file represents the data of an action of type <see cref="RaiseErrorAction"/>.
    /// </summary>
    public class RaiseErrorActionData : ActionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RaiseErrorActionData"/> class.
        /// </summary>
        /// <param name="alias">The alias of the action the data is for.</param>
        public RaiseErrorActionData(string name, string alias, IClock clock)
            : base(name, alias, ActionType.RaiseErrorAction, clock)
        {
            this.Alias = alias;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RaiseErrorActionData"/> class.
        /// </summary>
        [JsonConstructor]
        public RaiseErrorActionData()
            : base(ActionType.RaiseErrorAction)
        {
        }

        /// <summary>
        /// Gets or sets the error that was raised by the action.
        /// </summary>
        [JsonProperty("raisedError", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ErrorConverter))]
        public Domain.Error RaisedError { get; set; }
    }
}

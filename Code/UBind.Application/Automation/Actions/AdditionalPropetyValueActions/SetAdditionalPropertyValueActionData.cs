// <copyright file="SetAdditionalPropertyValueActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions.AdditionalPropetyValueActions
{
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Enums;

    /// <summary>
    /// Represents the data of an action of type <see cref="SetAdditionalPropertyValueActionData"/>.
    /// </summary>
    public class SetAdditionalPropertyValueActionData : ActionData
    {
        public SetAdditionalPropertyValueActionData(string name, string alias, IClock clock)
            : base(name, alias, ActionType.SetAdditionalPropertyValueAction, clock)
        {
        }

        [JsonConstructor]
        public SetAdditionalPropertyValueActionData()
            : base(ActionType.SetAdditionalPropertyValueAction)
        {
        }

        [JsonProperty("entityType")]
        public string EntityType { get; set; }

        [JsonProperty("entityId")]
        public string EntityId { get; set; }

        [JsonProperty("propertyAlias")]
        public string PropertyAlias { get; set; }

        [JsonProperty("resultingValue ")]
        public object ResultingValue { get; set; }
    }
}

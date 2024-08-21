// <copyright file="GroupActionData.cs" company="uBind">
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
    /// Represents the data of an <see cref="GroupAction"/>.
    /// </summary>
    public class GroupActionData : ActionData
    {
        public GroupActionData(string name, string alias, IClock clock)
            : base(name, alias, ActionType.GroupAction, clock)
        {
        }

        [JsonConstructor]
        public GroupActionData()
            : base(ActionType.GroupAction)
        {
        }

        [JsonProperty("actions")]
        public Dictionary<string, ActionData> Actions { get; set; } = new Dictionary<string, ActionData>();
    }
}

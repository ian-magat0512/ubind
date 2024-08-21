// <copyright file="SetVariableActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Enums;

    public class SetVariableActionData : ActionData
    {
        public SetVariableActionData(string name, string alias, IClock clock)
            : base(name, alias, ActionType.SetVariableAction, clock)
        {
        }

        [JsonConstructor]
        public SetVariableActionData()
            : base(ActionType.SetVariableAction)
        {
        }

        [JsonProperty("variableName")]
        public string VariableName { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        public void SetParameters(string path, string variableName, object value)
        {
            this.VariableName = variableName;
            this.Path = path;
            this.Value = value;
        }
    }
}

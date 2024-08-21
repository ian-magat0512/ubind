// <copyright file="IterateActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Object;

    /// <summary>
    /// Represents the data of an <see cref="IterateAction"/>.
    /// </summary>
    public class IterateActionData : ActionData
    {
        public IterateActionData(string name, string alias, IClock clock)
            : base(name, alias, ActionType.IterateAction, clock)
        {
        }

        [JsonConstructor]
        public IterateActionData()
            : base(ActionType.IterateAction)
        {
        }

        [JsonProperty("startIndex")]
        public long? StartIndex { get; set; }

        [JsonProperty("endIndex")]
        public long? EndIndex { get; set; }

        [JsonProperty("reverse")]
        public bool Reverse { get; set; }

        [JsonProperty("iterationsCompleted")]
        public int IterationsCompleted { get; set; } = 0;

        [JsonProperty("currentIteration")]
        public IterationItem? CurrentIteration { get; set; }

        [JsonProperty("previousIteration")]
        public IterationItem? PreviousIteration { get; set; }

        [JsonProperty("lastIteration")]
        public IterationItem? LastIteration { get; set; }
    }
}

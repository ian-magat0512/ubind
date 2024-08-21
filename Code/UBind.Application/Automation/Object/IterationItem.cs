// <copyright file="IterationItem.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Object
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Actions;

    /// <summary>
    /// This class represents an iterated item of an <see cref="IterateAction"/>.
    /// This can be accessed in the automation data of any iterate action.
    /// </summary>
    public class IterationItem
    {
        public IterationItem(object iterationItem, int itemIndexInList, int currentIterationCount)
        {
            this.Item = iterationItem;
            this.Index = itemIndexInList;
            this.Count = currentIterationCount;
        }

        /// <summary>
        /// Gets the item being iterated over.
        /// </summary>
        [JsonProperty("item")]
        public object Item { get; }

        /// <summary>
        /// Gets the index of the item, in relation to its list.
        /// </summary>
        [JsonProperty("index")]
        public int Index { get; }

        /// <summary>
        /// Gets the number of iterations completed before this iteration started.
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; }

        [JsonProperty("actions")]
        public Dictionary<string, ActionData> Actions { get; private set; } = new Dictionary<string, ActionData>();

        /// <summary>
        /// Updates the collection of action data with the data of an action for which this iteration item has been executed over.
        /// </summary>
        /// <param name="alias">The alias of the action.</param>
        /// <param name="actionData">The data of the action.</param>
        public void UpdateActionsExecuted(string alias, ActionData actionData)
        {
            if (!this.Actions.ContainsKey(alias))
            {
                this.Actions.Add(alias, actionData);
            }
        }

        /// <summary>
        /// Returns a new instance of this class whose values are taken out of this instance.
        /// </summary>
        /// <returns>A new iteration item.</returns>
        public IterationItem Clone()
        {
            var iteration = new IterationItem(this.Item, this.Index, this.Count);
            foreach (var actionData in this.Actions)
            {
                // We want a deep clone - referenced object should not be copied over.
                var serializedDatum = JsonConvert.SerializeObject(actionData.Value);
                var value = JsonConvert.DeserializeObject<ActionData>(
                    serializedDatum, AutomationDeserializationConfiguration.DataSettings);
                iteration.UpdateActionsExecuted(actionData.Key, value);
            }

            return iteration;
        }
    }
}

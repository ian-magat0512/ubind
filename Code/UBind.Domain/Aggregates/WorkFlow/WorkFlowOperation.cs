// <copyright file="WorkFlowOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Workflow
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the workflow transitions applicable.
    /// </summary>
    public class WorkFlowOperation
    {
        /// <summary>
        /// Gets or sets the action for the specified transition.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the list of required states before a transition can continue.
        /// </summary>
        public List<string> RequiredStates { get; set; }

        /// <summary>
        /// Gets or sets the resulting state once a transition has completed.
        /// </summary>
        public string? ResultingState { get; set; }
    }
}

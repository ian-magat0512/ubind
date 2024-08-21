// <copyright file="EventExporterCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Threading.Tasks;

    /// <summary>
    /// Condition that is triggered by an event.
    /// </summary>
    public abstract class EventExporterCondition
    {
        /// <summary>
        /// Gets or sets information about the result of the evaluation, and why it resulted in that.
        /// </summary>
        public string DebugInfo { get; protected set; }

        /// <summary>
        /// Evaluates the condition to determine whether it has been met.
        /// </summary>
        /// <param name="applicationEvent">The application event.</param>
        /// <returns>Boolean value if the condition has been met or not.</returns>
        public abstract Task<bool> Evaluate(Domain.ApplicationEvent applicationEvent);
    }
}

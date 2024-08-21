// <copyright file="ReplayOnlyEventExporterCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Threading.Tasks;
    using UBind.Domain;

    /// <summary>
    /// Factory for replay only conditions.
    /// </summary>
    public class ReplayOnlyEventExporterCondition : EventExporterCondition
    {
        /// <inheritdoc/>
        public override Task<bool> Evaluate(ApplicationEvent applicationEvent)
        {
            this.DebugInfo
                = "When working out whether to replay the event, isRetriggering was "
                + (applicationEvent.IsRetriggering ? "TRUE" : "FALSE");
            return Task.FromResult(applicationEvent.IsRetriggering);
        }
    }
}

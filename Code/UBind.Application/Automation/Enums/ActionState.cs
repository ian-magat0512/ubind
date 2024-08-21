// <copyright file="ActionState.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Enums
{
    using System.ComponentModel;

    /// <summary>
    /// Different action states.
    /// </summary>
    public enum ActionState
    {
        /// <summary>
        /// The action has not started yet.
        /// </summary>
        [Description("notStarted")]
        NotStarted = 0,

        /// <summary>
        /// The action has started.
        /// </summary>
        [Description("started")]
        Started,

        /// <summary>
        /// The action is currently checking for before-run error conditions.
        /// </summary>
        [Description("beforeRunErrorChecking")]
        BeforeRunErrorChecking,

        /// <summary>
        /// The action is currently running.
        /// </summary>
        [Description("running")]
        Running,

        /// <summary>
        /// The action is currently checking for after-run error conditions.
        /// </summary>
        [Description("afterRunErrorChecking")]
        AfterRunErrorChecking,

        /// <summary>
        /// The action is complete.
        /// </summary>
        [Description("completed")]
        Completed,

        [Description("unknown")]
        Unknown,
    }
}

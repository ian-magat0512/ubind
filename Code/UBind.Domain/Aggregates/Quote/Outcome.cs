// <copyright file="Outcome.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    /// <summary>
    /// Represents the outcome of an action that can succeed, fail without error or encounter erros..
    /// </summary>
    public enum Outcome
    {
        /// <summary>
        /// The action was successful.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The action executed without error, but was not successful.
        /// </summary>
        Failed = 1,

        /// <summary>
        /// There was an error attmpting to execute the action.
        /// </summary>
        Error = 2,
    }
}

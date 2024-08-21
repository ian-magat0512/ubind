// <copyright file="ContinuationsSupportIncludingFailedStateAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Imports
{
    using System.Collections.Generic;
    using Hangfire;
    using Hangfire.States;

    /// <summary>
    /// Custom hangfire attribute that will allow to continue the dependent job when the parent job failed.
    /// </summary>
    public class ContinuationsSupportIncludingFailedStateAttribute : ContinuationsSupportAttribute
    {
        public ContinuationsSupportIncludingFailedStateAttribute()
            : this(new[] { SucceededState.StateName, DeletedState.StateName, FailedState.StateName })
        {
        }

        public ContinuationsSupportIncludingFailedStateAttribute(
            string[] knownFinalStates)
            : base(new HashSet<string>(knownFinalStates))
        {
        }
    }
}

// <copyright file="IContextEntitySettings.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Configuration
{
    public interface IContextEntitySettings
    {
        /// <summary>
        /// Gets or sets which paths within the context entities should be resolved and included in the context entities object.
        /// </summary>
        string[] IncludeContextEntities { get; set; }

        /// <summary>
        /// Gets or sets the interval at which the context entities object should be automatically reloaded (regardless of other operations).
        /// </summary>
        int ReloadIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets the list of operations (other than load - if that counts as an operation)
        /// which should perform a second request after the initial (existing) request,
        /// to obtain an updated copy of the context entities object.
        /// </summary>
        string[] ReloadWithOperations { get; set; }
    }
}

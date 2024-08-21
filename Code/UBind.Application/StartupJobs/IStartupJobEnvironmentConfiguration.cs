// <copyright file="IStartupJobEnvironmentConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.StartupJobs
{
    /// <summary>
    /// Configuration to be used by the startup job runner when executing startup jobs.
    /// </summary>
    public interface IStartupJobEnvironmentConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current deployment environment is multinode.
        /// </summary>
        bool MultiNodeEnvironment { get; set; }
    }
}

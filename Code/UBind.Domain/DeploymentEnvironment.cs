// <copyright file="DeploymentEnvironment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Enumeration of the supported environments.
    /// </summary>
    public enum DeploymentEnvironment
    {
        /// <summary>
        /// No environment specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Dev environment (uses the current product configuration).
        /// </summary>
        Development = 1,

        /// <summary>
        /// Test environment for reviewing a given release as close to production as possible.
        /// </summary>
        Staging = 2,

        /// <summary>
        /// Production environment.
        /// </summary>
        Production = 3,
    }
}

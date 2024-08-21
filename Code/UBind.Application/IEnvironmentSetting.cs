// <copyright file="IEnvironmentSetting.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using UBind.Domain;

    /// <summary>
    /// These are settings grouped per environment.
    /// This is the configuration usually coming from appsettings.json.
    /// </summary>
    public interface IEnvironmentSetting<TSetting>
        where TSetting : class, new()
    {
        /// <summary>
        /// Gets the settings of develop envionment.
        /// </summary>
        TSetting Development { get; }

        /// <summary>
        /// Gets the settings of staging envionment.
        /// </summary>
        TSetting Staging { get; }

        /// <summary>
        /// Gets the settings of production envionment.
        /// </summary>
        TSetting Production { get; }

        /// <summary>
        /// Gets the settings of a specific environment.
        /// </summary>
        TSetting GetByEnvironment(DeploymentEnvironment deploymentEnvironment);
    }
}

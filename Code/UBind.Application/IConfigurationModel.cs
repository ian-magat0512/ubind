// <copyright file="IConfigurationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using UBind.Domain;

    /// <summary>
    /// For generating payment configurations for a given environment.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to generate.</typeparam>
    public interface IConfigurationModel<TConfiguration>
    {
        /// <summary>
        /// Generate a configuration of a given type for a given environment.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>The configuration for that environment.</returns>
        TConfiguration Generate(DeploymentEnvironment environment);
    }
}

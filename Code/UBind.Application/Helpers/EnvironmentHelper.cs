// <copyright file="EnvironmentHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Helpers
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Static helpers for environment.
    /// </summary>
    public static class EnvironmentHelper
    {
        /// <summary>
        /// Parses the environment or throws an exception if parsing fails.
        /// </summary>
        /// <param name="environment">The string environment.</param>
        /// <returns>The DeploymentEnvironment.</returns>
        public static DeploymentEnvironment ParseEnvironmentOrThrow(string environment)
        {
            var isSuccess = Enum.TryParse(environment, true, out DeploymentEnvironment env);
            if (!isSuccess)
            {
                throw new ErrorException(Errors.General.NotFound("environment", environment));
            }

            return env;
        }

        /// <summary>
        /// Parses the environment or throws an exception if parsing fails.
        /// Only throws if environment passed was not null.
        /// </summary>
        /// <param name="environment">The string environment.</param>
        /// <returns>The DeploymentEnvironment, or null.</returns>
        public static DeploymentEnvironment? ParseOptionalEnvironmentOrThrow(string environment)
        {
            if (string.IsNullOrEmpty(environment))
            {
                return null;
            }

            var isSuccess = Enum.TryParse(environment, true, out DeploymentEnvironment env);
            if (!isSuccess)
            {
                throw new ErrorException(Errors.General.NotFound("environment", environment));
            }

            return (DeploymentEnvironment?)env;
        }

        /// <summary>
        /// Throw an exception if the environment doesn't match.
        /// </summary>
        /// <param name="first">The first enviroment.</param>
        /// <param name="second">The second enviroment.</param>
        /// <param name="resourceTypeName">The name of the resource type being accessed.</param>
        public static void ThrowIfEnvironmentDoesNotMatch(
            DeploymentEnvironment first,
            DeploymentEnvironment second,
            string resourceTypeName)
        {
            if (first != second)
            {
                throw new ErrorException(Errors.Operations.EnvironmentMisMatch(resourceTypeName));
            }
        }

        /// <summary>
        /// Throw an exception if the environment doesn't match.
        /// </summary>
        /// <param name="entityEnvironment">The enviroment of the entity.</param>
        /// <param name="passedEnvironment">The environment that it's expected to be.</param>
        /// <param name="resourceTypeName">The name of the resource type being accessed.</param>
        public static void ThrowIfEnvironmentDoesNotMatchIfPassed(
            DeploymentEnvironment entityEnvironment,
            string passedEnvironment,
            string resourceTypeName)
        {
            if (passedEnvironment != null)
            {
                DeploymentEnvironment env = ParseEnvironmentOrThrow(passedEnvironment);
                if (entityEnvironment != env)
                {
                    throw new ErrorException(Errors.Operations.EnvironmentMisMatch(resourceTypeName));
                }
            }
        }
    }
}

// <copyright file="IBuilder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;

    /// <summary>
    /// Builds an automation entity of type T from properties, which are fulfilled using a json deserialiser.
    /// </summary>
    /// <typeparam name="T">The automation entity to be instantiated.</typeparam>
    public interface IBuilder<out T>
    {
        /// <summary>
        /// Constructs the Automation entity and returns it.
        /// </summary>
        /// <param name="dependencyProvider">A service provider that can provide dependencies that automation entities may need to have access to.</param>
        /// <returns>The Automation entity.</returns>
        T Build(IServiceProvider dependencyProvider);
    }
}

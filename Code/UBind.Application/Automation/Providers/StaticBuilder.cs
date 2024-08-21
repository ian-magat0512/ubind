// <copyright file="StaticBuilder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers
{
    using System;
    using UBind.Application.Automation;

    /// <summary>
    /// Builder which builds a provider from a static value.
    /// This is used to set properties of ConfigModels from automations.json where the value is written in as a string into the json directly.
    /// </summary>
    /// <typeparam name="T">The tpe of the value which will be finally provided.</typeparam>
    public class StaticBuilder<T> : IBuilder<IProvider<T>>
    {
        /// <summary>
        /// Gets or sets the static value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Builds the given model using the static value.
        /// </summary>
        /// <param name="dependencyProvider">The dependency provider.</param>
        /// <returns>Returns an instance of the <see cref="StaticProvider{TValue}"/>.</returns>
        public IProvider<T> Build(IServiceProvider dependencyProvider)
        {
            return new StaticProvider<T>(this.Value);
        }
    }
}

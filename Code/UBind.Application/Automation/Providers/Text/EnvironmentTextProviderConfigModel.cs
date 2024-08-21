// <copyright file="EnvironmentTextProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System;

    /// <summary>
    /// Model for building an instance of <see cref="EnvironmentTextProvider"/>.
    /// </summary>
    public class EnvironmentTextProviderConfigModel : IBuilder<IProvider<Data<string>>>
    {
        /// <summary>
        /// Gets or sets the text provider to use when no provider is specified for the current environment.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Default { get; set; }

        /// <summary>
        /// Gets or sets the text provider to use in the development environment.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Development { get; set; }

        /// <summary>
        /// Gets or sets the text provider to use in the stagingt environment.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Staging { get; set; }

        /// <summary>
        /// Gets or sets the text provider to use in the production environment.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Production { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<string>> Build(IServiceProvider dependencyProvider)
        {
            return new EnvironmentTextProvider(
                     this.Default?.Build(dependencyProvider),
                     this.Development?.Build(dependencyProvider),
                     this.Staging?.Build(dependencyProvider),
                     this.Production?.Build(dependencyProvider));
        }
    }
}

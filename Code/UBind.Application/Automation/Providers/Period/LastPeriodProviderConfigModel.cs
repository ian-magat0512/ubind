// <copyright file="LastPeriodProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Period
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for parsing automation configuration specifying an instance of
    /// <see cref="LastPeriodProvider"/>.
    /// </summary>
    public class LastPeriodProviderConfigModel : IBuilder<IProvider<Data<Interval>>>
    {
        /// <summary>
        /// Gets or sets the config model for the duration provider.
        /// </summary>
        public IBuilder<IProvider<Data<NodaTime.Period>>> Duration { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<Interval>> Build(IServiceProvider dependencyProvider)
        {
            return new LastPeriodProvider(
                this.Duration.Build(dependencyProvider),
                dependencyProvider.GetRequiredService<IClock>());
        }
    }
}

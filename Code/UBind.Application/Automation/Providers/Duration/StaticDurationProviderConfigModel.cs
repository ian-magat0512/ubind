// <copyright file="StaticDurationProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Duration
{
    using System;

    /// <summary>
    /// Represents the text duration configuration model defined by a text provider.
    /// </summary>
    public class StaticDurationProviderConfigModel : IBuilder<IProvider<Data<NodaTime.Period>>>
    {
        /// <summary>
        /// Gets the value of this text duration provider config model.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Value { get; internal set; }

        /// <inheritdoc/>
        public IProvider<Data<NodaTime.Period>> Build(IServiceProvider dependencyProvider)
        {
            return new StaticDurationProvider(this.Value.Build(dependencyProvider));
        }
    }
}

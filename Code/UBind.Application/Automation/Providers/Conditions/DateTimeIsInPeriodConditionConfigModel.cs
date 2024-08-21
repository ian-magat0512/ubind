// <copyright file="DateTimeIsInPeriodConditionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Returns a boolean true if a give date/time is in a given period..
    /// </summary>
    public class DateTimeIsInPeriodConditionConfigModel : IBuilder<IProvider<Data<bool>>>
    {
        /// <summary>
        /// Gets the config model for a date/time provider.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<Instant>>> DateTime { get; private set; }

        /// <summary>
        /// Gets the config model for a period provider.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<Interval>>> IsInPeriod { get; private set; }

        /// <inheritdoc/>
        public IProvider<Data<bool>> Build(IServiceProvider dependencyProvider)
        {
            var dateTimeProvider = this.DateTime.Build(dependencyProvider);
            var periodProvider = this.IsInPeriod.Build(dependencyProvider);
            return new DateTimeIsInPeriodCondition(
                dateTimeProvider, periodProvider);
        }
    }
}

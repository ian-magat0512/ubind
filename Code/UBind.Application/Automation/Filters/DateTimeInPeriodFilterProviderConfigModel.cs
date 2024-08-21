// <copyright file="DateTimeInPeriodFilterProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;

    /// <summary>
    /// Config model for an expression-based filter that tests whether a given date time in in a given period.
    /// </summary>
    public class DateTimeInPeriodFilterProviderConfigModel : IBuilder<IFilterProvider>
    {
        /// <summary>
        /// Gets a config model for a date time provider.
        /// </summary>
        [JsonProperty]
        public IDataExpressionProviderConfigModel<long> DateTime { get; private set; }

        /// <summary>
        /// Gets a config model for a period provider.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<Interval>>> IsInPeriod { get; private set; }

        /// <inheritdoc/>
        public IFilterProvider Build(IServiceProvider dependencyProvider)
        {
            return new DateTimeInPeriodFilterProvider(
                this.DateTime.Build(dependencyProvider),
                this.IsInPeriod.Build(dependencyProvider));
        }
    }
}

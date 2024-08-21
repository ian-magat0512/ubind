// <copyright file="PeriodTypeValueDurationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Duration
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Enums;

    /// <summary>
    /// Represents an instance of period type value duration config model to build period type value duration.
    /// </summary>
    public class PeriodTypeValueDurationConfigModel : IBuilder<IProvider<Data<NodaTime.Period>>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodTypeValueDurationConfigModel"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="periodType">The period type.</param>
        [JsonConstructor]
        public PeriodTypeValueDurationConfigModel(IBuilder<IProvider<Data<long>>> value, PeriodType periodType)
        {
            this.Value = value;
            this.PeriodType = periodType;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public IBuilder<IProvider<Data<long>>> Value { get; }

        /// <summary>
        /// Gets period type.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public PeriodType PeriodType { get; }

        /// <inheritdoc/>
        public IProvider<Data<NodaTime.Period>> Build(IServiceProvider dependencyProvider)
        {
            return new PeriodTypeValueDurationProvider(this.Value.Build(dependencyProvider), this.PeriodType);
        }
    }
}

// <copyright file="TimeIsBeforeOrEqualToConditionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;
    using NodaTime;

    /// <summary>
    /// Model for building an condition that tests if a given time is before or equal to a second given time.
    /// </summary>
    public class TimeIsBeforeOrEqualToConditionConfigModel : IBuilder<IProvider<Data<bool>>>
    {
        /// <summary>
        /// Gets or sets the first time to compare.
        /// </summary>
        public IBuilder<IProvider<Data<LocalTime>>> Time { get; set; }

        /// <summary>
        /// Gets or sets the second time to compare.
        /// </summary>
        public IBuilder<IProvider<Data<LocalTime>>> IsBeforeOrEqualTo { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<bool>> Build(IServiceProvider dependencyProvider)
        {
            return new ComparisonCondition<LocalTime>(
                this.Time.Build(dependencyProvider),
                this.IsBeforeOrEqualTo.Build(dependencyProvider),
                (first, second) => first <= second,
                "timeIsBeforeOrEqualToCondition");
        }
    }
}

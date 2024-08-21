// <copyright file="DateTimeIsEqualToConditionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;
    using NodaTime;

    /// <summary>
    /// Model for building an condition that tests if a given datetime is equal to a second given datetime.
    /// </summary>
    public class DateTimeIsEqualToConditionConfigModel : IBuilder<IProvider<Data<bool>>>
    {
        /// <summary>
        /// Gets or sets the first datetime to compare.
        /// </summary>
        public IBuilder<IProvider<Data<Instant>>> DateTime { get; set; }

        /// <summary>
        /// Gets or sets the second datetime to compare.
        /// </summary>
        public IBuilder<IProvider<Data<Instant>>> IsEqualTo { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<bool>> Build(IServiceProvider dependencyProvider)
        {
            return new ComparisonCondition<Instant>(
                this.DateTime.Build(dependencyProvider),
                this.IsEqualTo.Build(dependencyProvider),
                (first, second) => first.ToDateTimeUtc() == second.ToDateTimeUtc(),
                "dateTimeIsEqualToCondition");
        }
    }
}

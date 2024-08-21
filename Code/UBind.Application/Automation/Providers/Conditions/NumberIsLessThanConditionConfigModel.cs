﻿// <copyright file="NumberIsLessThanConditionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;

    /// <summary>
    /// Model for building an condition that tests if a given number is less than to a second given number.
    /// </summary>
    public class NumberIsLessThanConditionConfigModel : IBuilder<IProvider<Data<bool>>>
    {
        /// <summary>
        /// Gets or sets the first number to compare.
        /// </summary>
        public IBuilder<IProvider<Data<decimal>>> Number { get; set; }

        /// <summary>
        /// Gets or sets the second number to compare.
        /// </summary>
        public IBuilder<IProvider<Data<decimal>>> IsLessThan { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<bool>> Build(IServiceProvider dependencyProvider)
        {
            return new ComparisonCondition<decimal>(
                this.Number.Build(dependencyProvider),
                this.IsLessThan.Build(dependencyProvider),
                (first, second) => first < second,
                "numberIsLessThanCondition");
        }
    }
}

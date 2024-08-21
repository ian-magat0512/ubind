// <copyright file="ErrorConditionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Error
{
    using System;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for building <see cref="ErrorCondition"/>.
    /// </summary>
    public class ErrorConditionConfigModel : IBuilder<ErrorCondition>
    {
        /// <summary>
        /// Gets or sets the condition to be evaluated.
        /// </summary>
        public IBuilder<IProvider<Data<bool>>> Condition { get; set; }

        /// <summary>
        /// Gets or sets the error to be raised if condition is true.
        /// </summary>
        public ErrorProviderConfigModel Error { get; set; }

        /// <inheritdoc/>
        public ErrorCondition Build(IServiceProvider dependencyProvider)
        {
            return new ErrorCondition(
                this.Condition.Build(dependencyProvider),
                this.Error.Build(dependencyProvider));
        }
    }
}

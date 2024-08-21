// <copyright file="OrConditionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Model for building an instance of <see cref="OrCondition"/>.
    /// </summary>
    public class OrConditionConfigModel : IBuilder<IProvider<Data<bool>>>
    {
        /// <summary>
        /// Gets or sets a collection of conditions to be used in the OR evaluation.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<Data<bool>>>> Conditions { get; set; } = Enumerable.Empty<IBuilder<IProvider<Data<bool>>>>();

        /// <inheritdoc/>
        public IProvider<Data<bool>> Build(IServiceProvider dependencyProvider)
        {
            var conditions = this.Conditions.Select(c => c.Build(dependencyProvider)).ToList();
            return new OrCondition(conditions);
        }
    }
}

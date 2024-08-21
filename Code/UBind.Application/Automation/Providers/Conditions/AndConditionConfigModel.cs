// <copyright file="AndConditionConfigModel.cs" company="uBind">
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
    /// Model for AND automation condition.
    /// </summary>
    public class AndConditionConfigModel : IBuilder<IProvider<Data<bool>>>
    {
        /// <summary>
        /// Gets or sets the conditions that will be used for evaluation of an AND condition.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<Data<bool>>>> Conditions { get; set; } = Enumerable.Empty<IBuilder<IProvider<Data<bool>>>>();

        /// <inheritdoc/>
        public IProvider<Data<bool>> Build(IServiceProvider dependencyProvider)
        {
            var conditions = this.Conditions.Select(c => c.Build(dependencyProvider)).ToList();
            var model = new AndCondition(conditions);
            return model;
        }
    }
}

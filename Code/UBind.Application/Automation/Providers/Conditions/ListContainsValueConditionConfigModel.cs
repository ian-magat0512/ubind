// <copyright file="ListContainsValueConditionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;
    using UBind.Application.Automation.Providers.List;

    /// <summary>
    /// Model for building an condition that tests if a given list is contains the value.
    /// </summary>
    public class ListContainsValueConditionConfigModel : IBuilder<IProvider<Data<bool>>>
    {
        /// <summary>
        /// Gets or sets the list of object to check.
        /// </summary>
        public IBuilder<IDataListProvider<object>> List { get; set; }

        /// <summary>
        /// Gets or sets the value is looking for in the list.
        /// </summary>
        public IBuilder<IProvider<IData>> Value { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<bool>> Build(IServiceProvider dependencyProvider)
        {
            return new ListContainsValueCondition(
                this.List.Build(dependencyProvider),
                this.Value.Build(dependencyProvider));
        }
    }
}

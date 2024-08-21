// <copyright file="ObjectContainsPropertyConditionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Model for building an instance of <see cref="ObjectContainsPropertyCondition"/>.
    /// </summary>
    public class ObjectContainsPropertyConditionConfigModel : IBuilder<IProvider<Data<bool>>>
    {
        /// <summary>
        /// Gets or sets the name of the property to be checked.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the object to be tested.
        /// </summary>
        public IBuilder<IObjectProvider> Object { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<bool>> Build(IServiceProvider dependencyProvider)
        {
            return new ObjectContainsPropertyCondition(
                this.Object.Build(dependencyProvider),
                this.PropertyName.Build(dependencyProvider));
        }
    }
}

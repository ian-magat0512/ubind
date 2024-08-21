// <copyright file="TextEndsWithConditionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;

    /// <summary>
    /// Model for defining an instance of <see cref="TextEndsWithCondition"/>.
    /// </summary>
    public class TextEndsWithConditionConfigModel : IBuilder<IProvider<Data<bool>>>
    {
        /// <summary>
        /// Gets or sets the text value that will be tested.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Text { get; set; }

        /// <summary>
        /// Gets or sets the text value the test will be performed with.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> EndsWith { get; set; }

        /// <summary>
        /// Gets or sets the boolean value if will ignore the case.
        /// </summary>
        public IBuilder<IProvider<Data<bool>>> IgnoreCase { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<bool>> Build(IServiceProvider dependencyProvider)
        {
            return new TextEndsWithCondition(
                this.Text.Build(dependencyProvider),
                this.EndsWith.Build(dependencyProvider),
                this.IgnoreCase?.Build(dependencyProvider));
        }
    }
}

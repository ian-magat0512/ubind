// <copyright file="ObjectPropertyConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Object
{
    using System;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for creating an instance of <see cref="ObjectProperty"/>.
    /// </summary>
    public class ObjectPropertyConfigModel : IBuilder<ObjectProperty>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> PropertyName { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        public IBuilder<IProvider<IData>>? Value { get; set; }

        /// <inheritdoc/>
        public ObjectProperty Build(IServiceProvider dependencyProvider)
        {
            return new ObjectProperty(this.PropertyName.Build(dependencyProvider), this.Value?.Build(dependencyProvider));
        }
    }
}

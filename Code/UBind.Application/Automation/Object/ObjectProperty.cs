// <copyright file="ObjectProperty.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Object
{
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents a property of an object.
    /// </summary>
    public class ObjectProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectProperty"/> class.
        /// </summary>
        /// <param name="nameProvider">The property name provider.</param>
        /// <param name="valueProvider">The property value provider.</param>
        public ObjectProperty(IProvider<Data<string>> nameProvider, IProvider<IData>? valueProvider)
        {
            this.Name = nameProvider;
            this.Value = valueProvider;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public IProvider<Data<string>> Name { get; }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public IProvider<IData>? Value { get; }
    }
}

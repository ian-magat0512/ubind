// <copyright file="ObjectPathProperty.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Object
{
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents a property of an object, where the value is resolved via a path.
    /// If value is not found, a default value is used, if configured.
    /// </summary>
    public class ObjectPathProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPathProperty"/> class.
        /// </summary>
        /// <param name="propertyNameProvider">The property name provider to be used.</param>
        /// <param name="pathProvider">The path provider to be used.</param>
        /// <param name="defaultValueProvider">The default value provider, if any.</param>
        public ObjectPathProperty(
            IProvider<Data<string>> propertyNameProvider,
            IProvider<Data<string>> pathProvider,
            IProvider<IData>? defaultValueProvider,
            IProvider<Data<bool>>? raiseErrorIfNotFoundProvider,
            IProvider<IData>? valueIfNotFoundProvider,
            IProvider<Data<bool>>? raiseErrorIfNullProvider,
            IProvider<IData>? valueIfNullProvider)
        {
            this.PropertyNameProvider = propertyNameProvider;
            this.PathProvider = pathProvider;
            this.DefaultValueProvider = defaultValueProvider;
            this.RaiseErrorIfNotFoundProvider = raiseErrorIfNotFoundProvider;
            this.ValueIfNotFoundProvider = valueIfNotFoundProvider;
            this.RaiseErrorIfNullProvider = raiseErrorIfNullProvider;
            this.ValueIfNullProvider = valueIfNullProvider;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public IProvider<Data<string>> PropertyNameProvider { get; }

        /// <summary>
        /// Gets the path used to perform the lookup to resolve the value.
        /// </summary>
        public IProvider<Data<string>> PathProvider { get; }

        /// <summary>
        /// Gets the default value to be used if the path lookup was unable to be resolved.
        /// </summary>
        public IProvider<IData>? DefaultValueProvider { get; }

        public IProvider<Data<bool>>? RaiseErrorIfNotFoundProvider { get; }

        public IProvider<IData>? ValueIfNotFoundProvider { get; }

        public IProvider<Data<bool>>? RaiseErrorIfNullProvider { get; }

        public IProvider<IData>? ValueIfNullProvider { get; }
    }
}

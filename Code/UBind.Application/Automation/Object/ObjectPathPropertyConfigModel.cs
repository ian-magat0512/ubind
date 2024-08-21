// <copyright file="ObjectPathPropertyConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Object
{
    using System;
    using System.Diagnostics.Contracts;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// This class is used to build an instance of <see cref="ObjectPathProperty"/>.
    /// </summary>
    public class ObjectPathPropertyConfigModel : IBuilder<ObjectPathProperty>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPathPropertyConfigModel"/> class.
        /// </summary>
        /// <param name="propertyName">The property name provider builder.</param>
        /// <param name="path">The path value provider builder.</param>
        /// <param name="defaultValue">The default value provider builder.</param>
        [JsonConstructor]
        public ObjectPathPropertyConfigModel(
            IBuilder<IProvider<Data<string>>> propertyName,
            IBuilder<IProvider<Data<string>>> path,
            IBuilder<IProvider<IData>> defaultValue,
            IBuilder<IProvider<Data<bool>>> raiseErrorIfNotFound,
            IBuilder<IProvider<IData>> valueIfNotFound,
            IBuilder<IProvider<Data<bool>>> raiseErrorIfNull,
            IBuilder<IProvider<IData>> valueIfNull)
        {
            Contract.Assert(propertyName != null);
            Contract.Assert(path != null);

            this.PropertyName = propertyName;
            this.Path = path;
            this.DefaultValue = defaultValue;
            this.RaiseErrorIfNotFound = raiseErrorIfNotFound;
            this.ValueIfNotFound = valueIfNotFound;
            this.RaiseErrorIfNull = raiseErrorIfNull;
            this.ValueIfNull = valueIfNull;
        }

        /// <summary>
        /// Gets the name of this property on the output object.
        /// </summary>
        [JsonProperty("propertyName")]
        public IBuilder<IProvider<Data<string>>> PropertyName { get; }

        /// <summary>
        /// Gets the path used to perform the lookup to obtain a value for the output property.
        /// </summary>
        [JsonProperty("path")]
        public IBuilder<IProvider<Data<string>>> Path { get; }

        /// <summary>
        /// Gets the value that will be set on the output property if the path lookup was unable to resolve a value.
        /// </summary>
        [JsonProperty("defaultValue")]
        public IBuilder<IProvider<IData>> DefaultValue { get; }

        [JsonProperty("raiseErrorIfNotFound")]
        public IBuilder<IProvider<Data<bool>>> RaiseErrorIfNotFound { get; }

        [JsonProperty("valueIfNotFound")]
        public IBuilder<IProvider<IData>> ValueIfNotFound { get; }

        [JsonProperty("raiseErrorIfNull")]
        public IBuilder<IProvider<Data<bool>>> RaiseErrorIfNull { get; }

        [JsonProperty("valueIfNull")]
        public IBuilder<IProvider<IData>> ValueIfNull { get; }

        /// <inheritdoc/>
        public ObjectPathProperty Build(IServiceProvider dependencyProvider)
        {
            return new ObjectPathProperty(
                this.PropertyName.Build(dependencyProvider),
                this.Path.Build(dependencyProvider),
                this.DefaultValue?.Build(dependencyProvider),
                this.RaiseErrorIfNotFound?.Build(dependencyProvider),
                this.ValueIfNotFound?.Build(dependencyProvider),
                this.RaiseErrorIfNull?.Build(dependencyProvider),
                this.ValueIfNull?.Build(dependencyProvider));
        }
    }
}

// <copyright file="PathLookupValueTypeConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.PathLookup;

using UBind.Application.Automation.Providers;

/// <summary>
/// Model for building an instance of <see cref="PathLookupValueTypeProvider{TValue}"/>.
/// Some of the properties are optional, and some are required.
/// This is because the model is used for building the provider, and the provider
/// have same properties as the model.
/// </summary>
/// <typeparam name="TModel">The provider config model</typeparam>
/// <typeparam name="TProviderValue">The provider value type.</typeparam>
public class PathLookupValueTypeConfigModel<TModel, TProviderValue> : IBuilder<TProviderValue>
{
    public IBuilder<IObjectPathLookupProvider> PathLookup { get; set; }

    public IBuilder<IProvider<IData>> ValueIfNotFound { get; set; }

    public IBuilder<IProvider<Data<bool>>> RaiseErrorIfNotFound { get; set; }

    public IBuilder<IProvider<Data<bool>>> RaiseErrorIfNull { get; set; }

    public IBuilder<IProvider<IData>> ValueIfNull { get; set; }

    public IBuilder<IProvider<Data<bool>>> RaiseErrorIfTypeMismatch { get; set; }

    public IBuilder<IProvider<IData>> ValueIfTypeMismatch { get; set; }

    public IBuilder<IProvider<IData>> DefaultValue { get; set; }

    public virtual TProviderValue Build(IServiceProvider dependencyProvider)
    {
        return (TProviderValue)Activator.CreateInstance(
            typeof(TModel),
            new object[] {
                this.PathLookup.Build(dependencyProvider),
                this.ValueIfNotFound?.Build(dependencyProvider),
                this.RaiseErrorIfNotFound?.Build(dependencyProvider),
                this.RaiseErrorIfNull?.Build(dependencyProvider),
                this.ValueIfNull?.Build(dependencyProvider),
                this.RaiseErrorIfTypeMismatch?.Build(dependencyProvider),
                this.ValueIfTypeMismatch?.Build(dependencyProvider),
                this.DefaultValue?.Build(dependencyProvider)
            });
    }
}

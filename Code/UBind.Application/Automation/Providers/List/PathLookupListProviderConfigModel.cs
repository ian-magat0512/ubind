// <copyright file="PathLookupListProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List;

using Newtonsoft.Json;
using UBind.Application.Automation.PathLookup;

public class PathLookupListProviderConfigModel : PathLookupValueTypeConfigModel<PathLookupListProvider, IDataListProvider<object>>
{
    public PathLookupListProviderConfigModel()
    {
    }

    [JsonConstructor]
    public PathLookupListProviderConfigModel(
        IBuilder<IObjectPathLookupProvider> pathLookup,
        IBuilder<IProvider<IDataList<object>>> valueIfNotFound,
        IBuilder<IProvider<Data<bool>>> raiseErrorIfNotFound,
        IBuilder<IProvider<Data<bool>>> raiseErrorIfNull,
        IBuilder<IProvider<IDataList<object>>> valueIfNull,
        IBuilder<IProvider<Data<bool>>> raiseErrorIfTypeMismatch,
        IBuilder<IProvider<IDataList<object>>> valueIfTypeMismatch,
        IBuilder<IProvider<IDataList<object>>> defaultValue)
    {
        this.PathLookup = pathLookup;
        this.ValueIfNotFound = (IBuilder<IProvider<IData>>)valueIfNotFound;
        this.RaiseErrorIfNotFound = raiseErrorIfNotFound;
        this.RaiseErrorIfNull = raiseErrorIfNull;
        this.ValueIfNull = (IBuilder<IProvider<IData>>)valueIfNull;
        this.RaiseErrorIfTypeMismatch = raiseErrorIfTypeMismatch;
        this.ValueIfTypeMismatch = (IBuilder<IProvider<IData>>)valueIfTypeMismatch;
        this.DefaultValue = (IBuilder<IProvider<IData>>)defaultValue;
    }
}

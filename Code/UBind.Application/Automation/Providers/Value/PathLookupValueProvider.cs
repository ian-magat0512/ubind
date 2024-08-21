// <copyright file="PathLookupValueProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Value;

using MorseCode.ITask;
using UBind.Application.Automation.PathLookup;
using UBind.Application.Automation.Providers;

public class PathLookupValueProvider : IProvider<IData>
{
    private readonly IObjectPathLookupProvider path;
    private readonly IProvider<Data<bool>> raiseErrorIfNotFound;
    private readonly IProvider<IData> valueIfNotFound;
    private readonly IProvider<Data<bool>> raiseErrorIfNull;
    private readonly IProvider<IData> valueIfNull;
    private readonly IProvider<IData> defaultValue;

    public PathLookupValueProvider(
        IObjectPathLookupProvider path,
        IProvider<Data<bool>> raiseErrorIfNotFound,
        IProvider<IData> valueIfNotFound,
        IProvider<Data<bool>> raiseErrorIfNull,
        IProvider<IData> valueIfNull,
        IProvider<IData> defaultValue)
    {
        this.path = path;
        this.raiseErrorIfNotFound = raiseErrorIfNotFound;
        this.valueIfNotFound = valueIfNotFound;
        this.raiseErrorIfNull = raiseErrorIfNull;
        this.valueIfNull = valueIfNull;
        this.defaultValue = defaultValue;
    }

    public string SchemaReferenceKey => "objectPathLookupValue";

    public async ITask<IProviderResult<IData>> Resolve(IProviderContext providerContext)
    {
        IData lookupData = null;

        var lookupResult = await this.path.Resolve(providerContext);
        if (lookupResult.IsFailure)
        {
            lookupData = await PathLookupResolverHelper.ResolveValueOrThrowIfNotFound(
                        this.raiseErrorIfNotFound,
                        this.valueIfNotFound,
                        this.defaultValue,
                        providerContext,
                        this.SchemaReferenceKey,
                        lookupResult);
        }
        else
        {
            lookupData = lookupResult.Value;
        }

        if (lookupData == null)
        {
            lookupData = await PathLookupResolverHelper.ResolveValueOrThrowIfNull(
                        this.raiseErrorIfNull,
                        this.valueIfNull,
                        this.defaultValue,
                        providerContext,
                        this.SchemaReferenceKey,
                        lookupData);
        }

        return ProviderResult<IData>.Success(lookupData);
    }
}

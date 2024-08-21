// <copyright file="PathLookupListProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System.Collections.Generic;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.PathLookup;

    /// <summary>
    /// Resolves the path given against the chosen data object or automation data and returns the value if it is a valid
    /// list.
    /// </summary>
    /// <remarks>Schema key: objectPathLookupList.</remarks>
    public class PathLookupListProvider : IDataListProvider<object>
    {
        private readonly IObjectPathLookupProvider pathLookup;
        private readonly IDataListProvider<object> valueIfNotFound;
        private readonly IProvider<Data<bool>> raiseErrorIfNotFound;
        private readonly IProvider<Data<bool>> raiseErrorIfNull;
        private readonly IDataListProvider<object> valueIfNull;
        private readonly IProvider<Data<bool>> raiseErrorIfTypeMismatch;
        private readonly IDataListProvider<object> valueIfTypeMismatch;
        private readonly IDataListProvider<object> defaultValue;

        public PathLookupListProvider(
            IObjectPathLookupProvider pathLookup,
            IDataListProvider<object> valueIfNotFound,
            IProvider<Data<bool>> raiseErrorIfNotFound,
            IProvider<Data<bool>> raiseErrorIfNull,
            IDataListProvider<object> valueIfNull,
            IProvider<Data<bool>> raiseErrorIfTypeMismatch,
            IDataListProvider<object> valueIfTypeMismatch,
            IDataListProvider<object> defaultValue)
        {
            this.pathLookup = pathLookup;
            this.valueIfNotFound = valueIfNotFound;
            this.raiseErrorIfNotFound = raiseErrorIfNotFound;
            this.raiseErrorIfNull = raiseErrorIfNull;
            this.valueIfNull = valueIfNull;
            this.raiseErrorIfTypeMismatch = raiseErrorIfTypeMismatch;
            this.valueIfTypeMismatch = valueIfTypeMismatch;
            this.defaultValue = defaultValue;
        }

        public List<string> IncludedProperties { get; set; }

        public string SchemaReferenceKey => "objectPathLookupList";

        public async ITask<IProviderResult<IDataList<object>>> Resolve(IProviderContext providerContext)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            using (MiniProfiler.Current.Step(nameof(PathLookupListProvider) + "." + nameof(this.Resolve)))
            {
                IData lookupData = null;
                var lookupResult = await this.pathLookup.Resolve(providerContext);

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

                    if (lookupData == null)
                    {
                        return ProviderResult<IDataList<object>>.Success(null);
                    }
                }

                object pathLookupResult = lookupData.GetValueFromGeneric();
                if (pathLookupResult is JValue tokenValue)
                {
                    // if retrieved from a JSON object, then retrieve underlying value as object.
                    pathLookupResult = tokenValue.Value;
                }

                if (DataObjectHelper.IsArray(pathLookupResult))
                {
                    return ProviderResult<IDataList<object>>.Success(
                        new GenericDataList<object>(pathLookupResult as IEnumerable<object>));
                }
                else
                {
                    string typeName = TypeHelper.GetReadableTypeName(lookupData.GetValueFromGeneric());
                    IData valueIfTypeMismatch = await PathLookupResolverHelper.ResolveValueOrThrowIfTypeMismatch(
                        this.raiseErrorIfTypeMismatch,
                        this.valueIfTypeMismatch,
                        this.defaultValue,
                        providerContext,
                        typeName,
                        "list",
                        lookupData,
                        this.SchemaReferenceKey);

                    if (valueIfTypeMismatch == null)
                    {
                        return ProviderResult<IDataList<object>>.Success(null);
                    }

                    object valueIfTypeMismatchResult = valueIfTypeMismatch.GetValueFromGeneric();
                    return ProviderResult<IDataList<object>>.Success(
                        new GenericDataList<object>(valueIfTypeMismatchResult as IEnumerable<object>));
                }
            }
        }
    }
}

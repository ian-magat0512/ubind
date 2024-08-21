// <copyright file="PathLookupObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.PathLookup;

    /// <summary>
    /// Reads a data object from the automation data (or a custom data object) using a path defined by a text provider.
    /// </summary>
    public class PathLookupObjectProvider : IObjectProvider
    {
        private readonly IObjectPathLookupProvider objectPathLookup;
        private readonly IProvider<IData> valueIfNotFound;
        private readonly IProvider<Data<bool>> raiseErrorIfNotFound;
        private readonly IProvider<Data<bool>> raiseErrorIfNull;
        private readonly IProvider<IData> valueIfNull;
        private readonly IProvider<Data<bool>> raiseErrorIfTypeMismatch;
        private readonly IProvider<IData> valueIfTypeMismatch;
        private readonly IProvider<IData> defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathLookupObjectProvider"/> class.
        /// </summary>
        /// <param name="pathLookup">The object path lookup to be used.</param>
        /// <param name="defaultValue">The default value provider, if any.</param>
        public PathLookupObjectProvider(
            IObjectPathLookupProvider pathLookup,
            IProvider<IData> valueIfNotFound,
            IProvider<Data<bool>> raiseErrorIfNotFound,
            IProvider<Data<bool>> raiseErrorIfNull,
            IProvider<IData> valueIfNull,
            IProvider<Data<bool>> raiseErrorIfTypeMismatch,
            IProvider<IData> valueIfTypeMismatch,
            IProvider<IData> defaultValue)
        {
            this.objectPathLookup = pathLookup;
            this.valueIfNotFound = valueIfNotFound;
            this.raiseErrorIfNotFound = raiseErrorIfNotFound;
            this.raiseErrorIfNull = raiseErrorIfNull;
            this.valueIfNull = valueIfNull;
            this.raiseErrorIfTypeMismatch = raiseErrorIfTypeMismatch;
            this.valueIfTypeMismatch = valueIfTypeMismatch;
            this.defaultValue = defaultValue;
        }

        public string SchemaReferenceKey => "objectPathLookupObject";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<object>>> Resolve(IProviderContext providerContext)
        {
            IData lookupData = null;

            var lookupResult = await this.objectPathLookup.Resolve(providerContext);
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
                    return ProviderResult<Data<object>>.Success(null);
                }
            }

            JObject errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);

            try
            {
                var value = lookupData.GetValueFromGeneric();
                DataObjectHelper.ThrowIfNotObjectOrArray(value, this.GetType().Name, errorData);
                return ProviderResult<Data<object>>.Success(new Data<object>(value));
            }
            catch (Exception exception)
            {
                string typeName = TypeHelper.GetReadableTypeName(lookupData.GetValueFromGeneric());
                IData valueIfTypeMismatch = await PathLookupResolverHelper.ResolveValueOrThrowIfTypeMismatch(
                    this.raiseErrorIfTypeMismatch,
                    this.valueIfTypeMismatch,
                    this.defaultValue,
                    providerContext,
                    typeName,
                    "object",
                    lookupData,
                    this.SchemaReferenceKey,
                    exception);

                if (valueIfTypeMismatch == null)
                {
                    return ProviderResult<Data<object>>.Success(null);
                }

                var value = valueIfTypeMismatch.GetValueFromGeneric();
                DataObjectHelper.ThrowIfNotObjectOrArray(value, this.GetType().Name, errorData);
                return ProviderResult<Data<object>>.Success(new Data<object>(value));
            }
        }
    }
}

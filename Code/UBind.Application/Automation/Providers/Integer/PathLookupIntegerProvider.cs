// <copyright file="PathLookupIntegerProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Integer
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.PathLookup;

    /// <summary>
    /// Provides an integer value from the automation data (or a custom data object) using a path defined by a text provider.
    /// </summary>
    public class PathLookupIntegerProvider : IProvider<Data<long>>
    {
        private readonly IObjectPathLookupProvider lookup;
        private readonly IProvider<IData> valueIfNotFound;
        private readonly IProvider<Data<bool>> raiseErrorIfNotFound;
        private readonly IProvider<Data<bool>> raiseErrorIfNull;
        private readonly IProvider<IData> valueIfNull;
        private readonly IProvider<Data<bool>> raiseErrorIfTypeMismatch;
        private readonly IProvider<IData> valueIfTypeMismatch;
        private readonly IProvider<IData> defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathLookupIntegerProvider"/> class.
        /// </summary>
        /// <param name="lookupProvider">The lookup provider to be used.</param>
        /// <param name="defaultValueProvider">The default value provider, if any.</param>
        public PathLookupIntegerProvider(
            IObjectPathLookupProvider lookupProvider,
            IProvider<IData> valueIfNotFound,
            IProvider<Data<bool>> raiseErrorIfNotFound,
            IProvider<Data<bool>> raiseErrorIfNull,
            IProvider<IData> valueIfNull,
            IProvider<Data<bool>> raiseErrorIfTypeMismatch,
            IProvider<IData> valueIfTypeMismatch,
            IProvider<IData> defaultValue)
        {
            this.lookup = lookupProvider;
            this.valueIfNotFound = valueIfNotFound;
            this.raiseErrorIfNotFound = raiseErrorIfNotFound;
            this.raiseErrorIfNull = raiseErrorIfNull;
            this.valueIfNull = valueIfNull;
            this.raiseErrorIfTypeMismatch = raiseErrorIfTypeMismatch;
            this.valueIfTypeMismatch = valueIfTypeMismatch;
            this.defaultValue = defaultValue;
        }

        public string SchemaReferenceKey => "objectPathLookupInteger";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<long>>> Resolve(IProviderContext providerContext)
        {
            IData lookupData = null;

            var lookupResult = await this.lookup.Resolve(providerContext);

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
                    return ProviderResult<Data<long>>.Success(null);
                }
            }

            var isValid = long.TryParse(lookupData.ToString(), out long value);
            if (isValid)
            {
                return ProviderResult<Data<long>>.Success(value);
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
                "integer",
                lookupData,
                this.SchemaReferenceKey);

                if (valueIfTypeMismatch == null)
                {
                    return ProviderResult<Data<long>>.Success(null);
                }

                return ProviderResult<Data<long>>.Success(long.Parse(valueIfTypeMismatch.ToString()));
            }
        }
    }
}

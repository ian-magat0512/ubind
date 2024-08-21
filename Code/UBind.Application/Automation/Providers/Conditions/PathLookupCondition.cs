// <copyright file="PathLookupCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.PathLookup;

    /// <summary>
    /// Reads a boolean (true or false) value from the automation data (or a custom data object) using a path defined by a text provider.
    /// </summary>
    public class PathLookupCondition : IProvider<Data<bool>>
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
        /// Initializes a new instance of the <see cref="PathLookupCondition"/> class.
        /// </summary>
        /// <param name="objectPathLookup">The object path lookup.</param>
        /// <param name="valueIfNotFound">The default value provider, if any.</param>
        public PathLookupCondition(
            IObjectPathLookupProvider objectPathLookup,
            IProvider<IData> valueIfNotFound,
            IProvider<Data<bool>> raiseErrorIfNotFound,
            IProvider<Data<bool>> raiseErrorIfNull,
            IProvider<IData> valueIfNull,
            IProvider<Data<bool>> raiseErrorIfTypeMismatch,
            IProvider<IData> valueIfTypeMismatch,
            IProvider<IData> defaultValue)
        {
            this.objectPathLookup = objectPathLookup;
            this.valueIfNotFound = valueIfNotFound;
            this.raiseErrorIfNotFound = raiseErrorIfNotFound;
            this.raiseErrorIfNull = raiseErrorIfNull;
            this.valueIfNull = valueIfNull;
            this.raiseErrorIfTypeMismatch = raiseErrorIfTypeMismatch;
            this.valueIfTypeMismatch = valueIfTypeMismatch;
            this.defaultValue = defaultValue;
        }

        public string SchemaReferenceKey => "objectPathLookupCondition";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<bool>>> Resolve(IProviderContext providerContext)
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
                    return ProviderResult<Data<bool>>.Success(null);
                }
            }

            bool result = bool.TryParse(lookupData.ToString(), out bool valueInBoolean);
            if (result)
            {
                return ProviderResult<Data<bool>>.Success(valueInBoolean);
            }

            string typeName = TypeHelper.GetReadableTypeName(lookupData.GetValueFromGeneric());
            IData valueIfTypeMismatch = await PathLookupResolverHelper.ResolveValueOrThrowIfTypeMismatch(
                this.raiseErrorIfTypeMismatch,
                this.valueIfTypeMismatch,
                this.defaultValue,
                providerContext,
                typeName,
                "condition",
                lookupData,
                this.SchemaReferenceKey);

            if (valueIfTypeMismatch == null)
            {
                return ProviderResult<Data<bool>>.Success(null);
            }

            bool.TryParse(valueIfTypeMismatch.ToString(), out bool resultValue);
            return ProviderResult<Data<bool>>.Success(resultValue);
        }
    }
}

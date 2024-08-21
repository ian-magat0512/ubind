// <copyright file="PathLookupDateTimeProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.DateTime
{
    using MorseCode.ITask;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.PathLookup;

    /// <summary>
    /// Resolves the path given against the chosen data object or automation data and returns the value if it is a valid
    /// path lookup datetime.
    /// </summary>
    /// <remarks>Schema key: objectPathLookupDateTime.</remarks>
    public class PathLookupDateTimeProvider : IProvider<Data<Instant>>
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
        /// Initializes a new instance of the <see cref="PathLookupDateTimeProvider"/> class.
        /// </summary>
        /// <param name="pathLookup">The lookup provider to be used.</param>
        public PathLookupDateTimeProvider(
            IObjectPathLookupProvider pathLookup,
            IProvider<IData> valueIfNotFound,
            IProvider<Data<bool>> raiseErrorIfNotFound,
            IProvider<Data<bool>> raiseErrorIfNull,
            IProvider<IData> valueIfNull,
            IProvider<Data<bool>> raiseErrorIfTypeMismatch,
            IProvider<IData> valueIfTypeMismatch,
            IProvider<IData> defaultValue)
        {
            this.lookup = pathLookup;
            this.valueIfNotFound = valueIfNotFound;
            this.raiseErrorIfNotFound = raiseErrorIfNotFound;
            this.raiseErrorIfNull = raiseErrorIfNull;
            this.valueIfNull = valueIfNull;
            this.raiseErrorIfTypeMismatch = raiseErrorIfTypeMismatch;
            this.valueIfTypeMismatch = valueIfTypeMismatch;
            this.defaultValue = defaultValue;
        }

        public string SchemaReferenceKey => "objectPathLookupDateTime";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<Instant>>> Resolve(IProviderContext providerContext)
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
                    return ProviderResult<Data<Instant>>.Success(null);
                }
            }

            var dateTimeResult = OffsetDateTimePattern.ExtendedIso.Parse(lookupData.ToString());

            if (dateTimeResult.Success)
            {
                var result = InstantPattern.ExtendedIso.Parse(dateTimeResult.Value.ToInstant().ToString());
                return ProviderResult<Data<Instant>>.Success(new Data<Instant>(result.Value));
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
                    "dateTime",
                    lookupData,
                    this.SchemaReferenceKey,
                    dateTimeResult.Exception);

                if (valueIfTypeMismatch == null)
                {
                    return ProviderResult<Data<Instant>>.Success(null);
                }

                var valueIfTypeMismatchResult = OffsetDateTimePattern.ExtendedIso.Parse(valueIfTypeMismatch.ToString());

                if (valueIfTypeMismatchResult.Success)
                {
                    var result = InstantPattern.ExtendedIso.Parse(valueIfTypeMismatchResult.Value.ToInstant().ToString());
                    return ProviderResult<Data<Instant>>.Success(new Data<Instant>(result.Value));
                }

                throw valueIfTypeMismatchResult.Exception;
            }
        }
    }
}

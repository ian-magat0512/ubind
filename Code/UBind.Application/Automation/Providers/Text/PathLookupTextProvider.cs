// <copyright file="PathLookupTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System.Collections.Generic;
    using MorseCode.ITask;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.PathLookup;

    /// <summary>
    /// Provides a text value from the automation data (or a custom data object) using a path defined by a text.
    /// </summary>
    public class PathLookupTextProvider : IProvider<Data<string>>
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
        /// Initializes a new instance of the <see cref="PathLookupTextProvider"/> class.
        /// </summary>
        /// <param name="pathLookup">The path lookup provider.</param>
        /// <param name="valueIfNotFound">The default value provider, if any.</param>
        public PathLookupTextProvider(
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

        public string SchemaReferenceKey => "objectPathLookupText";

        /// <summary>
        /// Provides a string value obtained from an object via a given path.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>A string value.</returns>
        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
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
                    return ProviderResult<Data<string>>.Success(null);
                }
            }

            var result = lookupData.GetValueFromGeneric();

            if (!(result is string))
            {
                if (result is JValue)
                {
                    // JValue is also a JToken, and so if it wasn't for this if statement at the start, the value
                    // would be Serialized and we don't want that, because it will put extra quotes around the value.
                    result = result.ToString();
                }
                else if (result is JToken)
                {
                    result = JsonConvert.SerializeObject(result);
                }
                else if (result is IDictionary<string, object>)
                {
                    return JsonConvert.SerializeObject(JObject.FromObject(result));
                }
                else if (result is byte[] dataBytes)
                {
                    result = System.Text.Encoding.UTF8.GetString(dataBytes);
                }
                else if (DataObjectHelper.IsArray(result) || DataObjectHelper.IsObject(result))
                {
                    // since the result value here was not a string, we need to throw an error if it's a list or object.
                    // if you can see the code above some of the expected types are already handled.
                    string typeName = TypeHelper.GetReadableTypeName(result);
                    IData valueIfTypeMismatch = await PathLookupResolverHelper.ResolveValueOrThrowIfTypeMismatch(
                        this.raiseErrorIfTypeMismatch,
                        this.valueIfTypeMismatch,
                        this.defaultValue,
                        providerContext,
                        typeName,
                        "string",
                        lookupData,
                        this.SchemaReferenceKey);
                    var valueIfTypeMismatchResult = valueIfTypeMismatch?.GetValueFromGeneric();
                    return ProviderResult<Data<string>>.Success(new Data<string>(valueIfTypeMismatchResult as string));
                }
                else
                {
                    result = result.ToString();
                }
            }

            return ProviderResult<Data<string>>.Success(new Data<string>(result as string));
        }
    }
}

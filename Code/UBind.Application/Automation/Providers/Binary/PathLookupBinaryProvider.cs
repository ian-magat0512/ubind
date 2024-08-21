// <copyright file="PathLookupBinaryProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Binary
{
    using System;
    using MorseCode.ITask;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.PathLookup;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Provides a binary value from the automation data, as a byte array.
    /// </summary>
    public class PathLookupBinaryProvider : IProvider<Data<byte[]>>
    {
        private readonly IObjectPathLookupProvider lookup;
        private readonly IProvider<Data<byte[]>> valueIfNotFoundProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathLookupBinaryProvider"/> class.
        /// </summary>
        /// <param name="pathLookup">The path lookup provider.</param>
        /// <param name="valueIfNotFoundProvider">The default value provider, if any.</param>
        public PathLookupBinaryProvider(
            IObjectPathLookupProvider pathLookup,
            IProvider<Data<byte[]>> valueIfNotFoundProvider)
        {
            this.lookup = pathLookup;
            this.valueIfNotFoundProvider = valueIfNotFoundProvider;
        }

        public string SchemaReferenceKey => "objectPathLookupBinary";

        /// <summary>
        /// Provides a binary value obtained from an object via a given path.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>A binary value.</returns>
        public async ITask<IProviderResult<Data<byte[]>>> Resolve(IProviderContext providerContext)
        {
            object lookupData = null;
            try
            {
                var lookupResult = await this.lookup.Resolve(providerContext);
                if (lookupResult.IsFailure)
                {
                    if (this.valueIfNotFoundProvider != null)
                    {
                        return await this.valueIfNotFoundProvider.Resolve(providerContext);
                    }
                }

                lookupData = lookupResult.GetValueOrThrowIfFailed().GetValueFromGeneric();
            }
            catch (Exception ex) when (ex is NullReferenceException || ex is ErrorException)
            {
                if (ex is NullReferenceException)
                {
                    throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                        "path", this.SchemaReferenceKey));
                }

                throw;
            }

            if (lookupData == null)
            {
                return ProviderResult<Data<byte[]>>.Success(null);
            }

            if (lookupData is byte[] binaryData)
            {
                return ProviderResult<Data<byte[]>>.Success(new Data<byte[]>(binaryData));
            }

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            var errorMessage = $"When trying to get a binary value, a \"{lookupData.GetType()}\" value was received.";
            errorData.Add(ErrorDataKey.ErrorMessage, errorMessage);
            throw new ErrorException(Errors.Automation.InvalidValueTypeObtained(
                "Binary (byte[])",
                this.SchemaReferenceKey,
                errorData));
        }
    }
}

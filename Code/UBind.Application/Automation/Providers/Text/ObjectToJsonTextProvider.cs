// <copyright file="ObjectToJsonTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using MorseCode.ITask;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Generates a json text reprentation of a data object.
    /// </summary>
    public class ObjectToJsonTextProvider : IProvider<Data<string>>
    {
        private readonly IObjectProvider dataObjectProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectToJsonTextProvider"/> class.
        /// </summary>
        /// <param name="dataObjectProvider">The data object provider.</param>
        public ObjectToJsonTextProvider(IObjectProvider dataObjectProvider)
        {
            this.dataObjectProvider = dataObjectProvider;
        }

        public string SchemaReferenceKey => "objectToJsonText";

        /// <summary>
        /// Provides a json text reprentation of a data object.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>A text value.</returns>
        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
        {
            var dataObject = (await this.dataObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            if (dataObject == null)
            {
                return ProviderResult<Data<string>>.Success(null);
            }

            try
            {
                var settings = new JsonSerializerSettings
                {
                    // Set ReferenceLoopHandling to Ignore to prevent JSON serialization from
                    // throwing an error when encountering circular references in the object graph.
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };
                return ProviderResult<Data<string>>.Success(JsonConvert.SerializeObject(dataObject, settings));
            }
            catch (Exception ex)
            {
                JObject errorDataDictionary = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorDataDictionary.Add(ErrorDataKey.ErrorMessage, ex.Message);
                throw new ErrorException(Errors.Automation.ValueResolutionError(
                    this.SchemaReferenceKey, errorDataDictionary));
            }
        }
    }
}

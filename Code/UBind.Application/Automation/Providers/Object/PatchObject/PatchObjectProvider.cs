// <copyright file="PatchObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Object.PatchObject
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// This class is responsible for performing the operations on the object and return the new object to the user.
    /// </summary>
    public class PatchObjectProvider : IObjectProvider
    {
        public PatchObjectProvider(IObjectProvider obj, IEnumerable<BaseOperation> operations, IObjectProvider valueIfAborted)
        {
            this.Operations = operations;
            this.Object = obj;
            this.ValueIfAborted = valueIfAborted;
        }

        public IObjectProvider Object { get; }

        public IObjectProvider ValueIfAborted { get; }

        public IEnumerable<BaseOperation> Operations { get; }

        public string SchemaReferenceKey => "patchObject";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<object>>> Resolve(IProviderContext providerContext)
        {
            try
            {
                var dataObject = (await this.Object.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                var jObject = JObject.FromObject(dataObject);
                foreach (var operation in this.Operations)
                {
                    var result = await operation.Execute(jObject, providerContext);
                    jObject = result.Object;
                    if (result.PostPatchDirective == PostPatchDirective.End)
                    {
                        break;
                    }
                }

                return ProviderResult<Data<object>>.Success(new Data<object>(jObject));
            }
            catch (ErrorException appException) when (appException.Error.Code.StartsWith("automation.providers.patchObject"))
            {
                if (this.ValueIfAborted != null)
                {
                    var value = (await this.ValueIfAborted.Resolve(providerContext)).GetValueOrThrowIfFailed();
                    return ProviderResult<Data<object>>.Success(new Data<ReadOnlyDictionary<string, object>>(value.GetValueFromGeneric()));
                }

                var data = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                var additionalDetails = appException.Error.AdditionalDetails ?? new List<string>();
                additionalDetails.Add($"Error Message: {appException.Error.Title}");
                throw new ErrorException(Errors.Automation.Provider.PatchObject.PatchOperationFailed(appException.Error.HttpStatusCode, data, additionalDetails));
            }
        }
    }
}

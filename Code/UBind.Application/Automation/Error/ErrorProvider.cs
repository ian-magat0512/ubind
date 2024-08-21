// <copyright file="ErrorProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Error
{
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents an error which has occurred within the application.
    /// </summary>
    public class ErrorProvider : IProvider<ConfiguredError>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorProvider"/> class.
        /// </summary>
        /// <param name="codeProvider">The error code provider.</param>
        /// <param name="titleProvider">The message title provider.</param>
        /// <param name="messageProvider">The message provider.</param>
        /// <param name="httpStatusCodeProvider">The HTTP status code provider.</param>
        /// <param name="additionalDetailsProvider">The additional details, if any.</param>
        /// <param name="dataObjectProvider">The data object provider.</param>
        public ErrorProvider(
            IProvider<Data<string>> codeProvider,
            IProvider<Data<string>> titleProvider,
            IProvider<Data<string>> messageProvider,
            IProvider<Data<long>> httpStatusCodeProvider,
            IEnumerable<IProvider<Data<string>>> additionalDetailsProvider,
            IObjectProvider dataObjectProvider)
        {
            this.Code = codeProvider;
            this.Title = titleProvider;
            this.Message = messageProvider;
            this.HttpStatusCode = httpStatusCodeProvider;
            this.AdditionalDetails = additionalDetailsProvider;
            this.Data = dataObjectProvider;
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        public IProvider<Data<string>> Code { get; }

        /// <summary>
        /// Gets the title for the message.
        /// </summary>
        public IProvider<Data<string>> Title { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public IProvider<Data<string>> Message { get; }

        /// <summary>
        /// Gets the http status code.
        /// </summary>
        public IProvider<Data<long>> HttpStatusCode { get; }

        /// <summary>
        /// Gets a collection of additional details, if any.
        /// </summary>
        public IEnumerable<IProvider<Data<string>>> AdditionalDetails { get; } = Enumerable.Empty<IProvider<Data<string>>>();

        /// <summary>
        /// Gets any additional data which may be included to assist in error handling.
        /// </summary>
        public IObjectProvider Data { get; }

        public string SchemaReferenceKey => throw new System.NotImplementedException();

        /// <summary>
        /// Invokes the details for this error, returning an error data.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>An error data.</returns>
        public async ITask<IProviderResult<ConfiguredError>> Resolve(IProviderContext providerContext)
        {
            var resolveAdditionalDetails = this.AdditionalDetails != null
                ? await this.AdditionalDetails?.SelectAsync(async add => (await add.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue)
                : null;
            var additionalDetails = resolveAdditionalDetails?.ToList();
            JObject additionalData = null;
            if (this.Data != null)
            {
                var errorData = (await this.Data.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                additionalData = JObject.FromObject(errorData);
            }

            var httpStatusCode = (await this.HttpStatusCode.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var hasValidStatusCode = int.TryParse(httpStatusCode.ToString(), out int statusCode);
            var code = (await this.Code.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var title = (await this.Title.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var message = (await this.Message.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            return ProviderResult<ConfiguredError>.Success(new ConfiguredError(
                code.ToString(),
                title.ToString(),
                message.ToString(),
                hasValidStatusCode ? statusCode : default,
                additionalDetails,
                additionalData));
        }
    }
}

// <copyright file="Base64TextBinaryProvider.cs" company="uBind">
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
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    public class Base64TextBinaryProvider : IProvider<Data<byte[]>>
    {
        private readonly IProvider<Data<string>> textProvider;

        public Base64TextBinaryProvider(IProvider<Data<string>> textProvider)
        {
            this.textProvider = textProvider;
        }

        public string SchemaReferenceKey => "base64TextBinary";

        public async ITask<IProviderResult<Data<byte[]>>> Resolve(IProviderContext providerContext)
        {
            var resolveBase64String = await this.textProvider.Resolve(providerContext);
            var base64String = resolveBase64String.GetValueOrThrowIfFailed().DataValue;

            try
            {
                var content = Convert.FromBase64String(base64String);
                return ProviderResult<Data<byte[]>>.Success(new Data<byte[]>(content));
            }
            catch (FormatException ex)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.ErrorMessage, ex.Message);
                throw new ErrorException(Errors.Automation.Provider.InvalidInputData(
                    "base 64",
                    this.SchemaReferenceKey,
                    errorData));
            }
        }
    }
}

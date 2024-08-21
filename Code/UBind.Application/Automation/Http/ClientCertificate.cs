// <copyright file="ClientCertificate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using System.Threading.Tasks;
    using Humanizer;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Represents the unresolved <see cref="ClientCertificate"/> request to be fired/persisted for the automation data.
    /// </summary>
    public class ClientCertificate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        public ClientCertificate(
            string certificateFormat,
            IProvider<Data<byte[]>> certificateData,
            IProvider<Data<string>> password)
        {
            this.Format = certificateFormat;
            this.Data = certificateData;
            this.Password = password;
        }

        public string Format { get; }

        public IProvider<Data<byte[]>> Data { get; }

        public IProvider<Data<string>> Password { get; private set; }

        public async Task<ClientCertificateData> Resolve(IProviderContext providerContext)
        {
            var hasMatch = Enum.TryParse(this.Format?.Humanize(), true, out CertificateFormat format);
            if (!hasMatch)
            {
                var errorData = await providerContext.GetDebugContext();
                throw new ErrorException(Errors.Automation.HttpRequest.CustomCertificateUnsupportedFormat(this.Format?.Humanize(), errorData));
            }

            return new ClientCertificateData(
                this.Format,
                (await this.Data.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.Password.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue);
        }
    }
}

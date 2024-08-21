// <copyright file="ClientCertificateConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents Client Certificate where to get access to the API.
    /// MotorWeb requested custom 3rd party authentication.
    /// </summary>
    public class ClientCertificateConfigModel : IBuilder<ClientCertificate>
    {
        public string Format { get; set; }

        public IBuilder<IProvider<Data<byte[]>>> CertificateData { get; set; }

        public IBuilder<IProvider<Data<string>>> Password { get; set; }

        /// <inheritdoc/>
        public ClientCertificate Build(IServiceProvider dependencyProvider)
        {
            return new ClientCertificate(
                this.Format,
                this.CertificateData.Build(dependencyProvider),
                this.Password.Build(dependencyProvider));
        }
    }
}

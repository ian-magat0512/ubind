// <copyright file="ClientCertificateData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    /// <summary>
    /// Represents the resolved <see cref="ClientCertificate"/>.
    /// </summary>
    public class ClientCertificateData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientCertificateData"/> class.
        /// </summary>
        public ClientCertificateData(
            string certificateFormat,
            byte[] certificateData,
            string password)
        {
            this.CertificateFormat = certificateFormat;
            this.CertificateData = certificateData;
            this.Password = password;
        }

        /// <summary>
        /// Gets the certificate format.
        /// </summary>
        public string CertificateFormat { get; }

        /// <summary>
        /// Gets the certificate data.
        /// </summary>
        public byte[] CertificateData { get; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password { get; }
    }
}

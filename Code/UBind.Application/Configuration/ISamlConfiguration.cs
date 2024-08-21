// <copyright file="ISamlConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Configuration
{
    using System.Security.Cryptography.X509Certificates;

    public interface ISamlConfiguration : IDisposable
    {
        /// <summary>
        /// Gets or sets the subject name of the service provider certificate.
        /// We recommend using the following: "uBind SAML Service Provider".
        /// </summary>
        string? ServiceProviderCertificateSubjectName { get; set; }

        /// <summary>
        /// Gets or sets the service provider certificate.
        /// This certificate is loaded from the Windows Certificate Store (X509Store) during application startup.
        /// The service provider certificate is used to sign the SAML requests sent to the identity provider.
        /// It must include both a private and public key. The private key is used to sign the requests.
        /// The public key is sent to the identity provider so that they can verify the requests.
        /// </summary>
        X509Certificate2? ServiceProviderCertificate { get; set; }

        string? ServiceProviderCertificateBase64 { get; set; }
    }
}

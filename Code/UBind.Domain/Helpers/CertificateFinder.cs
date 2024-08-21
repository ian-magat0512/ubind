// <copyright file="CertificateFinder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System.Security.Cryptography.X509Certificates;

    public static class CertificateFinder
    {
        public static X509Certificate2? FindFirstWithSubjectName(
            this X509Certificate2Collection certificateCollection,
            string subjectName)
        {
            X509Certificate2Collection certificates
                = certificateCollection.Find(X509FindType.FindBySubjectName, subjectName, false);
            if (certificates.Count == 0)
            {
                // sometimes the certificate is not found even though it's there, so let's try to find it manually
                // It could be the encoding of the text makes it not match when using the Find method.
                foreach (X509Certificate2 cert in certificateCollection)
                {
                    string certSubjectName = cert.Subject.ToString();

                    if (certSubjectName == subjectName)
                    {
                        return cert;
                    }
                }
            }
            else
            {
                return certificates[0];
            }

            return null;
        }
    }
}

// <copyright file="AuthenticationMethodMapper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Mappers
{
    using System.Security.Cryptography.X509Certificates;
    using NodaTime;
    using Riok.Mapperly.Abstractions;
    using UBind.Application.Model.AuthenticationMethod;
    using UBind.Domain.Aggregates.Organisation.AuthenticationMethod;

    [Mapper]
    public partial class AuthenticationMethodMapper
    {
        public AuthenticationMethodBase MapUpsertModelToDomainModel(
            AuthenticationMethodUpsertModel upsertModel,
            Guid authenticationMethodId,
            Instant? createdTimestamp = null)
        {
            AuthenticationMethodBase result = upsertModel switch
            {
                SamlAuthenticationMethodUpsertModel samlModel => this.SamlUpsertModelToDomainModel(samlModel),
                LocalAccountAuthenticationMethodUpsertModel localAccountModel => this.LocalAccountUpsertModelToDomainModel(localAccountModel),
                _ => throw new System.NotImplementedException(),
            };

            // This is necesary because Mapperly doesn't support setting additional constructor params during
            // the mapping process
            result.SetId(authenticationMethodId);

            if (createdTimestamp.HasValue)
            {
                result.SetCreatedTimestamp(createdTimestamp.Value);
            }

            return result;
        }

        public partial Saml SamlUpsertModelToDomainModel(SamlAuthenticationMethodUpsertModel model);

        public partial LocalAccount LocalAccountUpsertModelToDomainModel(
            LocalAccountAuthenticationMethodUpsertModel model);

        private X509Certificate2 StringToIdentityProviderCertificate(string certificateString)
        {
            certificateString = certificateString.Replace("-----BEGIN CERTIFICATE-----", "");
            certificateString = certificateString.Replace("-----END CERTIFICATE-----", "");
            certificateString = certificateString.Replace("\n", "");
            byte[] certificateBytes = Convert.FromBase64String(certificateString);
            return new X509Certificate2(certificateBytes);
        }
    }
}

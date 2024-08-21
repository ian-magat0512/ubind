// <copyright file="AuthenticationMethodMapper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Mappers
{
    using Riok.Mapperly.Abstractions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Web.ResourceModels.Organisation;

    [Mapper]
    public partial class AuthenticationMethodMapper
    {
        /// <summary>
        /// Generates a AuthenticationMethodSummaryModel from a IAuthenticationMethodReadModelSummary.
        /// </summary>
        /// <param name="readModel">The read model.</param>
        public AuthenticationMethodSummaryModel MapReadModelToResourceModel(
            IAuthenticationMethodReadModelSummary readModel)
        {
            AuthenticationMethodSummaryModel result = readModel switch
            {
                SamlAuthenticationMethodReadModel samlModel => this.SamlReadModelToResourceModel(samlModel),
                LocalAccountAuthenticationMethodReadModel localAccountModel => this.LocalAccountReadModelToResourceModel(localAccountModel),
                _ => throw new System.NotImplementedException(),
            };

            result.CreatedDateTime = readModel.CreatedTimestamp.ToExtendedIso8601String();
            result.LastModifiedDateTime = readModel.LastModifiedTimestamp.ToExtendedIso8601String();

            return result;
        }

        private partial SamlAuthenticationMethodModel SamlReadModelToResourceModel(
            SamlAuthenticationMethodReadModel readModel);

        private partial LocalAccountAuthenicationMethodModel LocalAccountReadModelToResourceModel(
            LocalAccountAuthenticationMethodReadModel readModel);
    }
}

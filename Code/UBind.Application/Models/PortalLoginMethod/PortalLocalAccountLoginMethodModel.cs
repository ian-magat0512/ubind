// <copyright file="PortalLocalAccountLoginMethodModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Models.PortalLoginMethod
{
    using UBind.Domain;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;

    /// <summary>
    /// A model for the portal to display local account login.
    /// </summary>
    public class PortalLocalAccountLoginMethodModel : PortalLoginMethodModel
    {
        public PortalLocalAccountLoginMethodModel(
            PortalSignInMethodReadModel portalSignInMethodReadModel,
            LocalAccountAuthenticationMethodReadModel authenticationMethodReadModel,
            PortalReadModel portalReadModel)
            : base(portalSignInMethodReadModel, authenticationMethodReadModel)
        {
            this.AllowSelfRegistration = portalReadModel.UserType == PortalUserType.Customer
                ? authenticationMethodReadModel.AllowCustomerSelfRegistration
                : authenticationMethodReadModel.AllowAgentSelfRegistration;
        }

        public bool AllowSelfRegistration { get; set; }
    }
}

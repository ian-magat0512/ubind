// <copyright file="LoginSamlUserCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using ComponentSpace.Saml2;
    using UBind.Application.Models.User;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.Redis;

    [CreateTransactionThatSavesChangesIfNoneExists]
    public class LoginSamlUserCommand : ICommand<UserLoginResult>
    {
        public LoginSamlUserCommand(
            Tenant tenant,
            SamlAuthenticationMethodReadModel authenticationMethod,
            ISpSsoResult ssoResult,
            SamlSessionData samlSessionData)
        {
            this.Tenant = tenant;
            this.AuthenticationMethod = authenticationMethod;
            this.SsoResult = ssoResult;
            this.SamlSessionData = samlSessionData;
        }

        public Tenant Tenant { get; }

        public SamlAuthenticationMethodReadModel AuthenticationMethod { get; }

        public ISpSsoResult SsoResult { get; }

        public SamlSessionData SamlSessionData { get; }
    }
}

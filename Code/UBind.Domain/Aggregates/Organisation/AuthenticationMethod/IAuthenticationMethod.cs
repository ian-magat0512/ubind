// <copyright file="IAuthenticationMethod.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Organisation.AuthenticationMethod
{
    using System;
    using Flurl;

    public interface IAuthenticationMethod : IMutableEntity<Guid>
    {
        string Name { get; }

        string TypeName { get; }

        public bool CanCustomersSignIn { get; set; }

        public bool CanAgentsSignIn { get; set; }

        bool IncludeSignInButtonOnPortalLoginPage { get; }

        public string? SignInButtonBackgroundColor { get; }

        public Url? SignInButtonIconUrl { get; }

        public string? SignInButtonLabel { get; }

        public bool Disabled { get; set; }
    }
}

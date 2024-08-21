// <copyright file="IAuthenticationMethodReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Organisation
{
    public interface IAuthenticationMethodReadModelSummary : IEntityReadModel<Guid>
    {
        Guid OrganisationId { get; }

        string Name { get; }

        string TypeName { get; }

        bool CanCustomersSignIn { get; set; }

        bool CanAgentsSignIn { get; set; }

        bool IncludeSignInButtonOnPortalLoginPage { get; set; }

        string? SignInButtonBackgroundColor { get; set; }

        string? SignInButtonIconUrl { get; set; }

        string? SignInButtonLabel { get; set; }

        bool Disabled { get; set; }
    }
}

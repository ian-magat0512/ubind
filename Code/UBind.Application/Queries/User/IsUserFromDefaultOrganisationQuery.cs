// <copyright file="IsUserFromDefaultOrganisationQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System.Security.Claims;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Queries whether the user is in the default organisation.
    /// </summary>
    public class IsUserFromDefaultOrganisationQuery : IQuery<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsUserFromDefaultOrganisationQuery"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        public IsUserFromDefaultOrganisationQuery(ClaimsPrincipal user)
        {
            this.User = user;
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        public ClaimsPrincipal User { get; }
    }
}

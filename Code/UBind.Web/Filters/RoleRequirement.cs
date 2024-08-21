// <copyright file="RoleRequirement.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Authorization requirement for roles.
    /// </summary>
    public class RoleRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleRequirement"/> class.
        /// </summary>
        /// <param name="authorizedRoles">List of authorized roles.</param>
        public RoleRequirement(params string[] authorizedRoles)
        {
            this.AuthorizedRoles = authorizedRoles;
        }

        /// <summary>
        /// Gets the authorized roles.
        /// </summary>
        public IEnumerable<string> AuthorizedRoles { get; }
    }
}

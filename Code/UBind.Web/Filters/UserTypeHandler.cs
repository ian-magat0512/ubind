// <copyright file="UserTypeHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Authorization;
    using UBind.Application.ExtensionMethods;

    /// <summary>
    /// The authorization handler for user types.
    /// </summary>
    public class UserTypeHandler : AuthorizationHandler<RoleRequirement>
    {
        /// <inheritdoc/>
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            string userType = context.User.GetUserType().Humanize();
            if (requirement.AuthorizedRoles.Any(r => r.Equals(userType, StringComparison.InvariantCultureIgnoreCase)))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}

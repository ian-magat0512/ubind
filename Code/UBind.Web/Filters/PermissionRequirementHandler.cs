// <copyright file="PermissionRequirementHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using StackExchange.Profiling;
    using UBind.Application.ExtensionMethods;

    /// <summary>
    /// The authorization handler for roles.
    /// </summary>
    public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
    {
        public PermissionRequirementHandler()
        {
        }

        /// <inheritdoc/>
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            using (MiniProfiler.Current.Step("PermissionRequirementHandler.HandleRequirementAsync"))
            {
                var permissions = context.User.GetPermissions();
                if (requirement.RequiredPermissions.All(r => permissions.Contains(r)))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }

                return Task.CompletedTask;
            }
        }
    }
}

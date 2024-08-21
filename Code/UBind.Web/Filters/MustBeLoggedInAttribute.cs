// <copyright file="MustBeLoggedInAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using UBind.Domain;
    using UBind.Web.Extensions;

    /// <summary>
    /// An authorization attribute to replace the standard one, so that we can return a better ProblemDetails json response.
    /// </summary>
    public class MustBeLoggedInAttribute : Microsoft.AspNetCore.Authorization.AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        /// <inheritdoc/>
        public virtual Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // check if logged in
            var user = context.HttpContext.User;
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = Errors.General.NotAuthenticated().ToProblemJsonResult();
            }

            return Task.CompletedTask;
        }
    }
}

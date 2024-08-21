// <copyright file="MustHavePermissionAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using UBind.Application.Queries.Principal;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Web.Extensions;

    /// <summary>
    /// Filter attribute for authorizing based on Permission types.
    /// </summary>
    public class MustHavePermissionAttribute : MustBeLoggedInAttribute, IAsyncAuthorizationFilter
    {
        public MustHavePermissionAttribute(Permission permission)
        {
            this.Permission = permission;
        }

        /// <summary>
        /// Gets or sets attribute to handle enum to string.
        /// </summary>
        public Permission Permission { get; set; }

        public override async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await base.OnAuthorizationAsync(context);
            var user = context.HttpContext.User;
            var mediator = context.HttpContext.RequestServices.GetRequiredService<ICqrsMediator>();

            // check permission
            if (await mediator.Send(new PrincipalHasPermissionQuery(user, this.Permission)))
            {
                return;
            }

            context.Result = Domain.Errors.General.NotAuthorized().ToProblemJsonResult();
        }
    }
}

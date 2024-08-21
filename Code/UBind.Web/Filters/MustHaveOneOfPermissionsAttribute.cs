// <copyright file="MustHaveOneOfPermissionsAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using UBind.Application.Queries.Principal;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Web.Extensions;

    /// <summary>
    /// Throws an exception if the user does not have one of the specified permissions.
    /// </summary>
    public class MustHaveOneOfPermissionsAttribute : MustBeLoggedInAttribute, IAsyncAuthorizationFilter
    {
        public MustHaveOneOfPermissionsAttribute(params Permission[] permissions)
        {
            this.Permissions = permissions;
        }

        public Permission[] Permissions { get; set; }

        public override async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await base.OnAuthorizationAsync(context);
            var user = context.HttpContext.User;
            var mediator = context.HttpContext.RequestServices.GetRequiredService<ICqrsMediator>();

            // check permissions
            foreach (var permission in this.Permissions)
            {
                if (await mediator.Send(new PrincipalHasPermissionQuery(user, permission)))
                {
                    return;
                }
            }

            context.Result = Errors.General.NotAuthorized().ToProblemJsonResult();
        }
    }
}

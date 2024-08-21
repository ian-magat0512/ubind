// <copyright file="MustHaveUserTypeAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain;
    using UBind.Web.Extensions;

    /// <summary>
    /// Throws an exception if the user does not have one of these user types.
    /// </summary>
    public class MustHaveUserTypeAttribute : MustBeLoggedInAttribute, IAsyncAuthorizationFilter
    {
        private List<UserType> allowedUserTypes = new List<UserType>();

        public MustHaveUserTypeAttribute(params UserType[] userType)
        {
            this.allowedUserTypes = userType.ToList();
        }

        public override async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await base.OnAuthorizationAsync(context);
            var user = context.HttpContext.User;
            if (this.allowedUserTypes.Contains(user.GetUserType()))
            {
                return;
            }

            context.Result = Errors.General.NotAuthorized().ToProblemJsonResult();
        }
    }
}

// <copyright file="MustBeFromDefaultOrganisationAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain;
    using UBind.Domain.Services;
    using UBind.Web.Extensions;

    /// <summary>
    /// An authorization attribute that only permits of the user's organisation is a default of its tenancy.
    /// </summary>
    public class MustBeFromDefaultOrganisationAttribute
        : Microsoft.AspNetCore.Authorization.AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        /// <inheritdoc />
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            var userTenantId = user.GetTenantId();

            // you can also use registered services
            var organisationService = context.HttpContext.RequestServices
                .GetService(typeof(IOrganisationService)) as IOrganisationService;
            var isDefault = await organisationService.IsOrganisationDefaultForTenant(userTenantId, user.GetOrganisationId());
            if (!isDefault)
            {
                context.Result = Errors.User.Organisation
                    .UnauthorizedForNonDefault(user.GetId()).ToProblemJsonResult();
            }
        }
    }
}

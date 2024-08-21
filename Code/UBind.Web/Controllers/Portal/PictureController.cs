// <copyright file="PictureController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for handling picture requests.
    /// </summary>
    [Route("api/v1/picture")]
    public class PictureController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IUserProfilePictureRepository repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureController"/> class.
        /// </summary>
        /// <param name="repository">The user profile picture repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public PictureController(
            IUserProfilePictureRepository repository,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.repository = repository;
        }

        /// <summary>
        /// Gets the picture based on the picture id.
        /// </summary>
        /// <param name="pictureId">The picture id.</param>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <returns>The raw picture file data.</returns>
        /* TODO: We need to pass the bearer token to this request and then uncomment the below line */
        /*[Authorize]*/
        [HttpGet]
        [Route("{pictureId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [ResponseCache(CacheProfileName = CacheProfileNames.MaxStoreDuration)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        /*[MustHaveOneOfPermissions(Permission.ViewMyAccount, Permission.ManageUsers)]*/
        public IActionResult GetPicture(Guid pictureId, [FromQuery] string tenant)
        {
            /* TODO: We need to pass the bearer token to this request and then uncomment the below lines */
            /*
            var userTenantId = this.User.GetTenantNewId();
            var tenantModel = this.tenantAndProductResolver.GetTenant(new GuidOrAlias(tenant), false);
            var tenantId = tenantModel.NewId/* ?? userTenantId;
            if (guidTenantId != userTenantId && userTenantId != Tenant.MasterTenantNewId)
            {
                return Errors.General.Forbidden("get a user's profile picture from another tenancy").ToProblemJsonResult();
            }
            */

            var profilePicture = this.repository.GetById(pictureId);
            if (profilePicture == null)
            {
                return Errors.General.NotFound("profile picture", pictureId).ToProblemJsonResult();
            }

            return this.File(profilePicture.PictureData, "image/png");
        }
    }
}

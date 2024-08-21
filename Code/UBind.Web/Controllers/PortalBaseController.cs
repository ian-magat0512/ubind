// <copyright file="PortalBaseController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Controller base for portal.
    /// </summary>
    [Produces("application/json")]
    public abstract class PortalBaseController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalBaseController"/> class.
        /// </summary>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public PortalBaseController(
            ICachingResolver cachingResolver)
        {
            this.CachingResolver = cachingResolver;
        }

        /// <summary>
        /// Gets the caching resolver.
        /// </summary>
        protected ICachingResolver CachingResolver { get; private set; }

        /// <summary>
        /// Retrieves the tenant id and comparing it to the users tenant or throw an error.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="errorActionMessage">Custom action message when an error is thrown.</param>
        /// <param name="errorReasonMessage">Custom error message when an error is thrown.</param>
        /// <returns>The resulting tenant Id.</returns>
        protected Guid GetContextTenantIdOrThrow(
            Guid? tenantId,
            string errorActionMessage = "access record from a different tenancy.",
            string errorReasonMessage = "")
        {
            var userTenantId = this.User.GetTenantId();
            tenantId = tenantId ?? userTenantId;
            if (tenantId != userTenantId && userTenantId != Tenant.MasterTenantId)
            {
                throw new ErrorException(Errors.General.Forbidden(errorActionMessage, errorReasonMessage));
            }

            return tenantId.Value;
        }

        /// <summary>
        /// Retrieves the tenant id and comparing it to the users tenant or throw an error.
        /// </summary>
        /// <param name="tenant">The tenant Id or ALias.</param>
        /// <param name="errorActionMessage">Custom action message when an error is thrown.</param>
        /// <param name="errorReasonMessage">Custom error message when an error is thrown.</param>
        /// <returns>The resulting tenant Id.</returns>
        protected async Task<Guid> GetContextTenantIdOrThrow(
            string? tenant,
            string errorActionMessage = "access record from a different tenancy.",
            string errorReasonMessage = "")
        {
            var userTenantId = this.User.GetTenantId();
            tenant = tenant ?? userTenantId.ToString();
            var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return this.GetContextTenantIdOrThrow(tenantModel.Id, errorActionMessage, errorReasonMessage);
        }

        /// <summary>
        /// Retrieves the tenant and compares it to the users tenant.
        /// If the user is not a master tenant user and the tenant is not the same as the users tenant,
        /// an exception is thrown.
        /// </summary>
        /// <param name="tenant">The tenant Id or ALias.</param>
        /// <param name="errorActionMessage">Custom action message when an error is thrown.</param>
        /// <param name="errorReasonMessage">Custom error message when an error is thrown.</param>
        /// <returns>The resulting tenant.</returns>
        protected async Task<Tenant> GetContextTenantOrThrow(
            string? tenant,
            string errorActionMessage = "access record from a different tenancy.",
            string errorReasonMessage = "")
        {
            var userTenantId = this.User.GetTenantId();
            tenant = tenant ?? userTenantId.ToString();
            var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            if (tenantModel.Id != userTenantId && userTenantId != Tenant.MasterTenantId)
            {
                throw new ErrorException(Errors.General.Forbidden(errorActionMessage, errorReasonMessage));
            }

            return tenantModel;
        }
    }
}

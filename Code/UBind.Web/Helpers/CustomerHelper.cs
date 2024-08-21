// <copyright file="CustomerHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Helpers
{
    using System;
    using System.Threading.Tasks;
    using UBind.Application.Queries.Tenant;
    using UBind.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Helper for resolving customer details in preparation for commands or queries.
    /// </summary>
    public class CustomerHelper : ICustomerHelper
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerHelper"/> class.
        /// </summary>
        /// <param name="cachingResolver">The caching resolver.</param>
        /// <param name="mediator">The mediator.</param>
        public CustomerHelper(
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
        }

        /// <inheritdoc/>
        public async Task<ResolvedCustomerPersonalDetailsModel> CreateResolvedCustomerPersonalDetailsModel(
            Guid tenantId,
            CustomerPersonalDetailsModel customerPersonalDetailsModel,
            Guid? organisationId,
            Guid? portalId)
        {
            var tenant = await this.mediator.Send(new GetTenantByIdQuery(tenantId));
            if (!organisationId.HasValue)
            {
                var organisationModel = await this.cachingResolver.GetOrganisationOrNull(
                        tenantId,
                        new GuidOrAlias(
                            string.IsNullOrEmpty(customerPersonalDetailsModel.Organisation)
                                ? customerPersonalDetailsModel.OrganisationId
                                : customerPersonalDetailsModel.Organisation));

                organisationId = organisationModel?.Id
                    ?? tenant.Details.DefaultOrganisationId;
            }

            var portal = await this.cachingResolver.GetPortalOrNull(
                tenantId,
                new GuidOrAlias(
                    string.IsNullOrEmpty(customerPersonalDetailsModel.Portal)
                        ? customerPersonalDetailsModel.PortalId
                        : customerPersonalDetailsModel.Portal));
            portalId = portal?.Id ?? portalId;

            ResolvedCustomerPersonalDetailsModel resolvedCustomerPersonalDetailsModel
                = new ResolvedCustomerPersonalDetailsModel(organisationId.Value, tenant, customerPersonalDetailsModel, portalId);

            return resolvedCustomerPersonalDetailsModel;
        }
    }
}

// <copyright file="ApplicationJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Queries.Portal;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// general application template JSON object provider.
    /// </summary>
    public class ApplicationJObjectProvider : IJObjectProvider
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;
        private readonly IEmailInvitationConfiguration emailConfiguration;

        public ApplicationJObjectProvider(
            IEmailInvitationConfiguration emailConfiguration,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.emailConfiguration = emailConfiguration;
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
        }

        /// <inheritdoc/>
        public JObject JsonObject { get; private set; } = new JObject();

        /// <inheritdoc/>
        public async Task CreateJsonObject(ApplicationEvent applicationEvent)
        {
            Tenant tenant = null;
            Domain.Product.Product product = null;
            Guid? tenantId = applicationEvent?.Aggregate?.TenantId;
            Guid? productId = applicationEvent?.Aggregate?.ProductId;
            if (tenantId != null)
            {
                tenant = await this.cachingResolver.GetTenantOrThrow(tenantId.Value);
            }

            if (tenantId != null && productId != null)
            {
                product = await this.cachingResolver.GetProductOrThrow(tenantId.Value, productId.Value);
            }

            dynamic jsonObject = new JObject();
            jsonObject.ApplicationUrl = UrlFormatting.GetApplicationUrl(this.emailConfiguration.InvitationLinkHost);
            Guid? portalId = null;
            if (applicationEvent.Aggregate.HasCustomer)
            {
                var customer = await this.mediator.Send(
                    new GetCustomerByIdQuery(tenant.Id, applicationEvent.Aggregate.CustomerId.Value));
                portalId = customer.PortalId;
            }

            portalId = portalId ?? await this.mediator.Send(new GetDefaultPortalIdQuery(
                tenant.Id, applicationEvent.Aggregate.OrganisationId, PortalUserType.Customer));
            jsonObject.PortalUrl = await this.mediator.Send(new GetPortalUrlQuery(
                tenant.Id,
                applicationEvent.Aggregate.OrganisationId,
                portalId,
                applicationEvent.Aggregate.Environment));
            jsonObject.AssetsUrl = UrlFormatting.GetAssetsUrl(
                this.emailConfiguration.InvitationLinkHost,
                tenant?.Details?.Alias ?? string.Empty,
                product?.Details?.Alias ?? string.Empty,
                applicationEvent.Aggregate.Environment,
                WebFormAppType.Quote,
                applicationEvent.ProductReleaseId);
            jsonObject.TimeZoneAlias = "AET";

            IJsonObjectParser parser
                = new GenericJObjectParser(string.Empty, jsonObject);
            if (parser.JsonObject != null)
            {
                this.JsonObject = parser.JsonObject;
            }
        }
    }
}

// <copyright file="GetPortalLocationsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Portal
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Portal;
    using IPortalService = UBind.Domain.Services.IPortalService;

    public class GetPortalLocationsQueryHandler : IQueryHandler<GetPortalLocationsQuery, PortalLocations>
    {
        private readonly ICqrsMediator mediator;
        private readonly IPortalService portalService;

        public GetPortalLocationsQueryHandler(
            ICqrsMediator mediator,
            IPortalService portalService)
        {
            this.mediator = mediator;
            this.portalService = portalService;
        }

        public async Task<PortalLocations> Handle(GetPortalLocationsQuery query, CancellationToken cancellationToken)
        {
            var locations = new PortalLocations();
            locations.Production = new PortalLocations.Location { Url = query.Portal.ProductionUrl };
            locations.Production.IsEmbedded = !string.IsNullOrEmpty(query.Portal.ProductionUrl);
            locations.Staging = new PortalLocations.Location { Url = query.Portal.StagingUrl };
            locations.Staging.IsEmbedded = !string.IsNullOrEmpty(query.Portal.StagingUrl);
            locations.Development = new PortalLocations.Location { Url = query.Portal.DevelopmentUrl };
            locations.Development.IsEmbedded = !string.IsNullOrEmpty(query.Portal.DevelopmentUrl);
            if (string.IsNullOrEmpty(locations.Production.Url)
                || string.IsNullOrEmpty(locations.Staging.Url)
                || string.IsNullOrEmpty(locations.Development.Url))
            {
                string defaultPortalUrl = await this.mediator.Send(new GetDefaultPortalUrlQuery(query.Portal));
                if (string.IsNullOrEmpty(locations.Production.Url))
                {
                    locations.Production.Url = defaultPortalUrl;
                }

                if (string.IsNullOrEmpty(locations.Staging.Url))
                {
                    locations.Staging.Url = this.portalService.AddEnvironmentToUrl(
                        defaultPortalUrl, DeploymentEnvironment.Staging);
                }

                if (string.IsNullOrEmpty(locations.Development.Url))
                {
                    locations.Development.Url = this.portalService.AddEnvironmentToUrl(
                        defaultPortalUrl, DeploymentEnvironment.Development);
                }
            }

            return locations;
        }
    }
}

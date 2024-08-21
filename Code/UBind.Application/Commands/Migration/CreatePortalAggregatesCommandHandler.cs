// <copyright file="CreatePortalAggregatesCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using MediatR;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class CreatePortalAggregatesCommandHandler : ICommandHandler<CreatePortalAggregatesCommand, Unit>
    {
        private readonly IPortalRepository portalRepository;
        private readonly IPortalAggregateRepository portalAggregateRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IUBindDbContext dbContext;

        public CreatePortalAggregatesCommandHandler(
            IPortalRepository portalRepository,
            IPortalAggregateRepository portalAggregateRepository,
            ITenantRepository tenantRepository,
            IUBindDbContext dbContext)
        {
            this.portalRepository = portalRepository;
            this.portalAggregateRepository = portalAggregateRepository;
            this.tenantRepository = tenantRepository;
            this.dbContext = dbContext;
        }

        public async Task<Unit> Handle(CreatePortalAggregatesCommand request, CancellationToken cancellationToken)
        {
            var tenants = this.tenantRepository.GetTenants();
            foreach (var tenant in tenants)
            {
                var portals = this.portalRepository.GetPortals(tenant.Id, new Domain.ReadModel.EntityListFilters());
                foreach (var portal in portals)
                {
                    if (portal.Details.Deleted)
                    {
                        continue;
                    }

                    using (var transaction = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                        TransactionScopeAsyncFlowOption.Enabled))
                    {
                        this.dbContext.TransactionStack.Push(transaction);
                        try
                        {
                            var portalAggregate = new PortalAggregate(
                                tenant.Id,
                                portal.Id,
                                portal.Details.Name,
                                portal.Details.Alias,
                                portal.Details.Title,
                                PortalUserType.Agent,
                                portal.OrganisationId,
                                null,
                                portal.CreatedTimestamp);

                            if (portal.Details.Disabled)
                            {
                                portalAggregate.Disable(null, portal.Details.CreatedTimestamp);
                            }

                            if (portal.Details.StylesheetUrl != null)
                            {
                                portalAggregate.UpdateStyles(portal.Details.StylesheetUrl, null, null, portal.Details.CreatedTimestamp);
                            }

                            if (portal.DeploymentTargetCollection?.Count > 0)
                            {
                                try
                                {
                                    var deploymentTarget = portal.ActiveDeploymentTargets.FirstOrDefault();
                                    var latestUrl = deploymentTarget?.LatestUrl;
                                    if (latestUrl != null
                                        && !latestUrl.ToLower().StartsWith("http://")
                                        && !latestUrl.ToLower().StartsWith("https://"))
                                    {
                                        latestUrl = $"https://{latestUrl}";
                                    }

                                    portalAggregate.SetLocation(
                                        DeploymentEnvironment.Production,
                                        latestUrl,
                                        null,
                                        portal.Details.CreatedTimestamp);
                                }
                                catch (ErrorException ex) when (ex.Error.Code == "invalid.url")
                                {
                                    // Ignore any errors with invalid urls
                                    // we just won't set a custom location for those.
                                    // but we still need to save the portal aggregate
                                }
                            }

                            await this.portalAggregateRepository.ApplyChangesToDbContext(portalAggregate);
                            this.dbContext.SaveChanges();
                            transaction.Complete();
                        }
                        finally
                        {
                            this.dbContext.TransactionStack.Pop();
                        }
                    }

                    // Make sure we don't overwhelm the database
                    await Task.Delay(500);
                }
            }

            return Unit.Value;
        }
    }
}

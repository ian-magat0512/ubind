// <copyright file="DeletePortalCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Portal
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence.Infrastructure;

    public class DeletePortalCommandHandler : ICommandHandler<DeletePortalCommand, Unit>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IPortalAggregateRepository portalAggregateRepository;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ICachingResolver cachingResolver;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IUBindDbContext dbContext;

        public DeletePortalCommandHandler(
            ITenantRepository tenantRepository,
            IPortalAggregateRepository portalAggregateRepository,
            IOrganisationReadModelRepository organisationReadModelRepository,
            IOrganisationAggregateRepository organisationAggregateRepository,
            IClock clock,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICachingResolver cachingResolver,
            IUserReadModelRepository userReadModelRepository,
            IUserAggregateRepository userAggregateRepository,
            IBackgroundJobClient backgroundJobClient,
            IUBindDbContext dbContext)
        {
            this.tenantRepository = tenantRepository;
            this.portalAggregateRepository = portalAggregateRepository;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.cachingResolver = cachingResolver;
            this.userReadModelRepository = userReadModelRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.backgroundJobClient = backgroundJobClient;
            this.dbContext = dbContext;
        }

        public async Task<Unit> Handle(DeletePortalCommand command, CancellationToken cancellationToken)
        {
            var now = this.clock.Now();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var tenant = this.tenantRepository.GetTenantById(command.TenantId);
            var portalAggregate = this.portalAggregateRepository.GetById(command.TenantId, command.PortalId);
            bool isDefaultOrganisation = tenant.Details.DefaultOrganisationId == portalAggregate.OrganisationId;
            var organisation = this.organisationReadModelRepository.Get(tenant.Id, portalAggregate.OrganisationId);
            if (isDefaultOrganisation && tenant.Details.DefaultPortalId == command.PortalId)
            {
                // you can't delete the default portal for a tenant, otherwise nobody would be able
                // to log in to that tenant to administer it.
                throw new ErrorException(Errors.Portal.CannotDeleteDefaultPortalForTenant(
                    portalAggregate.Name,
                    organisation.Name,
                    tenant.Details.Name));
            }

            if (portalAggregate.IsDefault)
            {
                // remove the default portal from the organisation
                var organisationAggregate
                    = this.organisationAggregateRepository.GetById(tenant.Id, portalAggregate.OrganisationId);
                organisationAggregate.SetDefaultPortal(null, performingUserId, now);
                await this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
                this.cachingResolver.RemoveCachedOrganisations(
                    command.TenantId, organisationAggregate.Id, new List<string> { organisationAggregate.Alias });
            }

            this.backgroundJobClient.Enqueue<DeletePortalCommandHandler>(service =>
                service.ResetPortalOfAssociatedUsers(
                    command.TenantId,
                    command.PortalId,
                    performingUserId,
                    tenant.Details.Alias,
                    organisation.Alias,
                    portalAggregate.Alias));

            portalAggregate.Delete(performingUserId, now);
            await this.portalAggregateRepository.ApplyChangesToDbContext(portalAggregate);
            this.cachingResolver.RemoveCachedPortals(
                command.TenantId, command.PortalId, new List<string> { portalAggregate.Alias });
            return Unit.Value;
        }

        [DisplayName("Portal Deletion - Reset Associated Users | TENANT: {3}, ORGANISION: {4}, PORTAL: {5}")]
        public async Task ResetPortalOfAssociatedUsers(Guid tenantId, Guid portalId, Guid? performingUserId, string tenantAlias, string organisationAlias, string portalAlias)
        {
            var now = this.clock.Now();
            var users = this.userReadModelRepository.GetAllUsersByPortal(tenantId, portalId);
            using (var unitOfWork = new UnitOfWork(this.dbContext))
            {
                foreach (var user in users)
                {
                    var userAggregate = this.userAggregateRepository.GetById(tenantId, user.Id);
                    if (userAggregate != null)
                    {
                        userAggregate.ChangePortal(null, performingUserId, now);
                        await this.userAggregateRepository.ApplyChangesToDbContext(userAggregate);
                    }
                }
                await unitOfWork.Commit();
            }
        }
    }
}

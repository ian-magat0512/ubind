// <copyright file="UpdatePortalCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Portal
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Portal;

    public class UpdatePortalCommandHandler : ICommandHandler<UpdatePortalCommand, PortalReadModel>
    {
        private readonly IPortalAggregateRepository portalAggregateRepository;
        private readonly IPortalReadModelRepository portalReadModelRepository;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ICachingResolver cachingResolver;

        public UpdatePortalCommandHandler(
            IPortalAggregateRepository portalAggregateRepository,
            IPortalReadModelRepository portalReadModelRepository,
            IClock clock,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICachingResolver cachingResolver)
        {
            this.portalAggregateRepository = portalAggregateRepository;
            this.portalReadModelRepository = portalReadModelRepository;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.cachingResolver = cachingResolver;
        }

        public async Task<PortalReadModel> Handle(UpdatePortalCommand command, CancellationToken cancellationToken)
        {
            var portalAggregate = this.portalAggregateRepository.GetById(command.TenantId, command.PortalId);
            EntityHelper.ThrowIfNotFound(portalAggregate, command.PortalId);
            if (command.Alias.ToLower() == "null")
            {
                throw new ErrorException(
                    Errors.General.BadRequest($"The alias '{command.Alias}' is invalid"));
            }

            if (this.portalReadModelRepository.PortalNameExistingForTenant(command.TenantId, command.Name, command.PortalId))
            {
                throw new ErrorException(Errors.General.DuplicatePropertyValue("portal", "name", command.Name));
            }

            if (this.portalReadModelRepository.PortalAliasExistingForTenant(command.TenantId, command.Alias, command.PortalId))
            {
                throw new ErrorException(Errors.General.DuplicatePropertyValue("portal", "alias", command.Alias));
            }

            portalAggregate.Update(
                command.Name,
                command.Alias,
                command.Title,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now(),
                command.UserType);
            await this.portalAggregateRepository.ApplyChangesToDbContext(portalAggregate);
            this.cachingResolver.RemoveCachedPortals(
                command.TenantId, command.PortalId, new List<string> { portalAggregate.Alias });
            return portalAggregate.ReadModel;
        }
    }
}

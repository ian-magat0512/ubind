// <copyright file="QueueActivationEmailCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    public class QueueActivationEmailCommandHandler
        : ICommandHandler<QueueActivationEmailCommand, Unit>
    {
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IUserActivationInvitationService userActivationInvitationService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public QueueActivationEmailCommandHandler(
            IPersonAggregateRepository personAggregateRepository,
            IUserAggregateRepository userAggregateRepository,
            IOrganisationReadModelRepository organisationReadModelRepository,
            ITenantRepository tenantRepository,
            IUserActivationInvitationService userActivationInvitationService,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.personAggregateRepository = personAggregateRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.tenantRepository = tenantRepository;
            this.userActivationInvitationService = userActivationInvitationService;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(QueueActivationEmailCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var userAggregate = this.userAggregateRepository.GetById(request.TenantId, request.UserId);
            if (userAggregate == null)
            {
                throw new ErrorException(Errors.User.NotFound(request.UserId));
            }

            var invitationId = userAggregate.CreateActivationInvitation(
                this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.userAggregateRepository.Save(userAggregate);

            var personAggregate = this.personAggregateRepository.GetById(userAggregate.TenantId, userAggregate.PersonId);
            var tenant = this.tenantRepository.GetTenantById(personAggregate.TenantId);
            var organisation = this.organisationReadModelRepository.Get(tenant.Id, personAggregate.OrganisationId);

            // TODO: The user activation service needs to be in the domain layer
            await this.userActivationInvitationService.QueueActivationEmail(
                invitationId,
                userAggregate,
                personAggregate,
                tenant,
                organisation,
                request.Environment,
                userAggregate.PortalId);

            return Unit.Value;
        }
    }
}

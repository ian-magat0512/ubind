// <copyright file="SetDefaultOrganisationOnExistingUsersCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command handler for generating default organisation to existing users based from tenancy.
    /// </summary>
    public class SetDefaultOrganisationOnExistingUsersCommandHandler
        : ICommandHandler<SetDefaultOrganisationOnExistingUsersCommand, Unit>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDefaultOrganisationOnExistingUsersCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The repository for tenants.</param>
        /// <param name="userReadModelRepository">The repository for user read models.</param>
        /// <param name="userAggregateRepository">The repository for user aggregate.</param>
        /// <param name="personAggregateRepository">The repository for person aggregate repository.</param>
        /// <param name="httpContextPropertiesResolver">The resolver for performing user.</param>
        /// <param name="clock">Represents the clock which can return the time as <see cref="Instant"/>.</param>
        public SetDefaultOrganisationOnExistingUsersCommandHandler(
            ITenantRepository tenantRepository,
            IUserReadModelRepository userReadModelRepository,
            IUserAggregateRepository userAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.tenantRepository = tenantRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            SetDefaultOrganisationOnExistingUsersCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenants = this.tenantRepository.GetTenants();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;

            foreach (var tenant in tenants)
            {
                var users = this.userReadModelRepository.GetAllUsersAsQueryable(tenant.Id).ToList();
                foreach (var user in users)
                {
                    var userAggregate = this.userAggregateRepository.GetById(user.TenantId, user.Id);
                    if (userAggregate?.OrganisationId == Guid.Empty)
                    {
                        userAggregate.RecordOrganisationMigration(
                            tenant.Details.DefaultOrganisationId, performingUserId, this.clock.GetCurrentInstant());
                        await this.userAggregateRepository.Save(userAggregate);
                        await Task.Delay(100, cancellationToken);
                    }

                    var personAggregate = this.personAggregateRepository.GetById(user.TenantId, user.PersonId);
                    if (personAggregate?.OrganisationId == Guid.Empty)
                    {
                        personAggregate.RecordOrganisationMigration(
                            tenant.Details.DefaultOrganisationId, performingUserId, this.clock.GetCurrentInstant());
                        await this.personAggregateRepository.Save(personAggregate);
                        await Task.Delay(100, cancellationToken);
                    }
                }
            }

            return Unit.Value;
        }
    }
}

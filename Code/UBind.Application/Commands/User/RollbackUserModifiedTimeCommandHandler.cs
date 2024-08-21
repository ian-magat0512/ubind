// <copyright file="RollbackUserModifiedTimeCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using static UBind.Domain.Aggregates.User.UserAggregate;

    /// <summary>
    /// Command handler for reverting modified time affected by the organisation migration.
    /// </summary>
    public class RollbackUserModifiedTimeCommandHandler : ICommandHandler<RollbackUserModifiedTimeCommand, Unit>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        private readonly List<Type> eventListThatModifiesTime;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="RollbackUserModifiedTimeCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The repository for tenants.</param>
        /// <param name="userReadModelRepository">The repository for user read models.</param>
        /// <param name="userAggregateRepository">The repository for user aggregates.</param>
        /// <param name="httpContextPropertiesResolver">The resolver for performing user.</param>
        /// <param name="clock">Represents the clock which can return the time as <see cref="Instant"/>.</param>
        public RollbackUserModifiedTimeCommandHandler(
            ITenantRepository tenantRepository,
            IUserReadModelRepository userReadModelRepository,
            IUserAggregateRepository userAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.tenantRepository = tenantRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.eventListThatModifiesTime = new List<Type>()
            {
                typeof(UserInitializedEvent),
                typeof(UserImportedEvent),
                typeof(LoginEmailSetEvent),
                typeof(RoleAddedEvent),
                typeof(UserTypeUpdatedEvent),
                typeof(RoleAssignedEvent),
                typeof(RoleRetractedEvent),
                typeof(UserBlockedEvent),
                typeof(UserUnblockedEvent),
                typeof(ActivationInvitationCreatedEvent),
                typeof(UserActivatedEvent),
                typeof(PersonAggregate.FullNameUpdatedEvent),
                typeof(PersonAggregate.NamePrefixUpdatedEvent),
                typeof(PersonAggregate.FirstNameUpdatedEvent),
                typeof(PersonAggregate.MiddleNamesUpdatedEvent),
                typeof(PersonAggregate.LastNameUpdatedEvent),
                typeof(PersonAggregate.NameSuffixUpdatedEvent),
                typeof(PersonAggregate.CompanyUpdatedEvent),
                typeof(PersonAggregate.TitleUpdatedEvent),
                typeof(PersonAggregate.PreferredNameUpdatedEvent),
                typeof(PersonAggregate.EmailAddressUpdatedEvent),
                typeof(PersonAggregate.AlternativeEmailAddressUpdatedEvent),
                typeof(PersonAggregate.MobilePhoneUpdatedEvent),
                typeof(PersonAggregate.HomePhoneUpdatedEvent),
                typeof(PersonAggregate.WorkPhoneUpdatedEvent),
            };
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(RollbackUserModifiedTimeCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Getting all the tenants to be enumerated
            var tenants = this.tenantRepository.GetTenants();
            foreach (var tenant in tenants)
            {
                // Getting all the users per tenant
                var users = this.userReadModelRepository.GetAllUsersAsQueryable(tenant.Id).ToList();
                foreach (var user in users)
                {
                    // Get if there is an organisation migrated event for that user aggregate
                    var aggregate = this.userAggregateRepository.GetById(user.TenantId, user.Id);
                    var kvpEvents = aggregate.GetUnstrippedEvents()
                        .Select((@event, i) => new KeyValuePair<int, IEvent<UserAggregate, Guid>>(i, @event));

                    var organisationMigratedEvent = this.GetOrganisationMigratedEventFromEvents(kvpEvents);
                    if (organisationMigratedEvent.HasValue)
                    {
                        // Get the latest event before the organisation migration sequence that modifies time
                        var migrationSequenceNumber = organisationMigratedEvent.Value.Key;

                        // Check if the user aggregate was already modified after the organisation migration event
                        var hadModifiedTimeAfterOrganisationMigration
                            = this.HadModifiedTimeAfterTheSequenceFromEvents(migrationSequenceNumber, kvpEvents);
                        if (hadModifiedTimeAfterOrganisationMigration)
                        {
                            continue; // Skip this user and proceed to the next one
                        }

                        var eventsBeforeMigration
                            = this.GetEventsBeforeSequenceFromEvents(migrationSequenceNumber, kvpEvents);
                        dynamic eventToGetModifiedTime
                            = this.GetLatestEventThatModifiesTimeFromEvents(eventsBeforeMigration);
                        var modifiedTimestamp = eventToGetModifiedTime.CreatedTimestamp;
                        var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
                        aggregate.RollbackDateModifiedBeforeOrganisationMigration(
                            modifiedTimestamp, performingUserId, this.clock.GetCurrentInstant());
                        await this.userAggregateRepository.Save(aggregate);

                        await Task.Delay(100, cancellationToken);
                    }
                }
            }

            return Unit.Value;
        }

        private bool HadModifiedTimeAfterTheSequenceFromEvents(
            int sequenceNumber, IEnumerable<KeyValuePair<int, IEvent<UserAggregate, Guid>>> events)
        {
            return events
                .Any(kvp => kvp.Key > sequenceNumber && this.eventListThatModifiesTime.Contains(kvp.GetType()));
        }

        private Maybe<KeyValuePair<int, IEvent<UserAggregate, Guid>>> GetOrganisationMigratedEventFromEvents(
            IEnumerable<KeyValuePair<int, IEvent<UserAggregate, Guid>>> events)
        {
            var isValid = events.Any(kvp => kvp.Value is UserOrganisationMigratedEvent)
                && !events.Any(kvp => kvp.Value is UserModifiedTimeUpdatedEvent);
            if (isValid)
            {
                var latestEvent = events.LastOrDefault(kvp => kvp.Value is UserOrganisationMigratedEvent);
                if (latestEvent.Value != null)
                {
                    return latestEvent;
                }
            }

            return Maybe<KeyValuePair<int, IEvent<UserAggregate, Guid>>>.None;
        }

        private IEvent<UserAggregate, Guid> GetLatestEventThatModifiesTimeFromEvents(
            IEnumerable<KeyValuePair<int, IEvent<UserAggregate, Guid>>> events)
        {
            var eventsFromLatest = events.Select(kvp => kvp.Value).Reverse();
            foreach (IEvent<UserAggregate, Guid> @event in eventsFromLatest)
            {
                if (this.eventListThatModifiesTime.Contains(@event.GetType()))
                {
                    return @event;
                }
            }

            return null;
        }

        private IEnumerable<KeyValuePair<int, IEvent<UserAggregate, Guid>>> GetEventsBeforeSequenceFromEvents(
            int sequenceNumber, IEnumerable<KeyValuePair<int, IEvent<UserAggregate, Guid>>> events)
        {
            return events.Where(kvp => kvp.Key < sequenceNumber);
        }
    }
}

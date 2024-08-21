// <copyright file="RollbackCustomerModifiedTimeCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
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
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using static UBind.Domain.Aggregates.Customer.CustomerAggregate;

    /// <summary>
    /// Command handler for reverting modified time affected by the organisation migration.
    /// </summary>
    public class RollbackCustomerModifiedTimeCommandHandler
        : ICommandHandler<RollbackCustomerModifiedTimeCommand, Unit>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        private readonly List<Type> eventListThatModifiesTime;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="RollbackCustomerModifiedTimeCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The repository for tenants.</param>
        /// <param name="customerReadModelRepository">The repository for customer read models.</param>
        /// <param name="customerAggregateRepository">The repository for customer aggregate.</param>
        /// <param name="httpContextPropertiesResolver">The resolver for performing user.</param>
        /// <param name="clock">Represents the clock which can return the time as <see cref="Instant"/>.</param>
        public RollbackCustomerModifiedTimeCommandHandler(
            ITenantRepository tenantRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.tenantRepository = tenantRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.eventListThatModifiesTime = new List<Type>
            {
                typeof(CustomerInitializedEvent),
                typeof(CustomerImportedEvent),
                typeof(OwnershipAssignedEvent),
#pragma warning disable CS0618 // Type or member is obsolete
                typeof(UserAssociatedEvent),
#pragma warning restore CS0618 // Type or member is obsolete
                typeof(CustomerDeletedEvent),
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
                typeof(UserAggregate.UserInitializedEvent),
                typeof(UserAggregate.UserImportedEvent),
                typeof(UserAggregate.UserBlockedEvent),
                typeof(UserAggregate.UserUnblockedEvent),
                typeof(UserAggregate.ActivationInvitationCreatedEvent),
                typeof(UserAggregate.UserActivatedEvent),
            };
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            RollbackCustomerModifiedTimeCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Getting all tenants to be enumerated
            var tenants = this.tenantRepository.GetTenants();
            foreach (var tenant in tenants)
            {
                // Getting all the customers per tenant
                var customers = this.customerReadModelRepository.GetCustomersMatchingFilter(
                    tenant.Id, new EntityListFilters());
                foreach (var customer in customers)
                {
                    // Get if there is an organisation migrated event for that customer aggregate
                    var aggregate = this.customerAggregateRepository.GetById(tenant.Id, customer.Id);
                    var kvpEvents = aggregate.GetUnstrippedEvents()
                        .Select((@event, i) => new KeyValuePair<int, IEvent<CustomerAggregate, Guid>>(i, @event));

                    var organisationMigratedEvent = this.GetOrganisationMigratedEventFromEvents(kvpEvents);
                    if (organisationMigratedEvent.HasValue)
                    {
                        // Get the latest event before the organisation migration sequence that modifies time
                        var migrationSequenceNumber = organisationMigratedEvent.Value.Key;

                        // Check if the customer aggregate was already modified after the organisation migration event
                        var hadModifiedTimeAfterOrganisationMigration
                            = this.HadModifiedTimeAfterTheSequenceFromEvents(migrationSequenceNumber, kvpEvents);
                        if (hadModifiedTimeAfterOrganisationMigration)
                        {
                            continue; // Skip this customer and proceed to the next one
                        }

                        var eventsBeforeMigration
                            = this.GetEventsBeforeSequenceFromEvents(migrationSequenceNumber, kvpEvents);
                        dynamic eventToGetModifiedTime
                            = this.GetLatestEventThatModifiesTimeFromEvents(eventsBeforeMigration);
                        var modifiedTime = eventToGetModifiedTime.CreatedTimestamp;
                        var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
                        aggregate.RollbackDateModifiedBeforeOrganisationMigration(
                            modifiedTime, performingUserId, this.clock.GetCurrentInstant());
                        await this.customerAggregateRepository.Save(aggregate);

                        await Task.Delay(100, cancellationToken);
                    }
                }
            }

            return Unit.Value;
        }

        private bool HadModifiedTimeAfterTheSequenceFromEvents(
            int sequenceNumber, IEnumerable<KeyValuePair<int, IEvent<CustomerAggregate, Guid>>> events)
        {
            return events
                .Any(kvp => kvp.Key > sequenceNumber && this.eventListThatModifiesTime.Contains(kvp.GetType()));
        }

        private Maybe<KeyValuePair<int, IEvent<CustomerAggregate, Guid>>> GetOrganisationMigratedEventFromEvents(
            IEnumerable<KeyValuePair<int, IEvent<CustomerAggregate, Guid>>> events)
        {
            var isValid = events.Any(kvp => kvp.Value is CustomerOrganisationMigratedEvent)
                && !events.Any(kvp => kvp.Value is CustomerModifiedTimeUpdatedEvent);
            if (isValid)
            {
                var latestEvent = events.LastOrDefault(kvp => kvp.Value is CustomerOrganisationMigratedEvent);
                if (latestEvent.Value != null)
                {
                    return latestEvent;
                }
            }

            return Maybe<KeyValuePair<int, IEvent<CustomerAggregate, Guid>>>.None;
        }

        private IEvent<CustomerAggregate, Guid> GetLatestEventThatModifiesTimeFromEvents(
            IEnumerable<KeyValuePair<int, IEvent<CustomerAggregate, Guid>>> events)
        {
            var eventsFromLatest = events.Select(kvp => kvp.Value).Reverse();
            foreach (IEvent<CustomerAggregate, Guid> @event in eventsFromLatest)
            {
                if (this.eventListThatModifiesTime.Contains(@event.GetType()))
                {
                    return @event;
                }
            }

            return null;
        }

        private IEnumerable<KeyValuePair<int, IEvent<CustomerAggregate, Guid>>> GetEventsBeforeSequenceFromEvents(
            int sequenceNumber, IEnumerable<KeyValuePair<int, IEvent<CustomerAggregate, Guid>>> events)
        {
            return events.Where(kvp => kvp.Key < sequenceNumber);
        }
    }
}

// <copyright file="PersonService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Person
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class PersonService : IPersonService
    {
        private readonly ITenantRepository tenantRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IClock clock;
        private IHttpContextPropertiesResolver httpContextPropertiesResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonService"/> class.
        /// </summary>
        /// <param name="customerAggregateRepository">The customer aggregate repository.</param>
        /// <param name="customerReadModelRepository">The customer read model repository.</param>
        /// <param name="personAggregateRepository">The person repository.</param>
        /// <param name="personReadModelRepository">The person read model repository.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="userAggregateRepository">The user aggregate repository.</param>
        /// <param name="userReadModelRepository">The user read model repository.</param>
        /// <param name="tenantRepository">The tenant repository.</param>
        /// <param name="clock">A clock for obtaining time.</param>
        public PersonService(
            ICustomerAggregateRepository customerAggregateRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            IPersonAggregateRepository personAggregateRepository,
            IPersonReadModelRepository personReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IUserAggregateRepository userAggregateRepository,
            IUserReadModelRepository userReadModelRepository,
            ITenantRepository tenantRepository,
            IClock clock)
        {
            this.tenantRepository = tenantRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.personReadModelRepository = personReadModelRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.userAggregateRepository = userAggregateRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public IPersonReadModelSummary Get(Guid tenantId, Guid id)
        {
            var person = this.personReadModelRepository.GetPersonSummaryById(tenantId, id);
            return person;
        }

        /// <inheritdoc/>
        public PersonAggregate GetByCustomerId(Guid tenantId, Guid customerId)
        {
            var customerReadModel = this.customerReadModelRepository.GetCustomerById(tenantId, customerId);
            if (customerReadModel == null)
            {
                return null;
            }

            return this.personAggregateRepository.GetById(customerReadModel.TenantId, customerReadModel.PrimaryPersonId);
        }

        /// <inheritdoc/>
        public void RecreateExistingPeopleToPersonReadModelTable()
        {
            var tenants = this.tenantRepository.GetTenants().ToList();
            this.RecreateUserToPersonReadModelTable(tenants);
            this.RecreateCustomerToPersonReadModelTable(tenants);
        }

        public async Task<PersonAggregate> CreateNewPerson(
            Guid tenantId, IPersonalDetails personDetails, bool isTestData = false)
        {
            var personAggregate = PersonAggregate.CreatePersonFromPersonalDetails(
                tenantId,
                personDetails.OrganisationId,
                personDetails,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now(),
                isTestData);
            await this.personAggregateRepository.Save(personAggregate);
            return personAggregate;
        }

        private void RecreateCustomerToPersonReadModelTable(List<Tenant> tenants)
        {
            // Note: Do not copy this workflow
            // The proper implementation for this is to get the customers by tenant Id and loop it, so that we don't
            // get a big stack of customers in a single query.
            foreach (var tenant in tenants)
            {
                var dictCustomersPersonIds = this.customerReadModelRepository.GetAllExistingCustomersPersonIds(tenant.Id);
                foreach (var customerPersonKeyValue in dictCustomersPersonIds)
                {
                    async Task MigrateCustomer()
                    {
                        var customerId = customerPersonKeyValue.Value;
                        var personId = customerPersonKeyValue.Key;
                        var customer = this.customerReadModelRepository.GetCustomerById(tenant.Id, customerId);

                        // This would trigger the re-creation of the customer's person details in PersonReadModel table.
                        await this.personAggregateRepository.ReplayAllEventsByAggregateId(tenant.Id, personId);

                        var personAggregate = this.personAggregateRepository.GetById(customer.TenantId, personId);
                        if (personAggregate.UserId.HasValue && personAggregate.UserId.Value != default)
                        {
                            await this.ReplayUserSelectedEvents(tenant.Id, personAggregate.UserId.Value);
                        }

                        if (personAggregate != null)
                        {
                            if (personAggregate.TenantId == default || customer.TenantId == default)
                            {
                                personAggregate.TriggerApplyNewIdEvent(
                                    tenant.Id, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                            }

                            // Assign customer id to the newly created person read model record
                            personAggregate.AssociateWithCustomer(
                                customerId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                            await this.personAggregateRepository.Save(personAggregate);
                        }
                    }

                    RetryPolicyHelper.Execute<Exception>(async () => await MigrateCustomer(), maxJitter: 3000);
                }
            }
        }

        private void RecreateUserToPersonReadModelTable(List<Tenant> tenants)
        {
            foreach (var tenant in tenants)
            {
                var dictUsersPersonIds = this.userReadModelRepository.GetAllExistingUsersPersonIds(tenant.Id);
                foreach (var userPersonKeyValue in dictUsersPersonIds)
                {
                    async Task MigrateUser()
                    {
                        var userId = userPersonKeyValue.Value;
                        var personId = userPersonKeyValue.Key;
                        var user = this.userReadModelRepository.GetUser(tenant.Id, userId);

                        //// this would trigger the re-creation of the user's person details in PersonReadModel table.
                        await this.personAggregateRepository.ReplayAllEventsByAggregateId(tenant.Id, personId);
                        var personAggregate = this.personAggregateRepository.GetById(tenant.Id, personId);
                        if (personAggregate != null)
                        {
                            //// assign user id to the newly created person read model record
                            personAggregate.RecordUserAccountCreatedForPerson(userId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());

                            //// replay user's selected events
                            await this.ReplayUserSelectedEvents(tenant.Id, userId);

                            if (personAggregate?.TenantId == default || user?.TenantId == default)
                            {
                                personAggregate.TriggerApplyNewIdEvent(tenant.Id, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                            }

                            await this.personAggregateRepository.Save(personAggregate);
                        }
                    }

                    RetryPolicyHelper.Execute<Exception>(async () => await MigrateUser(), maxJitter: 3000);
                }
            }
        }

        private async Task ReplayUserSelectedEvents(Guid tenantId, Guid userId)
        {
            var types = new Type[]
            {
                typeof(UserAggregate.ActivationInvitationCreatedEvent),
                typeof(UserAggregate.UserActivatedEvent),
                typeof(UserAggregate.UserBlockedEvent),
                typeof(UserAggregate.UserUnblockedEvent),
            };

            await this.userAggregateRepository.ReplayEventsOfTypeByAggregateId(tenantId, userId, types);
        }
    }
}

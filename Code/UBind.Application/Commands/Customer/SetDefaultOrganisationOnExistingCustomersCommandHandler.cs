// <copyright file="SetDefaultOrganisationOnExistingCustomersCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command handler for generating default organisation to existing customers based from tenancy.
    /// </summary>
    public class SetDefaultOrganisationOnExistingCustomersCommandHandler
        : ICommandHandler<SetDefaultOrganisationOnExistingCustomersCommand, Unit>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="SetDefaultOrganisationOnExistingCustomersCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The repository for tenants.</param>
        /// <param name="customerReadModelRepository">The repository for customer read models.</param>
        /// <param name="customerAggregateRepository">The repository for customer aggregate.</param>
        /// <param name="personAggregateRepository">The repository for person aggregate.</param>
        /// <param name="httpContextPropertiesResolver">The resolver for performing user.</param>
        /// <param name="clock">Represents the clock which can return the time as <see cref="Instant"/>.</param>
        public SetDefaultOrganisationOnExistingCustomersCommandHandler(
            ITenantRepository tenantRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.tenantRepository = tenantRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            SetDefaultOrganisationOnExistingCustomersCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenants = this.tenantRepository.GetTenants();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;

            foreach (var tenant in tenants)
            {
                var customers = this.customerReadModelRepository.GetCustomersMatchingFilter(
                    tenant.Id, new EntityListFilters());
                foreach (var customer in customers)
                {
                    var customerAggregate = this.customerAggregateRepository.GetById(tenant.Id, customer.Id);
                    if (customerAggregate?.OrganisationId == Guid.Empty)
                    {
                        customerAggregate.RecordOrganisationMigration(
                            tenant.Details.DefaultOrganisationId, performingUserId, this.clock.GetCurrentInstant());
                        await this.customerAggregateRepository.Save(customerAggregate);
                        await Task.Delay(100, cancellationToken);
                    }

                    var personAggregate = this.personAggregateRepository.GetById(tenant.Id, customer.PrimaryPersonId);
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

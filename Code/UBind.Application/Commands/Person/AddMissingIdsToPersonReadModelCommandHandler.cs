// <copyright file="AddMissingIdsToPersonReadModelCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Person
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command handler for assigning organisationId, tenantId, userId or customerId that are missing on the
    /// personReadModel.
    /// </summary>
    public class AddMissingIdsToPersonReadModelCommandHandler
        : ICommandHandler<AddMissingIdsToPersonReadModelCommand, Unit>
    {
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly ILogger<AddMissingIdsToPersonReadModelCommandHandler> logger;
        private readonly ITenantRepository tenantRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddMissingIdsToPersonReadModelCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The repository for tenants.</param>
        /// <param name="userReadModelRepository">The repository for user read models.</param>
        /// <param name="customerReadModelRepository">The repository for customer read model.</param>
        /// <param name="personAggregateRepository">The repository for person aggregate repository.</param>
        /// <param name="personReadModelRepository">The person read model repository.</param>
        /// <param name="httpContextPropertiesResolver">The resolver for performing user.</param>
        /// <param name="logger">The loggers to identify issues.</param>
        /// <param name="clock">Represents the clock which can return the time as <see cref="Instant"/>.</param>
        public AddMissingIdsToPersonReadModelCommandHandler(
            ITenantRepository tenantRepository,
            IUserReadModelRepository userReadModelRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            IPersonAggregateRepository personAggregateRepository,
            IPersonReadModelRepository personReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ILogger<AddMissingIdsToPersonReadModelCommandHandler> logger,
            IClock clock)
        {
            this.personReadModelRepository = personReadModelRepository;
            this.logger = logger;
            this.tenantRepository = tenantRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public Task<Unit> Handle(
            AddMissingIdsToPersonReadModelCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenants = this.tenantRepository.GetTenants();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;

            foreach (var tenant in tenants)
            {
                this.logger.LogInformation($"Starting Migration for Missing data for tenant {tenant.Id}.");
                var users = this.userReadModelRepository.GetAllUsersAsQueryable(tenant.Id).ToList();

                foreach (var user in users)
                {
                    async Task ProcessUser()
                    {
                        PersonReadModel personReadModel = null;
                        try
                        {
                            bool save = false;
                            var personAggregate = this.personAggregateRepository.GetById(user.TenantId, user.PersonId);
                            personReadModel = this.personReadModelRepository.GetByIdWithoutTenantIdForMigrations(
                                user.PersonId);
                            if (personAggregate == null || personReadModel == null)
                            {
                                return;
                            }

                            // this checking is needed because ther aggregate has value but the model does not.
                            if (personAggregate.OrganisationId == default
                                || personReadModel.OrganisationId == default)
                            {
                                this.logger.LogInformation($"Assigning OrganisationId {tenant.Details.DefaultOrganisationId} to Person {personAggregate.Id}");

                                // We need to bypass the organisation checking inside the function this time.
                                personAggregate.RecordOrganisationMigration(
                                    tenant.Details.DefaultOrganisationId,
                                    performingUserId,
                                    this.clock.GetCurrentInstant(),
                                    true);
                                save = true;
                            }

                            if (personAggregate.UserId != user.Id
                                || personReadModel.UserId != user.Id)
                            {
                                this.logger.LogInformation(
                                    $"Assigning UserId {user.Id} to Person {personAggregate.Id}");
                                personAggregate.RecordUserAccountCreatedForPerson(
                                    user.Id, performingUserId, this.clock.GetCurrentInstant());
                                save = true;
                            }

                            if (personAggregate.TenantId != user.TenantId
                                || personReadModel.TenantId != user.TenantId
                                || personReadModel.TenantId != user.TenantId)
                            {
                                this.logger.LogInformation(
                                    $"Assigning TenantId {tenant.Id} to Person {personAggregate.Id}");
                                personAggregate.AssignMissingTenantId(
                                    tenant.Id, performingUserId, this.clock.GetCurrentInstant());
                                save = true;
                            }

                            if (save)
                            {
                                await this.personAggregateRepository.Save(personAggregate);
                                await Task.Delay(300, cancellationToken);
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.LogInformation(
                                $"Error for Person: {personReadModel?.Id} ErrorMessage: {e.Message}");
                            throw;
                        }
                    }

                    RetryPolicyHelper.Execute<Exception>(async () => await ProcessUser());
                }

                var customers = this.customerReadModelRepository.GetCustomersMatchingFilter(
                    tenant.Id, new EntityListFilters());

                foreach (var customer in customers)
                {
                    async Task ProcessCustomer()
                    {
                        PersonReadModel personReadModel = null;
                        try
                        {
                            bool save = false;
                            var personAggregate = this.personAggregateRepository.GetById(customer.TenantId, customer.PrimaryPersonId);
                            personReadModel = this.personReadModelRepository.GetByIdWithoutTenantIdForMigrations(
                                customer.PrimaryPersonId);
                            if (personAggregate == null || personReadModel == null)
                            {
                                return;
                            }

                            // this checking is needed because ther aggregate has value but the model does not.
                            if (personAggregate.OrganisationId == default
                                || personReadModel.OrganisationId == default)
                            {
                                this.logger.LogInformation($"Assigning OrganisationId {tenant.Details.DefaultOrganisationId} to Person {personAggregate.Id}");
                                personAggregate.RecordOrganisationMigration(
                                    tenant.Details.DefaultOrganisationId,
                                    performingUserId,
                                    this.clock.GetCurrentInstant(),
                                    true);
                                save = true;
                            }

                            if (personAggregate.CustomerId != customer.Id
                                || personReadModel.CustomerId != customer.Id)
                            {
                                this.logger.LogInformation(
                                    $"Assigning CustomerId {customer.Id} to Person {personAggregate.Id}");
                                personAggregate.AssociateWithCustomer(
                                    customer.Id, performingUserId, this.clock.GetCurrentInstant());
                                save = true;
                            }

                            if (personAggregate.TenantId != customer.TenantId
                               || personReadModel.TenantId != customer.TenantId
                               || personReadModel.TenantId != customer.TenantId)
                            {
                                this.logger.LogInformation(
                                    $"Assigning TenantId {tenant.Id} to Person {personAggregate.Id}");
                                personAggregate.AssignMissingTenantId(
                                    tenant.Id, performingUserId, this.clock.GetCurrentInstant());
                                save = true;
                            }

                            if (save)
                            {
                                await this.personAggregateRepository.Save(personAggregate);
                                await Task.Delay(300, cancellationToken);
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.LogInformation(
                                $"Error for Person: {personReadModel?.Id} ErrorMessage: {e.Message}");
                            throw;
                        }
                    }

                    RetryPolicyHelper.Execute<Exception>(async () => await ProcessCustomer());
                }
            }

            return Task.FromResult(Unit.Value);
        }
    }
}

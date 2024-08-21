// <copyright file="CreatePersonForCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Person
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command handler for creating a person.
    /// </summary>
    public class CreatePersonForCustomerCommandHandler : ICommandHandler<CreatePersonForCustomerCommand, Guid>
    {
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePersonForCustomerCommandHandler"/> class.
        /// </summary>
        /// <param name="personAggregateRepository">The repository for person aggregate.</param>
        /// <param name="customerAggregateRepository">The repository for customer aggregate.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="clock">A clock for obtaining time.</param>
        public CreatePersonForCustomerCommandHandler(
            IPersonAggregateRepository personAggregateRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.personAggregateRepository = personAggregateRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Guid> Handle(CreatePersonForCustomerCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var customerAggregate = this.customerAggregateRepository.GetById(command.TenantId, command.CustomerId);
            if (customerAggregate == null)
            {
                throw new ErrorException(Errors.Customer.NotFound(command.CustomerId));
            }

            var person = PersonAggregate.CreatePersonFromPersonalDetails(
                command.TenantId,
                customerAggregate.OrganisationId,
                command.PersonDetails,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now());
            person.AssociateWithCustomer(
                customerAggregate.Id, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.personAggregateRepository.Save(person);

            return person.Id;
        }
    }
}

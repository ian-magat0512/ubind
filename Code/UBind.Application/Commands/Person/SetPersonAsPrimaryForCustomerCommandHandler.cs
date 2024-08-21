// <copyright file="SetPersonAsPrimaryForCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Person
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Command handler for assigning the person as primary for customer.
    /// </summary>
    public class SetPersonAsPrimaryForCustomerCommandHandler
        : ICommandHandler<SetPersonAsPrimaryForCustomerCommand, Unit>
    {
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public SetPersonAsPrimaryForCustomerCommandHandler(
            ICustomerAggregateRepository customerAggregateRepository,
            IPersonReadModelRepository personReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.customerAggregateRepository = customerAggregateRepository;
            this.personReadModelRepository = personReadModelRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            SetPersonAsPrimaryForCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var person = this.personReadModelRepository.GetPersonSummaryById(request.TenantId, request.PersonId);
            var customerAggregate = this.customerAggregateRepository.GetById(request.TenantId, request.CustomerId);

            if (customerAggregate.PrimaryPersonId == person.Id)
            {
                throw new ErrorException(Errors.Customer.PersonIsAlreadyAPrimaryRecord(
                    request.PersonId, person.DisplayName, customerAggregate.Id));
            }

            if (customerAggregate.TenantId != request.TenantId)
            {
                throw new ErrorException(
                    Errors.General.NotAuthorized("setting of primary person", "customer", request.CustomerId));
            }

            customerAggregate.SetPrimaryPerson(request.PersonId, performingUserId, this.clock.GetCurrentInstant());
            await this.customerAggregateRepository.Save(customerAggregate);

            return Unit.Value;
        }
    }
}

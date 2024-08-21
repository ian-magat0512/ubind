// <copyright file="UnassignPortalFromCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Command handler for un-assigning portal to a customer.
    /// </summary>
    public class UnassignPortalFromCustomerCommandHandler
        : ICommandHandler<UnassignPortalFromCustomerCommand, Unit>
    {
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public UnassignPortalFromCustomerCommandHandler(
            ICustomerAggregateRepository customerAggregateRepository,
            IUserAggregateRepository userAggregateRepository,
            IPersonReadModelRepository personReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.customerAggregateRepository = customerAggregateRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.personReadModelRepository = personReadModelRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        public async Task<Unit> Handle(UnassignPortalFromCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var customer = this.customerAggregateRepository.GetById(request.TenantId, request.CustomerId);
            if (customer == null)
            {
                EntityHelper.ThrowIfNotFound(customer, request.CustomerId, "customer");
            }
            await this.UnassignPortalFromCustomer(customer);

            // Get the person read model first to find out if it has a user account
            var personReadModel = this.personReadModelRepository.GetPersonById(
                customer.TenantId, customer.PrimaryPersonId);
            if (personReadModel != null && personReadModel.UserId.HasValue)
            {
                var user = this.userAggregateRepository.GetById(request.TenantId, personReadModel.UserId.Value);
                if (user == null)
                {
                    throw new ErrorException(Errors.General.Unexpected(
                        "When trying to unassign the portal from a customer, " +
                        "the PersonReadModel for that customer had a non null UserId " +
                        $"\"{personReadModel.UserId}\", and when we tried to fetch the user, it was not found."));
                }
                await this.UnassignPortalFromUser(user);
            }
            return Unit.Value;
        }

        private async Task UnassignPortalFromUser(UserAggregate user)
        {
            if (user.PortalId != null)
            {
                user.ChangePortal(
                null, this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
                await this.userAggregateRepository.Save(user);
            }
        }

        private async Task UnassignPortalFromCustomer(CustomerAggregate customer)
        {
            if (customer.PortalId != null)
            {
                customer.ChangePortal(
                    null, this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
                await this.customerAggregateRepository.Save(customer);
            }
        }
    }
}

// <copyright file="AssignPortalToCustomerCommandHandler.cs" company="uBind">
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
    /// Command handler for assigning portal to a customer.
    /// </summary>
    public class AssignPortalToCustomerCommandHandler : ICommandHandler<AssignPortalToCustomerCommand, Unit>
    {
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly ICachingResolver cachingResolver;

        public AssignPortalToCustomerCommandHandler(
            ICustomerAggregateRepository customerAggregateRepository,
            IUserAggregateRepository userAggregateRepository,
            IPersonReadModelRepository personReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            ICachingResolver cachingResolver)
        {
            this.customerAggregateRepository = customerAggregateRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.personReadModelRepository = personReadModelRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.cachingResolver = cachingResolver;
        }

        public async Task<Unit> Handle(AssignPortalToCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.cachingResolver.GetPortalOrThrow(request.TenantId, request.PortalId);
            var customer = this.customerAggregateRepository.GetById(request.TenantId, request.CustomerId);
            if (customer == null)
            {
                EntityHelper.ThrowIfNotFound(customer, request.CustomerId, "customer");
            }
            await this.AssignPortalToCustomer(customer, request.PortalId);

            // Get the person read model first to find out if it has a user account
            var personReadModel = this.personReadModelRepository.GetPersonById(
                customer.TenantId, customer.PrimaryPersonId);
            if (personReadModel != null && personReadModel.UserId.HasValue)
            {
                var user = this.userAggregateRepository.GetById(request.TenantId, personReadModel.UserId.Value);
                if (user == null)
                {
                    throw new ErrorException(Errors.General.Unexpected("When trying to assign the portal to a customer, " +
                        "the PersonReadModel for that customer had a non null UserId " +
                        $"\"{personReadModel.UserId}\", and when we tried to fetch the user, it was not found."));
                }
                await this.AssignPortalToUser(user, request.PortalId);
            }

            return Unit.Value;
        }

        private async Task AssignPortalToUser(UserAggregate user, Guid newPortalId)
        {
            if (user.PortalId != newPortalId)
            {
                user.ChangePortal(
                    newPortalId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
                await this.userAggregateRepository.Save(user);
            }
        }

        private async Task<CustomerAggregate> AssignPortalToCustomer(CustomerAggregate customer, Guid newPortalId)
        {
            if (customer.PortalId != newPortalId)
            {
                customer.ChangePortal(
                    newPortalId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
                await this.customerAggregateRepository.Save(customer);
            }

            return customer;
        }
    }
}

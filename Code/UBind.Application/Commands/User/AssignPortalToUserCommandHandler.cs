// <copyright file="AssignPortalToUserCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
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

    public class AssignPortalToUserCommandHandler : ICommandHandler<AssignPortalToUserCommand, Unit>
    {
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly ICachingResolver cachingResolver;

        public AssignPortalToUserCommandHandler(
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

        public async Task<Unit> Handle(AssignPortalToUserCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.cachingResolver.GetPortalOrThrow(command.TenantId, command.PortalId);
            var user = this.userAggregateRepository.GetById(command.TenantId, command.UserId);
            if (user == null)
            {
                EntityHelper.ThrowIfNotFound(user, command.UserId, "user");
            }
            await this.AssignPortalToUser(user, command.PortalId);

            // Get the person read model first to find out if it has a customer account
            var personReadModel = this.personReadModelRepository.GetPersonById(
                user.TenantId, user.PersonId);
            if (personReadModel != null && personReadModel.CustomerId.HasValue)
            {
                var customer = this.customerAggregateRepository.GetById(user.TenantId, personReadModel.CustomerId.Value);
                if (customer == null)
                {
                    throw new ErrorException(Errors.General.Unexpected(
                       "When trying to assign the portal to a user, " +
                       "the PersonReadModel for that user had a non null CustomerId " +
                       $"\"{personReadModel.CustomerId}\", and when we tried to fetch the customer, it was not found."));
                }
                await this.AssignPortalToCustomer(customer, command.PortalId);
            }

            return Unit.Value;
        }

        private async Task AssignPortalToCustomer(CustomerAggregate customer, Guid newPortalId)
        {
            if (customer.PortalId != newPortalId)
            {
                customer.ChangePortal(
                    newPortalId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
                await this.customerAggregateRepository.Save(customer);
            }
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
    }
}

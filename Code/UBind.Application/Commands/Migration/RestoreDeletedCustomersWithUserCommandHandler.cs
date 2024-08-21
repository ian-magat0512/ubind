// <copyright file="RestoreDeletedCustomersWithUserCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Application.Commands.Customer.Merge;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories;

    public class RestoreDeletedCustomersWithUserCommandHandler : ICommandHandler<RestoreDeletedCustomersWithUserCommand, Unit>
    {
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IWritableReadModelRepository<UserReadModel> userReadModelRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ICqrsMediator mediator;
        private readonly IClock clock;

        public RestoreDeletedCustomersWithUserCommandHandler(
            ICustomerAggregateRepository customerAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            IUserAggregateRepository userAggregateRepository,
            IWritableReadModelRepository<UserReadModel> userReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICqrsMediator mediator,
            IClock clock)
        {
            this.customerAggregateRepository = customerAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.mediator = mediator;
            this.clock = clock;
        }

        public async Task<Unit> Handle(RestoreDeletedCustomersWithUserCommand request, CancellationToken cancellationToken)
        {
            var deletedCustomersWithUser = this.customerReadModelRepository.GetDeletedCustomersWithUser();
            foreach (var deletedCustomer in deletedCustomersWithUser)
            {
                var customerAggregate = this.customerAggregateRepository.GetById(deletedCustomer.TenantId, deletedCustomer.Id);
                customerAggregate.MarkAsUndeleted(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                await this.customerAggregateRepository.Save(customerAggregate);

                var personAggregate = this.personAggregateRepository.GetById(deletedCustomer.TenantId, deletedCustomer.PrimaryPersonId);
                if (personAggregate.IsDeleted)
                {
                    personAggregate.Restore(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                    await this.personAggregateRepository.Save(personAggregate);
                }

                if (personAggregate.UserId.HasValue)
                {
                    var userAggregate = this.userAggregateRepository.GetById(deletedCustomer.TenantId, personAggregate.UserId.Value);
                    var userReadModel = this.userReadModelRepository.GetById(userAggregate.TenantId, userAggregate.Id);
                    if (userReadModel == null)
                    {
                        // Replay all events for the user aggregate to reacreate or sync the user read model.
                        await this.userAggregateRepository.ReplayAllEventsByAggregateId(deletedCustomer.TenantId, personAggregate.UserId.Value);
                        await this.userAggregateRepository.Save(userAggregate);
                    }

                    if (userAggregate.IsDeleted)
                    {
                        userAggregate.Restore(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                        await this.userAggregateRepository.Save(userAggregate);
                    }
                }

                if (deletedCustomer.Environment == DeploymentEnvironment.Production)
                {
                    // remerge existing customers into the restored customer in production environment
                    var mergeCommand = new MergeCustomerIntoExistingInvitedOrActivatedCustomerCommand(
                        customerAggregate.TenantId, customerAggregate.Environment, personAggregate.Id);
                    await this.mediator.Send(mergeCommand);
                }
            }

            return Unit.Value;
        }
    }
}

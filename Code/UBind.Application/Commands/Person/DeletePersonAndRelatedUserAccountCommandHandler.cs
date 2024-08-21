// <copyright file="DeletePersonAndRelatedUserAccountCommandHandler.cs" company="uBind">
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
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command handler for deleting a person and its related user accounts.
    /// </summary>
    public class DeletePersonAndRelatedUserAccountCommandHandler
        : ICommandHandler<DeletePersonAndRelatedUserAccountCommand, Unit>
    {
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IUserLoginEmailRepository userLoginEmailRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IUserService userService;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeletePersonAndRelatedUserAccountCommandHandler"/> class.
        /// </summary>
        /// <param name="personAggregateRepository">The repository for person aggregate.</param>
        /// <param name="userAggregateRepository">The repository for user aggregate.</param>
        /// <param name="userLoginEmailRepository">The repository for user login.</param>
        /// <param name="customerAggregateRepository">The repository for customer aggregate.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="clock">A clock for obtaining time.</param>
        public DeletePersonAndRelatedUserAccountCommandHandler(
            IPersonAggregateRepository personAggregateRepository,
            IUserAggregateRepository userAggregateRepository,
            IUserLoginEmailRepository userLoginEmailRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IUserService userService,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.personAggregateRepository = personAggregateRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.userLoginEmailRepository = userLoginEmailRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.userService = userService;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            DeletePersonAndRelatedUserAccountCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var personAggregate = this.personAggregateRepository.GetById(command.TenantId, command.PersonId);

            if (personAggregate == null)
            {
                throw new ErrorException(Errors.Person.NotFound(command.PersonId));
            }

            if (personAggregate.TenantId != command.TenantId)
            {
                throw new ErrorException(Errors.General.NotAuthorized("delete", "person", command.PersonId));
            }

            if (personAggregate.IsDeleted)
            {
                throw new ErrorException(Errors.Person.PersonRecordAlreadyDeleted(command.PersonId));
            }

            if (personAggregate.CustomerId.HasValue)
            {
                var customer = this.customerAggregateRepository.GetById(personAggregate.TenantId, personAggregate.CustomerId.Value);
                if (personAggregate.Id == customer.PrimaryPersonId)
                {
                    throw new ErrorException(
                        Errors.Person.DeletePrimaryPersonRecord(command.PersonId, personAggregate.FullName));
                }
            }

            if (personAggregate.UserId.HasValue)
            {
                await this.userService.Delete(personAggregate.TenantId, personAggregate.UserId.Value);
            }

            personAggregate.MarkAsDeleted(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.personAggregateRepository.ApplyChangesToDbContext(personAggregate);

            return Unit.Value;
        }
    }
}

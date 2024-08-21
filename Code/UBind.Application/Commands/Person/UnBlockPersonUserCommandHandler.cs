// <copyright file="UnBlockPersonUserCommandHandler.cs" company="uBind">
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
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command handler for blocking or unblocking the person's user account.
    /// </summary>
    public class UnBlockPersonUserCommandHandler : ICommandHandler<UnBlockPersonUserCommand, Unit>
    {
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnBlockPersonUserCommandHandler"/> class.
        /// </summary>
        /// <param name="personAggregateRepository">The repository for person aggregate.</param>
        /// <param name="userAggregateRepository">The repository for user aggregate.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="clock">A clock for obtaining time.</param>
        public UnBlockPersonUserCommandHandler(
            IPersonAggregateRepository personAggregateRepository,
            IUserAggregateRepository userAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.personAggregateRepository = personAggregateRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(UnBlockPersonUserCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var personAggregate = this.personAggregateRepository.GetById(command.TenantId, command.PersonId);
            if (personAggregate == null)
            {
                throw new ErrorException(Errors.Person.NotFound(command.PersonId));
            }

            if (personAggregate.TenantId != command.TenantId)
            {
                throw new ErrorException(Errors.General.NotAuthorized("unblock", "person", command.PersonId));
            }

            if (personAggregate.UserId.HasValue)
            {
                var userAggregate = this.userAggregateRepository.GetById(command.TenantId, personAggregate.UserId.Value);
                if (userAggregate == null)
                {
                    throw new ErrorException(Errors.User.NotFound(personAggregate.UserId.Value));
                }

                userAggregate.Unblock(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                await this.userAggregateRepository.Save(userAggregate);

                return Unit.Value;
            }

            throw new ErrorException(Errors.Person.UserNotFound(personAggregate.Id));
        }
    }
}

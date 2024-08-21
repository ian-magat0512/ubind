// <copyright file="UpdatePersonCommandHandler.cs" company="uBind">
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
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command handler for updating person record.
    /// </summary>
    public class UpdatePersonCommandHandler : ICommandHandler<UpdatePersonCommand, Unit>
    {
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IClock clock;
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePersonCommandHandler"/> class.
        /// </summary>
        /// <param name="personAggregateRepository">The repository for person aggregate.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="clock">A clock for obtaining time.</param>
        public UpdatePersonCommandHandler(
            IPersonAggregateRepository personAggregateRepository,
            IUserAggregateRepository userAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IUBindDbContext dbContext)
        {
            this.personAggregateRepository = personAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.userAggregateRepository = userAggregateRepository;
            this.clock = clock;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(UpdatePersonCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var person = this.personAggregateRepository.GetById(command.TenantId, command.PersonId);
            if (person == null)
            {
                throw new ErrorException(Errors.Person.NotFound(command.PersonId));
            }

            string oldEmail = person.Email;
            person.Update(command.PersonDetails, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.personAggregateRepository.ApplyChangesToDbContext(person);

            if (!string.IsNullOrEmpty(oldEmail) && oldEmail != person.Email && person.UserId != null)
            {
                var user = this.userAggregateRepository.GetById(person.TenantId, person.UserId.Value);
                user.SetLoginEmail(person.Email, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                await this.userAggregateRepository.ApplyChangesToDbContext(user);
            }

            this.dbContext.SaveChanges();
            return Unit.Value;
        }
    }
}

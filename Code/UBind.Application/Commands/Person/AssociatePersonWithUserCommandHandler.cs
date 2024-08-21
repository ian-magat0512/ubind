// <copyright file="AssociatePersonWithUserCommandHandler.cs" company="uBind">
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

    public class AssociatePersonWithUserCommandHandler : ICommandHandler<AssociatePersonWithUserCommand, Unit>
    {
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public AssociatePersonWithUserCommandHandler(
            IUserAggregateRepository userAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.userAggregateRepository = userAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        public async Task<Unit> Handle(AssociatePersonWithUserCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var userAggregate = this.userAggregateRepository.GetById(request.TenantId, request.UserId);
            if (userAggregate == null)
            {
                throw new ErrorException(Errors.Customer.NotFound(request.UserId));
            }

            var personAggregate = this.personAggregateRepository.GetById(request.TenantId, request.PersonId);
            personAggregate.AssociateWithUserAccount(
                userAggregate.Id, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.personAggregateRepository.Save(personAggregate);

            return Unit.Value;
        }
    }
}

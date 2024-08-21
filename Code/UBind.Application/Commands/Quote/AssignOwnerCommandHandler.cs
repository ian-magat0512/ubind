// <copyright file="AssignOwnerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class AssignOwnerCommandHandler : ICommandHandler<AssignOwnerCommand>
    {
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly IPersonReadModelRepository personReadModelRepository;

        public AssignOwnerCommandHandler(
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IPersonReadModelRepository personReadModelRepository)
        {
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.personReadModelRepository = personReadModelRepository;
        }

        public Task<Unit> Handle(AssignOwnerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var person = this.personReadModelRepository.GetPersonDetailForOwnerAssignmentByUserId(request.QuoteAggregate.TenantId, request.OwnerUserId);
            if (person != null)
            {
                request.QuoteAggregate.AssignToOwner(
                        person.UserId.Value,
                        person.Id,
                        person.FullName,
                        this.httpContextPropertiesResolver.PerformingUserId,
                        this.clock.Now());
            }
            return Task.FromResult(Unit.Value);
        }
    }
}

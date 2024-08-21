// <copyright file="CreateCustomerForClaimCommandHandler.cs" company="uBind">
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
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    public class CreateCustomerForClaimCommandHandler : ICommandHandler<CreateCustomerForClaimCommand, Guid>
    {
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ICustomerService customerService;
        private readonly IClock clock;
        private readonly IMediator mediator;

        public CreateCustomerForClaimCommandHandler(
            IClaimAggregateRepository claimAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICustomerService customerService,
            IClock clock,
            IMediator mediator)
        {
            this.claimAggregateRepository = claimAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.customerService = customerService;
            this.clock = clock;
            this.mediator = mediator;
        }

        public async Task<Guid> Handle(CreateCustomerForClaimCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var customerAggregate = await this.customerService.CreateCustomerForNewPerson(
                request.TenantId,
                request.Environment,
                request.PersonDetails,
                request.OwnerId,
                request.PortalId,
                request.IsTestData);
            request.ClaimAggregate.AssignToCustomer(
                customerAggregate, request.PersonDetails, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.claimAggregateRepository.ApplyChangesToDbContext(request.ClaimAggregate);
            return customerAggregate.Id;
        }
    }
}

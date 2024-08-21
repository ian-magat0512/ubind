// <copyright file="AssociateClaimWithCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// A command handler that is responsible for associating claim with an existing customer record.
    /// </summary>
    public class AssociateClaimWithCustomerCommandHandler : ICommandHandler<AssociateClaimWithCustomerCommand>
    {
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public AssociateClaimWithCustomerCommandHandler(
            IClaimReadModelRepository claimReadModelRepository,
            IClaimAggregateRepository claimAggregateRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.claimReadModelRepository = claimReadModelRepository;
            this.claimAggregateRepository = claimAggregateRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        public async Task<Unit> Handle(AssociateClaimWithCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var customerAggregate = this.customerAggregateRepository.GetById(request.TenantId, request.CustomerId);
            if (customerAggregate == null)
            {
                throw new ErrorException(Errors.Customer.NotFound(request.CustomerId));
            }

            // Every customer aggregate must have one primary person
            var personAggregate = this.personAggregateRepository.GetById(customerAggregate.TenantId, customerAggregate.PrimaryPersonId);
            var personalDetails = new PersonalDetails(personAggregate);

            // Process the claims
            var claims = this.claimReadModelRepository.ListClaimsByPolicy(request.TenantId, request.PolicyId, new EntityListFilters()).ToList();
            foreach (var claim in claims)
            {
                // Assign claim aggregate with another existing customer aggregate
                var claimAggregate = this.claimAggregateRepository.GetById(claim.TenantId, claim.Id);
                claimAggregate.AssignToCustomer(customerAggregate, personalDetails, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                await this.claimAggregateRepository.Save(claimAggregate);
                await Task.Delay(100, cancellationToken);
            }

            return Unit.Value;
        }
    }
}

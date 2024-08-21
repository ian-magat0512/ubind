// <copyright file="AssignOwnerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;

    public class AssignOwnerCommandHandler : ICommandHandler<AssignOwnerCommand, Unit>
    {
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteService quoteService;
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IClaimService claimService;
        private readonly IClock clock;

        public AssignOwnerCommandHandler(
            ICustomerAggregateRepository customerAggregateRepository,
            IUserAggregateRepository userAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteService quoteService,
            IClaimAggregateRepository claimAggregateRepository,
            IClaimService claimService,
            IClock clock)
        {
            this.customerAggregateRepository = customerAggregateRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.quoteService = quoteService;
            this.claimAggregateRepository = claimAggregateRepository;
            this.claimService = claimService;
            this.clock = clock;
        }

        public async Task<Unit> Handle(AssignOwnerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var customerAggregate = this.customerAggregateRepository.GetById(request.TenantId, request.CustomerId);

            if (customerAggregate == null)
            {
                throw new ErrorException(Errors.General.NotFound("customer", request.CustomerId));
            }

            if (customerAggregate.TenantId != request.TenantId)
            {
                throw new ErrorException(Errors.General.NotAuthorized($"update for customer", "customer", request.CustomerId));
            }

            var filter = new QuoteReadModelFilters
            {
                CustomerId = request.CustomerId,
            };

            var quotes = this.quoteService.GetQuotes(request.TenantId, filter).ToList();
            var claims = this.claimService.GetClaims(request.TenantId, filter).ToList();

            if (request.OwnerUserId != null)
            {
                var ownerUser = this.userAggregateRepository.GetById(request.TenantId, request.OwnerUserId.Value);
                var ownerPerson = this.personAggregateRepository.GetById(request.TenantId, ownerUser.PersonId);
                customerAggregate.AssignOwnership(request.OwnerUserId.Value, ownerPerson, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());

                foreach (var quote in quotes)
                {
                    var quoteAggregate = this.quoteAggregateRepository.GetById(request.TenantId, quote.AggregateId);
                    quoteAggregate.AssignToOwner(ownerPerson, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                    await this.quoteAggregateRepository.Save(quoteAggregate);
                }

                foreach (var claim in claims)
                {
                    var claimAggregate = this.claimAggregateRepository.GetById(request.TenantId, claim.Id);
                    claimAggregate.AssignToOwner(ownerUser.Id, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                    await this.claimAggregateRepository.Save(claimAggregate);
                }
            }
            else
            {
                customerAggregate.UnassignOwnership(this.httpContextPropertiesResolver.PerformingUserId.Value, this.clock.Now());

                foreach (var quote in quotes)
                {
                    var quoteAggregate = this.quoteAggregateRepository.GetById(request.TenantId, quote.AggregateId);
                    quoteAggregate.UnassignOwnership(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                    await this.quoteAggregateRepository.Save(quoteAggregate);
                }

                foreach (var claim in claims)
                {
                    var claimAggregate = this.claimAggregateRepository.GetById(request.TenantId, claim.Id);
                    claimAggregate.UnassignOwnership(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                    await this.claimAggregateRepository.Save(claimAggregate);
                }
            }

            await this.customerAggregateRepository.Save(customerAggregate);

            return Unit.Value;
        }
    }
}

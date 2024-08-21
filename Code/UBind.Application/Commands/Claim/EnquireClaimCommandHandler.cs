// <copyright file="EnquireClaimCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Claim
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the handler for lodging a claim enquiry.
    /// </summary>
    public class EnquireClaimCommandHandler : ICommandHandler<EnquireClaimCommand, Unit>
    {
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public EnquireClaimCommandHandler(
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IClaimAggregateRepository claimAggregateRepository)
        {
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.claimAggregateRepository = claimAggregateRepository;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(EnquireClaimCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var claimAggregate = this.claimAggregateRepository.GetById(command.TenantId, command.ClaimId);
            claimAggregate = EntityHelper.ThrowIfNotFound(claimAggregate, command.ClaimId, "Claim");
            claimAggregate.UpdateFormData(command.FormDataJson, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            claimAggregate.MakeEnquiry(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.claimAggregateRepository.Save(claimAggregate);
            return Unit.Value;
        }
    }
}

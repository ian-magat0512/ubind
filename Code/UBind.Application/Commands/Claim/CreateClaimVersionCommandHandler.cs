// <copyright file="CreateClaimVersionCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Commands.Claim;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// Represents the handler for creating new quote version.
    /// </summary>
    public class CreateClaimVersionCommandHandler : ICommandHandler<CreateClaimVersionCommand, ClaimReadModel>
    {
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly IClaimAggregateRepository claimAggregateRepository;

        public CreateClaimVersionCommandHandler(
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IClaimAggregateRepository claimAggregateRepository)
        {
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.claimAggregateRepository = claimAggregateRepository;
        }

        /// <inheritdoc/>
        public async Task<ClaimReadModel> Handle(CreateClaimVersionCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var claimAggregate = this.claimAggregateRepository.GetById(command.TenantId, command.ClaimId);
            var timestamp = this.clock.Now();
            if (command.FormData != null)
            {
                claimAggregate.UpdateFormData(command.FormData, this.httpContextPropertiesResolver.PerformingUserId, timestamp);
            }

            claimAggregate.CreateVersion(this.httpContextPropertiesResolver.PerformingUserId, timestamp);
            await this.claimAggregateRepository.Save(claimAggregate);
            return claimAggregate.ReadModel;
        }
    }
}

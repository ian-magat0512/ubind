// <copyright file="CreateClaimCalculationCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the handler for creating new quote version.
    /// </summary>
    public class CreateClaimCalculationCommandHandler : ICommandHandler<CreateClaimCalculationCommand, Result<Guid, Error>>
    {
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly ICalculationService calculationService;
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IReleaseQueryService releaseQueryService;

        public CreateClaimCalculationCommandHandler(
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            ICalculationService calculationService,
            IClaimAggregateRepository claimAggregateRepository,
            IReleaseQueryService releaseQueryService)
        {
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.claimAggregateRepository = claimAggregateRepository;
            this.calculationService = calculationService;
            this.releaseQueryService = releaseQueryService;
        }

        public async Task<Result<Guid, Error>> Handle(CreateClaimCalculationCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var claimAggregate = this.claimAggregateRepository.GetById(command.ProductContext.TenantId, command.ClaimId);
            var releaseContext = this.releaseQueryService.GetDefaultReleaseContextOrThrow(
                claimAggregate.TenantId,
                claimAggregate.ProductId,
                claimAggregate.Environment);
            var calculationJson = this.calculationService.GetClaimCalculation(releaseContext, command.FormDataJson);
            claimAggregate.UpdateFormData(command.FormDataJson, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            claimAggregate.RecordCalculationResult(command.FormDataJson, calculationJson, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.claimAggregateRepository.Save(claimAggregate);
            return Result.Success<Guid, Error>(claimAggregate.Id);
        }
    }
}

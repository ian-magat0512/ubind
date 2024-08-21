// <copyright file="ChangeClaimStateCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim;

using System.Threading;
using System.Threading.Tasks;
using UBind.Application.Releases;
using UBind.Domain.Aggregates.Claim;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel.Claim;
using UBind.Domain.Services;

public class ChangeClaimStateCommandHandler : ICommandHandler<ChangeClaimStateCommand, ClaimReadModel>
{
    private readonly IClaimService claimService;
    private readonly IClaimAggregateRepository claimAggregateRepository;
    private readonly IReleaseQueryService releaseQueryService;

    public ChangeClaimStateCommandHandler(
        IClaimService claimService,
        IClaimAggregateRepository claimAggregateRepository,
        IReleaseQueryService releaseQueryService)
    {
        this.claimService = claimService;
        this.claimAggregateRepository = claimAggregateRepository;
        this.releaseQueryService = releaseQueryService;
    }

    public async Task<ClaimReadModel> Handle(ChangeClaimStateCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var claimAggregate = this.claimAggregateRepository.GetById(command.TenantId, command.ClaimId);
        EntityHelper.ThrowIfNotFound(claimAggregate, command.ClaimId, "claim");
        var releaseContext = this.releaseQueryService.GetDefaultReleaseContextOrThrow(
            command.TenantId,
            claimAggregate.ProductId,
            claimAggregate.Environment);
        await this.claimService.ChangeClaimState(releaseContext, claimAggregate, command.Operation, command.FormDataJson);
        await this.claimAggregateRepository.Save(claimAggregate);
        if (claimAggregate.ReadModel == null)
        {
            throw new InvalidOperationException("After updating the claim state, the read model was not updated. This is unexpected.");
        }

        return claimAggregate.ReadModel;
    }
}

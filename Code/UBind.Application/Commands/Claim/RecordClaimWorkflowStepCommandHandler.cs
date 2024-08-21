// <copyright file="RecordClaimWorkflowStepCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim;

using NodaTime;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain;
using UBind.Domain.Aggregates.Claim;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel.Claim;

public class RecordClaimWorkflowStepCommandHandler : ICommandHandler<RecordClaimWorkflowStepCommand, ClaimReadModel>
{
    private readonly IClaimAggregateRepository claimAggregateRepository;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly IClock clock;

    public RecordClaimWorkflowStepCommandHandler(
        IClaimAggregateRepository quoteAggregateRepository,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        IClock clock)
    {
        this.claimAggregateRepository = quoteAggregateRepository;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.clock = clock;
    }

    public async Task<ClaimReadModel> Handle(RecordClaimWorkflowStepCommand command, CancellationToken cancellationToken)
    {
        var claimAggregate = this.claimAggregateRepository.GetById(command.TenantId, command.ClaimId);
        EntityHelper.ThrowIfNotFound(claimAggregate, command.ClaimId, "claim");
        claimAggregate.RecordWorkflowStep(
            command.WorkflowStep,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.Now());
        await this.claimAggregateRepository.Save(claimAggregate);
        return claimAggregate.ReadModel;
    }
}

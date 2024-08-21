// <copyright file="RecordClaimWorkflowStepCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim;

using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel.Claim;

public class RecordClaimWorkflowStepCommand : ICommand<ClaimReadModel>
{
    public RecordClaimWorkflowStepCommand(Guid tenantId, Guid claimId, string workflowStep)
    {
        this.TenantId = tenantId;
        this.ClaimId = claimId;
        this.WorkflowStep = workflowStep;
    }

    public Guid TenantId { get; }

    public Guid ClaimId { get; }

    public string WorkflowStep { get; }
}

// <copyright file="RecordQuoteWorkflowStepCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote;

using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;

public class RecordQuoteWorkflowStepCommand : ICommand<NewQuoteReadModel>
{
    public RecordQuoteWorkflowStepCommand(Guid tenantId, Guid quoteId, string workflowStep)
    {
        this.TenantId = tenantId;
        this.QuoteId = quoteId;
        this.WorkflowStep = workflowStep;
    }

    public Guid TenantId { get; }

    public Guid QuoteId { get; }

    public string WorkflowStep { get; }
}

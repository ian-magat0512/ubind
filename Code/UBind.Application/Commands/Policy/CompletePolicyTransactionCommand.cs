// <copyright file="CompletePolicyTransactionCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Commands.Policy;

using System;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;

[CreateTransactionThatSavesChangesIfNoneExists]
public class CompletePolicyTransactionCommand : ICommand<NewQuoteReadModel>
{
    /// <summary>
    /// Constructor to be used when quote Id is the only one present.
    /// </summary>
    /// <param name="tenantId">The Id of the tenant.</param>
    /// <param name="quoteId">The Id of the quote.</param>
    /// <param name="calculationResultId">The Id of the calculation result.</param>
    /// <param name="latestFormData">The latest form data.</param>
    public CompletePolicyTransactionCommand(
        Guid tenantId,
        Guid quoteId,
        Guid calculationResultId,
        FormData latestFormData,
        bool progressQuoteState = true)
    {
        this.TenantId = tenantId;
        this.QuoteId = quoteId;
        this.CalculationResultId = calculationResultId;
        this.LatestFormData = latestFormData;
        this.ProgressQuoteState = progressQuoteState;
    }

    public Guid TenantId { get; }

    public Guid QuoteId { get; }

    public Guid CalculationResultId { get; }

    public FormData LatestFormData { get; }

    public bool ProgressQuoteState { get; }
}

// <copyright file="ActualiseQuoteCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote;

using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;

/// <summary>
/// This command returns an instance of <see cref="NewQuoteReadModel"/> after actualising a quote, only if
/// the quote was actualised successfully. If it was already actualised, it will return null.
/// </summary>
public class ActualiseQuoteCommand : ICommand<NewQuoteReadModel?>
{
    public ActualiseQuoteCommand(Guid tenantId, Guid quoteId, Domain.Aggregates.Quote.FormData? formData)
    {
        this.TenantId = tenantId;
        this.QuoteId = quoteId;
        this.FormData = formData;
    }

    public Guid TenantId { get; }

    public Guid QuoteId { get; }

    public Domain.Aggregates.Quote.FormData? FormData { get; }
}

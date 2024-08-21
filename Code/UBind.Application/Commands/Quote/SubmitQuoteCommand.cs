// <copyright file="SubmitQuoteCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote;

using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;

/// <summary>
/// Represents the command for submitting a quote.
/// </summary>
public class SubmitQuoteCommand : ICommand<NewQuoteReadModel>
{
    public SubmitQuoteCommand(Guid tenantId, Guid quoteId, FormData? formdata)
    {
        this.TenantId = tenantId;
        this.QuoteId = quoteId;
        this.Formdata = formdata;
    }

    public Guid TenantId { get; }

    public Guid QuoteId { get; }

    public FormData? Formdata { get; }
}

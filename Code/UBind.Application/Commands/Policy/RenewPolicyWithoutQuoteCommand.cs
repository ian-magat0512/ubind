// <copyright file="RenewPolicyWithoutQuoteCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Policy;

using NodaTime;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadWriteModel;

/// <summary>
/// Command for renewing a policy without a quote.
/// </summary>
public class RenewPolicyWithoutQuoteCommand : ICommand<QuoteAggregate>
{
    public RenewPolicyWithoutQuoteCommand(
        Guid tenantId,
        Guid quoteAggregateId,
        IPolicyReadModelDetails policy,
        CalculationResult calculationResult,
        FormData finalFormData,
        DateTimeZone timezone,
        ReleaseContext releaseContext)
    {
        this.TenantId = tenantId;
        this.QuoteAggregateId = quoteAggregateId;
        this.CalculationResult = calculationResult;
        this.FinalFormData = finalFormData;
        this.Timezone = timezone;
        this.ReleaseContext = releaseContext;
        this.Policy = policy;
    }

    public Guid QuoteAggregateId { get; }

    public Guid TenantId { get; }

    public CalculationResult CalculationResult { get; }

    public FormData FinalFormData { get; }

    public DateTimeZone Timezone { get; }

    public ReleaseContext ReleaseContext { get; }

    public IPolicyReadModelDetails Policy { get; }
}

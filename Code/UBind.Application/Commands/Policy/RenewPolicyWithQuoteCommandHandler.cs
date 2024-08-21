// <copyright file="RenewPolicyWithQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Policy;

using UBind.Application.Services;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Services;

/// <summary>
/// Command handler for renewing a policy with a quote.
/// </summary>
public class RenewPolicyWithQuoteCommandHandler : ICommandHandler<RenewPolicyWithQuoteCommand, QuoteAggregate>
{
    private readonly IAggregateLockingService aggregateLockingService;
    private readonly IPolicyRenewalService policyRenewalService;

    public RenewPolicyWithQuoteCommandHandler(
        IAggregateLockingService aggregationLockingService,
        IPolicyRenewalService policyRenewalService)
    {
        this.aggregateLockingService = aggregationLockingService;
        this.policyRenewalService = policyRenewalService;
    }

    public async Task<QuoteAggregate> Handle(RenewPolicyWithQuoteCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, command.QuoteAggregateId, AggregateType.Quote))
        {
            return await this.policyRenewalService.RenewPolicyWithQuote(
                command.TenantId, command.QuoteAggregateId, command.CalculationResult, command.FinalFormData, command.Timezone, command.ReleaseContext);
        }
    }
}

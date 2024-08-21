// <copyright file="RenewPolicyWithoutQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Policy;

using UBind.Application.Services;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Services;

/// <summary>
/// Command handler for renewing a policy without a quote.
/// </summary>
public class RenewPolicyWithoutQuoteCommandHandler : ICommandHandler<RenewPolicyWithoutQuoteCommand, QuoteAggregate>
{
    private readonly IAggregateLockingService aggregateLockingService;
    private readonly IPolicyRenewalService policyRenewalService;

    public RenewPolicyWithoutQuoteCommandHandler(
        IAggregateLockingService aggregationLockingService,
        IPolicyRenewalService policyRenewalService)
    {
        this.aggregateLockingService = aggregationLockingService;
        this.policyRenewalService = policyRenewalService;
    }

    public async Task<QuoteAggregate> Handle(RenewPolicyWithoutQuoteCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, command.QuoteAggregateId, AggregateType.Quote))
        {
            return await this.policyRenewalService.RenewPolicyWithoutQuote(
                command.TenantId, command.QuoteAggregateId, command.Policy, command.CalculationResult, command.FinalFormData, command.Timezone, command.ReleaseContext);
        }
    }
}

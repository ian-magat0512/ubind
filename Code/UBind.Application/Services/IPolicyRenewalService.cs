// <copyright file="IPolicyRenewalService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services;

using NodaTime;
using System.Threading.Tasks;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Product;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadWriteModel;

public interface IPolicyRenewalService
{
    /// <summary>
    /// Creates a policy renewal from a quote provided.
    /// </summary>
    Task<QuoteAggregate> RenewPolicyWithQuote(
        Guid tenantId,
        Guid quoteAggregateId,
        CalculationResult calculationResult,
        FormData finalFormData,
        DateTimeZone timeZone,
        ReleaseContext releaseContext);

    /// <summary>
    /// Creates a policy renewal from a policy provided.
    /// </summary>
    Task<QuoteAggregate> RenewPolicyWithoutQuote(
        Guid tenantId,
        Guid quoteAggregateId,
        IPolicyReadModelDetails policy,
        CalculationResult calculationResult,
        FormData finalFormData,
        DateTimeZone timeZone,
        ReleaseContext releaseContext);
}

// <copyright file="AccountingTransactionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services;

using NodaTime;
using System.Threading;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.Quote.Workflow;
using UBind.Domain.Extensions;
using UBind.Domain.Product;
using UBind.Domain.ReferenceNumbers;

public class AccountingTransactionService : IAccountingTransactionService
{
    private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
    private readonly ICreditNoteNumberRepository creditNoteNumberRepository;
    private readonly IInvoiceNumberRepository invoiceNumberRepository;
    private readonly ISystemAlertService systemAlertService;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly IProductFeatureSettingService productFeatureSettingService;
    private readonly IClock clock;

    public AccountingTransactionService(
        IQuoteWorkflowProvider quoteWorklowProvider,
        ICreditNoteNumberRepository creditNoteNumberRepository,
        IInvoiceNumberRepository invoiceNumberRepository,
        ISystemAlertService systemAlertService,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        IProductFeatureSettingService productFeatureSettingService,
        IClock clock)
    {

        this.quoteWorkflowProvider = quoteWorklowProvider;
        this.creditNoteNumberRepository = creditNoteNumberRepository;
        this.invoiceNumberRepository = invoiceNumberRepository;
        this.systemAlertService = systemAlertService;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.productFeatureSettingService = productFeatureSettingService;
        this.clock = clock;
    }

    public async Task<string> IssueCreditNote(ReleaseContext releaseContext, Quote quote, bool fromBind = false, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
        var creditNoteNumber = this.creditNoteNumberRepository.ConsumeAndSave(quote.ProductContext);
        this.systemAlertService.QueueCreditNoteNumberThresholdAlertCheck(
            releaseContext.TenantId, releaseContext.ProductId, releaseContext.Environment);
        quote.Aggregate.IssueCreditNote(
            creditNoteNumber,
            fromBind,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.Now(),
            quoteWorkflow,
            quote.Id);

        return creditNoteNumber;
    }

    public void UnConsumeCreditNoteNumberAndPersist(IProductContext productContext, string creditNoteNumberUsed)
    {
        this.creditNoteNumberRepository.UnconsumeAndSave(productContext, creditNoteNumberUsed);
    }

    public async Task<string> IssueInvoice(ReleaseContext releaseContext, Quote quote, bool fromBind = false, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
        var numberConsumed = this.invoiceNumberRepository.ConsumeAndSave(quote.ProductContext);
        await this.systemAlertService.QueueInvoiceNumberThresholdAlertCheck(
            releaseContext.TenantId, releaseContext.ProductId, releaseContext.Environment);
        quote.Aggregate.IssueInvoice(
            numberConsumed,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.Now(),
            quoteWorkflow,
            quote.Id,
        fromBind);

        return numberConsumed;
    }

    public void UnConsumeInvoiceNumberAndPersist(IProductContext productContext, string invoiceNumberUsed)
    {
        this.invoiceNumberRepository.UnconsumeAndSave(productContext, invoiceNumberUsed);
    }
}

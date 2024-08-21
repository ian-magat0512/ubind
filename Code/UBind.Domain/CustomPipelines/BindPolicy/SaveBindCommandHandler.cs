// <copyright file="SaveBindCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.CustomPipelines.BindPolicy
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Services;

    /// <summary>
    /// Handles the persistence of all the events related to the quote as part
    /// of binding.
    /// </summary>
    /// <typeparam name="TRequest">The request of type <see cref="BindPolicyCommand"/>.</typeparam>
    /// <typeparam name="TResponse">The response of type <see cref="Unit"/>.</typeparam>
    /// <remarks>
    /// Persistence happens only on this part of the bind pipeline. This is to ensure that any retry in case of concurrency
    /// exception will persist all events that should have been made persisted.
    /// </remarks>
    public class SaveBindCommandHandler<TRequest, TResponse>
        : IPipelineBehavior<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>
    {
        private readonly IHttpContextPropertiesResolver contextPropertiesResolver;
        private readonly IQuoteAggregateResolverService quoteAggregateResolver;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
        private readonly IClock clock;
        private readonly IPolicyService policyService;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IPolicyTransactionTimeOfDayScheme limitTimesPolicy;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IAccountingTransactionService accountingTransactionService;

        public SaveBindCommandHandler(
            IHttpContextPropertiesResolver contextPropertiesResolver,
            IQuoteAggregateResolverService quoteAggregateResolver,
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteWorkflowProvider quoteWorkflow,
            IPolicyService policyService,
            IAccountingTransactionService accountingTransactionService,
            IPersonAggregateRepository personAggregateRepository,
            IPolicyTransactionTimeOfDayScheme limitTimesPolicy,
            IProductConfigurationProvider productConfigurationProvider,
            IClock clock)
        {
            this.contextPropertiesResolver = contextPropertiesResolver;
            this.quoteAggregateResolver = quoteAggregateResolver;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.quoteWorkflowProvider = quoteWorkflow;
            this.accountingTransactionService = accountingTransactionService;
            this.policyService = policyService;
            this.personAggregateRepository = personAggregateRepository;
            this.limitTimesPolicy = limitTimesPolicy;
            this.productConfigurationProvider = productConfigurationProvider;
            this.clock = clock;
        }

        public async Task<ValueTuple<NewQuoteReadModel, PolicyReadModel>> Handle(
            BindPolicyCommand command,
            RequestHandlerDelegate<ValueTuple<NewQuoteReadModel, PolicyReadModel>> next,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (command.QuoteId.HasValue)
            {
                var quoteAggregate = command.HaveTriedPersistingCommandBefore
                    ? this.quoteAggregateResolver.GetQuoteAggregateForQuote(command.ReleaseContext.TenantId, command.QuoteId.Value)
                    : command.QuoteAggregate;
                var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId.Value);
                quote.ThrowIfGivenCalculationIdNotMatchingTheLatest(command.BindRequirements?.CalculationResultId);
                var latestCalculationResultId = command.BindRequirements == null
                    ? quote.LatestCalculationResult.Id
                    : command.BindRequirements.CalculationResultId;
                var calculationResult = quote.LatestCalculationResult.Data;
                var formData = quote.LatestFormData.Data;
                var quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(command.ReleaseContext);
                var performingUserId = this.contextPropertiesResolver.PerformingUserId;
                await this.AssignQuoteNumberIfNoneHasBeenAssigned(command.ReleaseContext, quote);

                // persist payment/funding event first
                if (command.AcceptedFundingProposal != null)
                {
                    var fundingProposal = command.AcceptedFundingProposal;
                    quoteAggregate.RecordFundingProposalAccepted(
                        fundingProposal.InternalId,
                        performingUserId,
                        this.clock.Now(),
                        command.QuoteId.Value);
                }
                else if (command.PaymentResult != null)
                {
                    var gatewayResult = command.PaymentResult;
                    if (gatewayResult.Success)
                    {
                        quoteAggregate.RecordPaymentMade(
                            gatewayResult.Reference,
                            gatewayResult.Details,
                            performingUserId,
                            this.clock.Now(),
                            command.QuoteId.Value);
                    }
                    else
                    {
                        quoteAggregate.RecordPaymentFailed(
                            gatewayResult.Errors, performingUserId, this.clock.Now(), command.QuoteId.Value);
                    }

                    if (!gatewayResult.Success)
                    {
                        // Don't continue with rest of process requirement, try to persist here
                        command.HaveTriedPersistingCommandBefore = true;
                        await this.quoteAggregateRepository.Save(quoteAggregate);

                        if (this.contextPropertiesResolver.IsIpAddressWhitelisted)
                        {
                            throw new ErrorException(Errors.Payment.CardPaymentFailed(gatewayResult.Errors, gatewayResult.Details));
                        }
                        else
                        {
                            throw new ErrorException(Errors.Payment.CardPaymentFailed(gatewayResult.Errors));
                        }
                    }
                }
                else
                {
                    bool isPremiumGreaterThanThreshold
                    = quoteWorkflow.PremiumThresholdRequiringSettlement.HasValue
                        && calculationResult.PayablePrice.TotalPayable
                           > quoteWorkflow.PremiumThresholdRequiringSettlement.Value;

                    // no payment nor funding  made. Check that payment is not needed
                    // if needed, and not satisfied, throw error here
                    if (quoteWorkflow.IsSettlementRequired && !isPremiumGreaterThanThreshold)
                    {
                        throw new ErrorException(Errors.Operations.Bind.SettlementRequired());
                    }
                }

                string invoiceNumberUsed = string.Empty;
                string creditNoteNumberUsed = string.Empty;
                string policyNumberUsed = string.Empty;

                try
                {
                    var bindOptions = quote.GetBindingOptions(quoteWorkflow);
                    if (bindOptions.HasFlag(BindOptions.TransactionRecord))
                    {
                        if (calculationResult.PayablePrice.TotalPayable > 0)
                        {
                            invoiceNumberUsed = await this.accountingTransactionService.IssueInvoice(command.ReleaseContext, quote, true, cancellationToken);
                        }
                        else if (calculationResult.PayablePrice.TotalPayable < 0)
                        {
                            creditNoteNumberUsed = await this.accountingTransactionService.IssueCreditNote(command.ReleaseContext, quote, true, cancellationToken);
                        }
                    }

                    if (bindOptions.HasFlag(BindOptions.Policy))
                    {
                        policyNumberUsed = await this.policyService.CompletePolicyTransaction(
                            command.ReleaseContext,
                            quote,
                            latestCalculationResultId,
                            cancellationToken,
                            formData,
                            command.PolicyNumber);
                    }

                    quoteAggregate.RecordQuoteBinding(
                        performingUserId,
                        latestCalculationResultId,
                        this.clock.Now(),
                        quoteWorkflow,
                        quote.Id);

                    // set this to true, so that when a concurrency exception is raised above, retrying this command handler will require
                    // the reloading of the quote aggregate.
                    command.HaveTriedPersistingCommandBefore = true;

                    // CqrsMediator handles retries.
                    await this.quoteAggregateRepository.Save(quoteAggregate);

                    // return the read models
                    return (quote.ReadModel, quoteAggregate?.Policy?.ReadModel);
                }
                catch
                {
                    this.UnconsumeReferenceNumbersUsed(
                        quoteAggregate.ProductContext,
                        policyNumberUsed,
                        invoiceNumberUsed,
                        creditNoteNumberUsed);
                    throw;
                }
            }
            else
            {
                await this.CreatePolicyForFormData(command);
                return (null, command.QuoteAggregate?.Policy?.ReadModel);
            }
        }

        private async Task CreatePolicyForFormData(BindPolicyCommand command)
        {
            PersonalDetails? details = null;
            var productContext = command.ReleaseContext;
            var calculationResult = command.FormModelPolicyCreationRequirement.CalculationResult;
            var customer = command.FormModelPolicyCreationRequirement.Customer;
            if (customer != null)
            {
                PersonAggregate? personAggregate = this.personAggregateRepository.GetById(Guid.Parse(customer.TenantId), Guid.Parse(customer.PrimaryPersonId));
                details = personAggregate != null ? new PersonalDetails(personAggregate) : null;
            }
            var personId = string.IsNullOrEmpty(customer?.PrimaryPersonId) ? (Guid?)null : Guid.Parse(customer.PrimaryPersonId);
            var productConfiguration = await this.productConfigurationProvider.GetProductConfiguration(productContext, WebFormAppType.Quote);
            var dataRetriever = new StandardQuoteDataRetriever(
                productConfiguration,
                command.FormModelPolicyCreationRequirement.FormData,
                calculationResult);
            var quoteTitle = dataRetriever.Retrieve<string>(StandardQuoteDataField.QuoteTitle);
            var inceptionDate = dataRetriever.Retrieve<LocalDate>(StandardQuoteDataField.InceptionDate);
            var expiryDate = dataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate);
            string policyNumber = string.Empty;

            try
            {
                policyNumber = command.FormModelPolicyCreationRequirement.ExternalPolicyNumber ??
                this.policyService.ConsumePolicyNumberAndPersist(productContext);
                var quoteAggregate = QuoteAggregate.CreatePolicy(
                    productContext.TenantId,
                    command.FormModelPolicyCreationRequirement.OrganisationId,
                    productContext.ProductId,
                    productContext.Environment,
                    personId,
                    customer?.Id,
                    details,
                    policyNumber,
                    quoteTitle,
                    inceptionDate,
                    expiryDate,
                    command.FormModelPolicyCreationRequirement.FormData,
                    calculationResult.JObject,
                    command.FormModelPolicyCreationRequirement.IsTestData,
                    command.FormModelPolicyCreationRequirement.TimeZone,
                    this.limitTimesPolicy,
                    this.contextPropertiesResolver.PerformingUserId,
                    this.clock.Now(),
                    productContext.ProductReleaseId);

                await this.quoteAggregateRepository.Save(quoteAggregate);
                command.QuoteAggregate = quoteAggregate;
            }
            catch
            {
                this.policyService.UnConsumePolicyNumberAndPersist(productContext, policyNumber);
                throw;
            }
        }

        /// <summary>
        /// unconsume reference numbers used.
        /// </summary>
        private void UnconsumeReferenceNumbersUsed(IProductContext productContext, string policyNumberUsed, string invoiceNumberUsed, string creditNoteNumberUsed)
        {
            if (!string.IsNullOrEmpty(policyNumberUsed))
            {
                this.policyService.UnConsumePolicyNumberAndPersist(productContext, policyNumberUsed);
            }

            if (!string.IsNullOrEmpty(invoiceNumberUsed))
            {
                this.accountingTransactionService.UnConsumeInvoiceNumberAndPersist(productContext, invoiceNumberUsed);
            }

            if (!string.IsNullOrEmpty(creditNoteNumberUsed))
            {
                this.accountingTransactionService.UnConsumeCreditNoteNumberAndPersist(productContext, creditNoteNumberUsed);
            }
        }

        private async Task AssignQuoteNumberIfNoneHasBeenAssigned(ReleaseContext releaseContext, Quote quote)
        {
            if (quote.QuoteNumber == null)
            {
                string newQuoteNumber = await this.policyService.GenerateQuoteNumber(releaseContext);
                quote.AssignQuoteNumber(
                    newQuoteNumber, this.contextPropertiesResolver.PerformingUserId, this.clock.Now());
            }
        }
    }
}

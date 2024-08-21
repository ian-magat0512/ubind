// <copyright file="QuoteCalculationCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.QuoteCalculation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Queries.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Payment;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;

    /// <summary>
    /// Command handler class for writing investment option inputs.
    /// </summary>
    /// TODO: CAN THIS BE REMOVED?
    public class QuoteCalculationCommandHandler : ICommandHandler<QuoteCalculationCommand, CalculationResponseModel>
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly ICqrsMediator mediator;
        private readonly IClock clock;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IPaymentService paymentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteCalculationCommandHandler"/> class.
        /// </summary>
        /// <param name="quoteAggregateRepository">The quote aggregate repository.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="clock">A clock for obtaining the current time.</param>
        public QuoteCalculationCommandHandler(
            IQuoteAggregateRepository quoteAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator,
            IClock clock,
            IPaymentService paymentService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.cachingResolver = cachingResolver;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.mediator = mediator;
            this.clock = clock;
            this.httpContextAccessor = httpContextAccessor;
            this.paymentService = paymentService;
        }

        /// <inheritdoc/>
        public async Task<CalculationResponseModel> Handle(QuoteCalculationCommand request, CancellationToken cancellationToken)
        {
            CalculationResult calculationResult;
            cancellationToken.ThrowIfCancellationRequested();

            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(request.ProductContext.TenantId);
            var isMutual = TenantHelper.IsMutual(tenantAlias);
            var quoteDataRetriever = new StandardQuoteDataRetriever(request.ProductConfiguration, request.FinalFormData, request.CalculationResultData);

            var allowRefund = true;
            if (request.QuoteType == QuoteType.Cancellation || request.QuoteType == QuoteType.Adjustment)
            {
                var command = new IsRefundAllowedQuery(request.Policy, quoteDataRetriever, request.ReleaseContext);
                allowRefund = await this.mediator.Send(command);
            }

            calculationResult = this.GenerateCalculationResult(
                request,
                request.CalculationResultData,
                quoteDataRetriever,
                allowRefund);

            // We are currently calculating merchant fees for single one time payment only.
            // This has to be revised when direct monthly payments (not through premium funding provider) is to be supported
            // Alternatively, a configuration option for payment should be added ('addMerchantFees').
            if (request.PaymentData != null
                && request.PaymentData.SinglePayment
                && await this.paymentService.CanCalculateMerchantFees(request.ReleaseContext))
            {
                if (calculationResult.PayablePrice.TotalPayable > 0m)
                {
                    await this.UpdateCalculationWithNewMerchantFees(
                        calculationResult,
                        request.PaymentData,
                        request.ReleaseContext);
                }
            }

            // record the new calculation result
            if (request.Quote != null && request.PersistResults)
            {
                calculationResult.FormDataId = request.FormDataUpdateId;
                request.Quote.RecordCalculationResult(
                        calculationResult,
                        request.CalculationResultData,
                        this.clock.Now(),
                        request.ProductConfiguration.FormDataSchema,
                        isMutual,
                        this.httpContextPropertiesResolver.PerformingUserId);

                // When Funding Proposal is null, there should be no FundingProposalCreatedEvent
                if (request.HasFundingProposal && request.FundingProposal != null)
                {
                    request.Quote.RecordFundingProposalCreated(
                        request.FundingProposal, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), request.Quote.Id);
                }

                // save the changes
                await this.quoteAggregateRepository.Save(request.Quote.Aggregate);
            }

            // Set a http response header so we know which release was used for this calculation
            Guid releaseId = request.ReleaseCalculationOutput.ReleaseId;
            this.httpContextAccessor.HttpContext?.Response.Headers.Add("X-Product-Release-Id", releaseId.ToString());

            return new CalculationResponseModel(calculationResult, request.Quote?.ReadModel, request.FundingProposal);
        }

        /// <summary>
        /// Generates a new calculation result.
        /// </summary>
        /// <param name="calculationResultData">The calculation result json.</param>
        /// <param name="quoteDataRetriever">A helper for finding data in the form model and calculation result.</param>
        /// <param name="provideRefund">indicator if refund will be provided.</param>
        private CalculationResult GenerateCalculationResult(
            QuoteCalculationCommand request,
            CachingJObjectWrapper calculationResultData,
            StandardQuoteDataRetriever quoteDataRetriever,
            bool provideRefund = true)
        {
            CalculationResult newCalculationResult;
            switch (request.QuoteType)
            {
                case QuoteType.NewBusiness:
                    newCalculationResult = CalculationResult.CreateForNewPolicy(calculationResultData, quoteDataRetriever);
                    break;
                case QuoteType.Renewal:
                    newCalculationResult = CalculationResult.CreateForRenewal(
                        calculationResultData,
                        quoteDataRetriever,
                        request.Policy.ExpiryDateTime.Value.Date);
                    break;
                case QuoteType.Adjustment:
                    newCalculationResult = CalculationResult.CreateForAdjustment(
                            calculationResultData,
                            quoteDataRetriever,
                            request.Policy.DataSnapshot.CalculationResult.Data.CompoundPrice,
                            provideRefund,
                            request.Policy.LatestPolicyPeriodStartDateTime.Date,
                            request.Policy.ExpiryDateTime.Value.Date);
                    break;
                case QuoteType.Cancellation:
                    newCalculationResult = CalculationResult.CreateForCancellation(
                            calculationResultData,
                            quoteDataRetriever,
                            request.Policy.DataSnapshot.CalculationResult.Data.CompoundPrice,
                            provideRefund,
                            request.Policy.LatestPolicyPeriodStartDateTime.Date,
                            request.Policy.ExpiryDateTime.Value.Date);
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected Quote Type \"{request.QuoteType.Humanize()}\" when trying to generate a quote calculation");
            }

            return newCalculationResult;
        }

        private async Task UpdateCalculationWithNewMerchantFees(
            CalculationResult calculationResult,
            PaymentData paymentData,
            ReleaseContext releaseContext)
        {
            if (paymentData == null || paymentData.CardBin == null)
            {
                return;
            }

            if (calculationResult.JObject["payment"] is not JObject payment)
            {
                return;
            }

            var priceBreakdown = calculationResult.PayablePrice;
            decimal originalTotalPayable = priceBreakdown.TotalPayable;
            decimal amount = priceBreakdown.TotalPayable - (priceBreakdown.MerchantFees + priceBreakdown.MerchantFeesGst);
            MerchantFees merchantFees = await this.paymentService.CalculateMerchantFees(
                releaseContext,
                amount,
                priceBreakdown.CurrencyCode,
                paymentData);

            priceBreakdown.MerchantFeesGst = (merchantFees.Total * (1m / 11m)).FloorToDecimalPlace();
            priceBreakdown.MerchantFees = merchantFees.Total - priceBreakdown.MerchantFeesGst;
            if (priceBreakdown.TotalPayable == originalTotalPayable)
            {
                // nothing changed, so don't bother updating the other places.
                return;
            }

            if (payment["instalments"] is JObject instalments)
            {
                instalments["instalmentAmount"] = priceBreakdown.TotalPayable.ToString();
            }

            if (payment["outputVersion"] == null)
            {
                // assume V1
                payment["total"]["merchantFees"] = (priceBreakdown.MerchantFees + priceBreakdown.MerchantFeesGst).ToString();
                payment["total"]["payable"] = priceBreakdown.TotalPayable.ToString();

                calculationResult.InvalidateJson();
            }
            else if (payment["outputVersion"].ToString() == "2")
            {
                payment["priceComponents"]["merchantFees"] = priceBreakdown.MerchantFees.ToString();
                payment["priceComponents"]["merchantFeesGST"] = priceBreakdown.MerchantFeesGst.ToString();
                payment["priceComponents"]["totalGST"] = priceBreakdown.TotalGst.ToString();
                payment["priceComponents"]["totalPayable"] = priceBreakdown.TotalPayable.ToString();
                calculationResult.InvalidateJson();
            }
        }
    }
}

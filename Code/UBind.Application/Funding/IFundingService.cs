// <copyright file="IFundingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Json;
    using UBind.Domain.Payment;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Service for creating and accepting premium funding proposals.
    /// </summary>
    public interface IFundingService
    {
        /// <summary>
        /// Gets a value indicating whether the service supports proposal creation for pricing as part of a quote.
        /// </summary>
        bool PricingSupported { get; }

        /// <summary>
        /// Gets a value indicating whether the service supports direct debit.
        /// </summary>
        bool DirectDebitSupported { get; }

        /// <summary>
        /// Gets a value indicating whether the service supports credit cards.
        /// </summary>
        bool CreditCardSupported { get; }

        /// <summary>
        /// Gets a value indicating whether proposals can be accepted without redirecting to service site.
        /// </summary>
        bool CanAcceptWithoutRedirect { get; }

        /// <summary>
        /// Gets a value indicating whether acceptance of the funding proposal is to be done via API.
        /// This will be false for those where a funding iframe is used, because the front end sends
        /// the acceptance directly to the funding provider.
        /// </summary>
        bool AcceptancePerformedViaApi { get; }

        /// <summary>
        /// Create a funding proposal.
        /// </summary>
        /// <param name="formData">The form data json.</param>
        /// <param name="calculationResultData">The calculation json.</param>
        /// <param name="priceBreakdown">The price breakdown.</param>
        /// <param name="quote">The quote, or null if this is being done independently of a quote.</param>
        /// <returns>A task whose result contains a funding proposal when successful, or error messages when unsuccessful.</returns>
        Task<FundingProposal?> CreateFundingProposal(
            IProductContext productContext,
            CachingJObjectWrapper formData,
            CachingJObjectWrapper calculationResultData,
            PriceBreakdown priceBreakdown,
            Domain.Aggregates.Quote.Quote? quote,
            PaymentData? paymentData,
            bool isTestData,
            CancellationToken cancellationToken);

        /// <summary>
        /// Update a funding proposal.
        /// </summary>
        /// <param name="formData">The form data json.</param>
        /// <param name="calculationResultData">The calculation json.</param>
        /// <param name="priceBreakdown">The price breakdown.</param>
        /// <param name="quote">The quote, or null if this is being done independently of a quote.</param>
        /// <returns>A task whose result contains a funding proposal when successful, or error messages when unsuccessful.</returns>
        Task<FundingProposal?> UpdateFundingProposal(
            IProductContext productContext,
            string? providerContractId,
            CachingJObjectWrapper formData,
            CachingJObjectWrapper calculationResultData,
            PriceBreakdown priceBreakdown,
            Domain.Aggregates.Quote.Quote? quote,
            PaymentData? paymentData,
            bool isTestData,
            CancellationToken cancellationToken);

        /// <summary>
        /// Accept a funding proposal with payment.
        /// </summary>
        /// <param name="quote">The quote which the funding proposal is for.</param>
        /// <param name="fundingProposalId">The ID of the funding proposal to accept.</param>
        /// <param name="paymentMethodDetails">They payment details.</param>
        /// <returns>The funding proposal.</returns>
        Task<FundingProposal> AcceptFundingProposal(
            Domain.Aggregates.Quote.Quote quote,
            Guid fundingProposalId,
            IPaymentMethodDetails paymentMethodDetails,
            bool isTestData,
            CancellationToken cancellationToken);

        /// <summary>
        /// Accept a funding proposal without supplying payment.
        /// </summary>
        /// <param name="quote">The quote which the funding proposal is for.</param>
        /// <param name="fundingProposalId">The ID of the funding proposal to accept.</param>
        /// <returns>The funding proposal.</returns>
        Task<FundingProposal> AcceptFundingProposal(
            Domain.Aggregates.Quote.Quote quote,
            Guid fundingProposalId,
            bool isTestData);
    }
}

// <copyright file="CalculationResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.ReadWriteModel.CalculationTrigger;

    /// <summary>
    /// Representation of a calculation result.
    /// </summary>
    public class CalculationResult : CachingJObjectWrapper
    {
        /// <summary>
        /// The string representation of the state which a quote must be in for it to be bound.
        /// </summary>
        public const string BindableState = "bindingQuote";

        private const string ResultState = "state";
        private readonly IEnumerable<ICalculationTrigger> triggers = new List<ICalculationTrigger>
        {
            new SoftReferralCalculationTrigger(),
            new EndorsementCalculationTrigger(),
            new HardReferralCalculationTrigger(),
            new DeclinedCalculationTrigger(),
            new ErrorCalculationTrigger(),
            new ReviewCalculationTrigger(),
        };

        [System.Text.Json.Serialization.JsonConstructor]
        public CalculationResult(
            Guid? formDataId,
            IntervalPrice applicablePrice,
            PriceBreakdown parentRefund,
            CompoundPrice compoundPrice,
            PriceBreakdown payablePrice,
            JObject calculationResultModel)
        {
            this.FormDataId = formDataId;
            this.ApplicablePrice = applicablePrice;
            this.ParentRefund = parentRefund;
            this.CompoundPrice = compoundPrice;
            this.PayablePrice = payablePrice;
            this.internalJObject = calculationResultModel;
        }

        private CalculationResult(QuoteAggregate.AggregateCreationFromPolicyEvent policyIssuedEvent)
        {
            this.CalculateForNewPolicyPeriod(
                policyIssuedEvent.DataSnapshot.CalculationResult.Data,
                policyIssuedEvent.InceptionDateTime.Date,
                policyIssuedEvent.ExpiryDateTime.Value.Date);
        }

        private CalculationResult(IPolicyCreatedEvent policyCreatedEvent)
        {
            this.CalculateForNewPolicyPeriod(
                policyCreatedEvent.DataSnapshot.CalculationResult.Data,
                policyCreatedEvent.InceptionDateTime.Date,
                policyCreatedEvent.ExpiryDateTime.Value.Date);
        }

        [JsonConstructor]
        private CalculationResult()
        {
        }

        /// <summary>
        /// Gets or sets the ID of the form data record used in the calculation.
        /// </summary>
        [JsonProperty]
        public Guid? FormDataId { get; set; }

        /// <summary>
        /// Gets the price applicable for the current quote's effective period.
        /// </summary>
        [JsonProperty]
        public IntervalPrice ApplicablePrice { get; private set; }

        /// <summary>
        /// Gets the annualized price based on the latest applicable price.
        /// Note: this null checking was needed for the migration UB-4617 to proceed because some prod records dont have this.
        /// this is triggered when getting an aggregate by id, but the production data have this as null,
        /// maybe we can remove this null handling after the migration has happened.
        /// </summary>
        public PriceBreakdown AnnualizedPrice => this.ApplicablePrice?.Annualize();

        /// <summary>
        /// Gets the price applicable for the current quote's effective period
        /// (not accounting for the refundable amount of the previous price).
        /// </summary>
        [JsonProperty]
        public PriceBreakdown ParentRefund { get; private set; }

        /// <summary>
        /// Gets the compound price, recording the various prices applicable to the policy and its parents over its lifetime.
        /// </summary>
        [JsonProperty]
        public CompoundPrice CompoundPrice { get; private set; }

        /// <summary>
        /// Gets the net amount payable for the current quote.
        /// </summary>
        [JsonProperty]
        public PriceBreakdown PayablePrice { get; private set; }

        /// <summary>
        /// Gets the amount refundable as a positive amount (if the amount payable is a refund).
        /// </summary>
        public PriceBreakdown RefundBreakdown
        {
            get
            {
                PriceBreakdown refundBreakdown = null;

                // Note: this null checking was needed for the migration UB-4617 to proceed because some prod records dont have this.
                // this is triggered when getting an aggregate by id, but the production data have this as null,
                // maybe we can remove this null handling after the migration has happened.
                if (this.PayablePrice != null)
                {
                    refundBreakdown = this.PayablePrice.TotalPayable < 0
                        ? this.PayablePrice * -1
                        : PriceBreakdown.Zero(this.PayablePrice.CurrencyCode);
                }

                return refundBreakdown;
            }
        }

        /// <summary>
        /// Gets the state of the calculation result.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string CalculationResultState
        {
            get
            {
                return this.JObject[ResultState].ToString();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the calculation result's state is bindable.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsBindable
        {
            get
            {
                return this.CalculationResultState.Equals(BindableState);
            }
        }

        /// <summary>
        /// Gets a collection of all the triggers in the calculation result.
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IEnumerable<ICalculationTrigger> Triggers
        {
            get
            {
                return this.GetCalculationTriggers();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the calculation has <see cref="SoftReferralCalculationTrigger"/>.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasSoftReferralTriggers
        {
            get
            {
                return this.GetCalculationTriggers().Any(x => x.GetType().Name == typeof(SoftReferralCalculationTrigger).Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the calculation has <see cref="EndorsementCalculationTrigger"/>.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasEndorsementTriggers
        {
            get
            {
                return this.GetCalculationTriggers().Any(x => x.GetType().Name == typeof(EndorsementCalculationTrigger).Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the calculation has <see cref="HardReferralCalculationTrigger"/>.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasHardReferralTriggers
        {
            get
            {
                return this.GetCalculationTriggers().Any(x => x.GetType().Name == typeof(HardReferralCalculationTrigger).Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the calculation has <see cref="DeclinedCalculationTrigger"/>.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasDeclinedReferralTriggers
        {
            get
            {
                return this.GetCalculationTriggers().Any(x => x.GetType().Name == typeof(DeclinedCalculationTrigger).Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the calculation has <see cref="ErrorCalculationTrigger"/>.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasErrorCalculationTriggers
        {
            get
            {
                return this.GetCalculationTriggers().Any(x => x.GetType().Name == typeof(ErrorCalculationTrigger).Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the calculation has <see cref="ReviewCalculationTrigger"/>.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasReviewCalculationTriggers
        {
            get
            {
                return this.GetCalculationTriggers().Any(x => x.GetType().Name == typeof(ReviewCalculationTrigger).Name);
            }
        }

        public JObject CalculationResultModel => this.JObject;

        /// <summary>
        /// Create a calculation result for a new policy period (e.g. a new business transaction, or a renewal).
        /// </summary>
        /// <param name="calculationResultData">The calculation workbook output.</param>
        /// <param name="quoteData">The quote data.</param>
        /// <returns>A new instance of <see cref="CalculationResult"/> with pricing details for a new policy period.</returns>
        public static CalculationResult CreateForNewPolicy(
            CachingJObjectWrapper calculationResultData,
            StandardQuoteDataRetriever quoteData)
        {
            var calculationResult = new CalculationResult();
            var inceptionDate = quoteData.Retrieve<LocalDate?>(StandardQuoteDataField.InceptionDate)
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "policy inception date")));
            var expiryDate = quoteData.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate)
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "policy expiry date")));

            if (inceptionDate > expiryDate)
            {
                throw new ErrorException(Errors.Calculation.PolicyPeriodStartDateMustNotBeAfterEndDate());
            }

            calculationResult.CalculateForNewPolicyPeriod(calculationResultData, inceptionDate, expiryDate);
            return calculationResult;
        }

        /// <summary>
        /// Create a calculation result for a claim.
        /// </summary>
        /// <param name="calculationResultData">The calculation workbook output.</param>
        /// <param name="inceptionDate">The inception date of the calculation result.</param>
        /// <param name="expiryDate">The expiry date of the calculation result.</param>
        /// <returns>A new instance of <see cref="CalculationResult"/> with pricing details for a new policy period.</returns>
        public static CalculationResult CreateForClaim(
            CachingJObjectWrapper calculationResultData, LocalDate inceptionDate, LocalDate expiryDate)
        {
            var calculationResult = new CalculationResult();

            // TODO: Handle claim calculations properly.
            calculationResult.CalculateForNewPolicyPeriod(calculationResultData, inceptionDate, expiryDate);
            return calculationResult;
        }

        public static CalculationResult CreateForRenewal(
            CachingJObjectWrapper calculationResultData,
            StandardQuoteDataRetriever quoteData,
            LocalDate currentPolicyPeriodEndDate)
        {
            var calculationResult = new CalculationResult();
            var newPolicyPeriodEndDate = quoteData.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate)
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "policy expiry date")));

            if (newPolicyPeriodEndDate < currentPolicyPeriodEndDate)
            {
                throw new ErrorException(Errors.Calculation.RenewalExpiryDateCannotBeBeforePolicyPeriodStartDate());
            }

            calculationResult.CalculateForNewPolicyPeriod(calculationResultData, currentPolicyPeriodEndDate, newPolicyPeriodEndDate);
            return calculationResult;
        }

        /// <summary>
        /// Create a calculation result for an adjusted or cancellation policy.
        /// </summary>
        /// <param name="calculationResultData">The calculation workbook output.</param>
        /// <param name="quoteData">The quote data.</param>
        /// <param name="parentCompoundPrice">The compound price being adjusted.</param>
        /// <returns>A new instance of <see cref="CalculationResult"/> with pricing details for a new policy period.</returns>
        public static CalculationResult CreateForCancellation(
            CachingJObjectWrapper calculationResultData,
            StandardQuoteDataRetriever quoteData,
            CompoundPrice parentCompoundPrice,
            bool allowRefund,
            LocalDate currentPolicyPeriodStartDate,
            LocalDate currentPolicyPeriodEndDate)
        {
            var calculationResult = new CalculationResult();
            var cancellationDateOrNull = quoteData.Retrieve<LocalDate?>(StandardQuoteDataField.CancellationEffectiveDate);
            if (cancellationDateOrNull == null)
            {
                throw new ErrorException(Errors.Calculation.UnableToCalculateAPriceOrRefundBecauseCancellationDateIsEmpty());
            }

            var cancellationDate = cancellationDateOrNull
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "cancellation date")));
            if (cancellationDate < currentPolicyPeriodStartDate || cancellationDate > currentPolicyPeriodEndDate)
            {
                throw new ErrorException(Errors.Calculation.CancellationDateOutsidePolicyPeriod());
            }

            calculationResult.CalculateForCancellation(
                calculationResultData,
                currentPolicyPeriodStartDate,
                currentPolicyPeriodEndDate,
                cancellationDate,
                parentCompoundPrice,
                allowRefund);
            return calculationResult;
        }

        /// <summary>
        /// Create a calculation result for an adjusted or cancellation policy.
        /// </summary>
        /// <param name="calculationResultData">The calculation workbook output.</param>
        /// <param name="quoteData">The quote data.</param>
        /// <param name="parentCompoundPrice">The compound price being adjusted.</param>
        /// <returns>A new instance of <see cref="CalculationResult"/> with pricing details for a new policy period.</returns>
        public static CalculationResult CreateForAdjustment(
            CachingJObjectWrapper calculationResultData,
            StandardQuoteDataRetriever quoteData,
            CompoundPrice parentCompoundPrice,
            bool allowRefund,
            LocalDate currentPolicyPeriodStartDate,
            LocalDate currentPolicyPeriodEndDate)
        {
            var calculationResult = new CalculationResult();
            var adjustmentEffectiveDate = quoteData.Retrieve<LocalDate?>(StandardQuoteDataField.EffectiveDate)
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "effective date")));

            var newEndDate = quoteData.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate);
            var policyExpiryDate = newEndDate ?? currentPolicyPeriodEndDate;

            if (adjustmentEffectiveDate < currentPolicyPeriodStartDate || adjustmentEffectiveDate > policyExpiryDate)
            {
                throw new ErrorException(Errors.Calculation.AdjustmentDateOutsidePolicyPeriod());
            }

            if (policyExpiryDate < currentPolicyPeriodStartDate)
            {
                throw new ErrorException(Errors.Calculation.AdjustmentExpiryDateCannotBeBeforePolicyPeriodStartDate());
            }

            calculationResult.CalculateForAdjustment(
                calculationResultData,
                currentPolicyPeriodStartDate,
                policyExpiryDate,
                adjustmentEffectiveDate,
                parentCompoundPrice,
                allowRefund);
            return calculationResult;
        }

        /// <summary>
        /// For generating calculation result for policies that were issued before calculation result
        /// was stored as part of policy issued event.
        /// </summary>
        /// <param name="policyCreatedEvent">The policy imported event.</param>
        /// <returns>A new instance of <see cref="CalculationResult"/> with pricing details from the policy.</returns>
        public static CalculationResult CreateFromExistingPolicy(IPolicyCreatedEvent policyCreatedEvent)
        {
            return new CalculationResult(policyCreatedEvent);
        }

        /// <summary>
        /// For generating calculation result for policies that were issued before calculation result
        /// was stored as part of policy issued event.
        /// </summary>
        /// <param name="policyImportedEvent">The policy imported event.</param>
        /// <returns>A new instance of <see cref="CalculationResult"/> with pricing details from the policy.</returns>
        public static CalculationResult CreateFromExistingPolicy(QuoteAggregate.AggregateCreationFromPolicyEvent policyImportedEvent)
        {
            return new CalculationResult(policyImportedEvent);
        }

        /// <summary>
        /// Calculate a refund for the price.
        /// </summary>
        /// <param name="refundDate">The date the refund will be calculated for.</param>
        /// <returns>A price breakdown of the refund.</returns>
        public PriceBreakdown CalculateRefund(LocalDate refundDate)
        {
            return this.CompoundPrice.CalculateRefund(refundDate);
        }

        /// <summary>
        /// Returns a value indicating whether the calculation result json contains a given property.
        /// </summary>
        /// <param name="path">The json path specifying the property.</param>
        /// <returns><c>true</c> if the calculation result contains the given property, otherwise <c>false</c>.</returns>
        public bool HasProperty(string path)
        {
            return this.JObject.SelectTokens(path).Any();
        }

        /// <summary>
        /// Returns a value indicating whether the calculation result contains a given property and the property has data.
        /// </summary>
        /// <param name="path">The json path specifying the property.</param>
        /// <returns><c>true</c> if the calculation result contains the given propertyand the property has data, otherwise <c>false</c>.</returns>
        public bool PropertyIsNullOrEmpty(string path)
        {
            return this.JObject.SelectToken(path).IsNullOrEmpty();
        }

        /// <summary>
        /// Determines if a given property can be patched according to given rules.
        /// </summary>
        /// <param name="path">The path specifying the property.</param>
        /// <param name="rules">Rules for determining if the property can be patched.</param>
        /// <returns>A result indicating whether the property can be patched, or the reason why not.</returns>
        public Result CanPatchProperty(JsonPath path, PatchRules rules)
        {
            return this.JObject.CanPatchProperty(path, rules);
        }

        /// <summary>
        /// Patch a given property in the calculation result json.
        /// </summary>
        /// <param name="path">The path of the property to patch.</param>
        /// <param name="value">The new value for the property.</param>
        public void PatchProperty(JsonPath path, JToken value)
        {
            this.JObject.PatchProperty(path, value);
            this.internalJson = null;
        }

        /// <summary>
        /// Get the merchant fee rate.
        /// </summary>
        /// <returns>Return merchant fee rate.</returns>
        private static decimal GetMerchantFeeRate(PriceBreakdown priceBreakdown)
        {
            // Hack until payment charges are moved out of calculation result properly.
            var merchantFeeRate = priceBreakdown.TotalExcludingPaymentCharges == 0
                ? 0 : (priceBreakdown.MerchantFees + priceBreakdown.MerchantFeesGst) / priceBreakdown.TotalExcludingPaymentCharges;
            return merchantFeeRate;
        }

        private static IntervalPrice GetPriceForPolicyPeriod(PriceBreakdown priceBreakdown, LocalDate startDate, LocalDate endDate)
        {
            var priceComponentFilter = PriceComponentFilter.DefaultFixedComponentFilter;
            var breakdownExcludingPaymentCharges = priceBreakdown.Filter(PriceComponentFilter.ExcludePaymentFees);
            var fixedAndScalablePrice = new FixedAndScalablePrice(breakdownExcludingPaymentCharges, priceComponentFilter);

            // The price so far is an annualised price, so let's get the price for the specified policy period.
            // It might be 12 months, in which case there'll be no difference, but if not then this will give the price
            // for the specified policy period.
            return new IntervalPrice(
                fixedAndScalablePrice,
                startDate,
                endDate);
        }

        /// <summary>
        /// This is not a great way to calculate payment charges, because we are trying to prorate merchant fees
        /// which would likely introduce rounding errors.
        /// This should probably be removed, and instead we add merchant fees after on the final amount each time.
        /// The reason it's done this way is that sometimes merchant fees come from the workbook, on the original
        /// full period calculation, which is obviously a bad idea.
        /// </summary>
        private static PriceBreakdown GetPaymentCharges(
            PriceBreakdown originalPriceBreakdown,
            PriceBreakdown payablePriceExcludingPaymentCharge,
            bool allowRefund,
            bool calculateGst)
        {
            var gstRate = 0.1m;
            var merchantFeeRate = GetMerchantFeeRate(originalPriceBreakdown);
            var transactionCosts = originalPriceBreakdown.TransactionCosts;
            var paymentCharges = PriceBreakdown.Zero(originalPriceBreakdown.CurrencyCode);
            if (payablePriceExcludingPaymentCharge.TotalPayable > 0m)
            {
                var merchantFeesTotal = (payablePriceExcludingPaymentCharge.TotalExcludingPaymentCharges * merchantFeeRate).FloorToDecimalPlace();
                var merchantFeesGst = calculateGst
                    ? (merchantFeesTotal - (merchantFeesTotal / (1 + gstRate))).FloorToDecimalPlace()
                    : 0m;
                var merchantFees = calculateGst
                    ? merchantFeesTotal - merchantFeesGst
                    : merchantFeesTotal;

                paymentCharges = PriceBreakdown.CreateForPaymentCharges(
                    merchantFees,
                    merchantFeesGst,
                    transactionCosts,
                    calculateGst ? originalPriceBreakdown.TransactionCostsGst : 0m,
                    originalPriceBreakdown.CurrencyCode);
            }
            else
            {
                if (!allowRefund)
                {
                    var merchantFeesTotal = ((payablePriceExcludingPaymentCharge.TotalExcludingPaymentCharges * -1) * merchantFeeRate).FloorToDecimalPlace();
                    var merchantFeesGst = calculateGst
                        ? (merchantFeesTotal - (merchantFeesTotal / (1 + gstRate))).FloorToDecimalPlace()
                        : 0;
                    var merchantFees = calculateGst
                        ? merchantFeesTotal - merchantFeesGst
                        : merchantFeesTotal;

                    paymentCharges = PriceBreakdown.CreateForPaymentCharges(
                        merchantFees,
                        merchantFeesGst,
                        transactionCosts,
                        calculateGst ? originalPriceBreakdown.TransactionCostsGst : 0m,
                        originalPriceBreakdown.CurrencyCode);
                }
            }

            return paymentCharges;
        }

        private void CalculateForAdjustment(
            CachingJObjectWrapper jObject,
            LocalDate startDate,
            LocalDate expiryDate,
            LocalDate adjustmentEffectiveDate,
            CompoundPrice parentCompoundPrice,
            bool allowRefund)
        {
            // Using the new calculations, get the price from the adjustment date to the policy end date
            this.JObject = jObject.JObject;
            PriceBreakdown priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(jObject);
            var fullPolicyPeriodPrice = GetPriceForPolicyPeriod(priceBreakdown, startDate, expiryDate);
            this.ApplicablePrice = fullPolicyPeriodPrice.CalculateProRataPrice(adjustmentEffectiveDate, fullPolicyPeriodPrice.EndDate);

            // take out any payment charges
            var payablePriceExcludingPaymentCharge = this.ApplicablePrice.FixedAndScalablePrice.FixedComponents
                + this.ApplicablePrice.FixedAndScalablePrice.ScalableComponents;

            // work out amount to be credited
            this.ParentRefund = parentCompoundPrice.CalculateRefund(adjustmentEffectiveDate);

            // offset that with the payable amount for the remainder of the policy period
            payablePriceExcludingPaymentCharge = payablePriceExcludingPaymentCharge - this.ParentRefund;

            // set the payable price to 0 if refunds are not allowed and it's a negative amount
            if (payablePriceExcludingPaymentCharge.TotalPayable <= 0m && !allowRefund)
            {
                payablePriceExcludingPaymentCharge = PriceBreakdown.Zero(priceBreakdown.CurrencyCode);
            }

            // Add in payment charges
            this.PayablePrice = payablePriceExcludingPaymentCharge;
            if (this.PayablePrice.TotalPayable > 0m)
            {
                // calculate payment charges on the new payable amount
                // (note this should be removed and is only temporary because payment charges are being
                // calculated in the workbook).
                var paymentCharges = GetPaymentCharges(priceBreakdown, payablePriceExcludingPaymentCharge, allowRefund, priceBreakdown.MerchantFeesGst > 0);
                this.PayablePrice = payablePriceExcludingPaymentCharge + paymentCharges;
            }

            // Update the price history with the adjusted price
            this.CompoundPrice = parentCompoundPrice.Update(this.ApplicablePrice);
        }

        /// <summary>
        /// When we calculate a refund, we still need a new calculation, because fees could be chargeable when you cancel,
        /// e.g. a cancellation fee or broker fee. So we take the calculations and grab the fees from them only.
        /// Then we prorata the premium for the previously calculated policy period to calculation the refund amount.
        /// </summary>
        private void CalculateForCancellation(
            CachingJObjectWrapper jObject,
            LocalDate inceptionDate,
            LocalDate expiryDate,
            LocalDate cancellationEffectiveDate,
            CompoundPrice parentCompoundPrice,
            bool allowRefund)
        {
            // Get cancellation fees from the calculation JObject. Note that we do prorating on the prices, however
            // it's likely that has no effect since fees are not prorated, since they are normally not scalable components.
            // Fixed components don't change during prorata calculations.
            this.JObject = jObject.JObject;
            PriceBreakdown priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(jObject);
            var fullPolicyPeriodPrice = GetPriceForPolicyPeriod(priceBreakdown, inceptionDate, expiryDate);
            this.ApplicablePrice = fullPolicyPeriodPrice.CalculateProRataPrice(cancellationEffectiveDate, fullPolicyPeriodPrice.EndDate);

            // here we take out any payment charges from the calculation of cancellation fees.
            // We'll add payment charges in again once we know the total payable.
            var payablePriceExcludingPaymentCharge = this.ApplicablePrice.FixedAndScalablePrice.FixedComponents
                + this.ApplicablePrice.FixedAndScalablePrice.ScalableComponents;

            // now we calculate the proportion of actual premium that should be refunded due to the early end to the policy
            this.ParentRefund = parentCompoundPrice.CalculateRefund(cancellationEffectiveDate);

            // add the cancellation fees to the refund amount
            payablePriceExcludingPaymentCharge = payablePriceExcludingPaymentCharge - this.ParentRefund;

            // set the payable price to 0 if refunds are not allowed and it's a negative amount
            if (payablePriceExcludingPaymentCharge.TotalPayable <= 0m && !allowRefund)
            {
                payablePriceExcludingPaymentCharge = PriceBreakdown.Zero(priceBreakdown.CurrencyCode);
            }

            this.PayablePrice = payablePriceExcludingPaymentCharge;
            if (payablePriceExcludingPaymentCharge.TotalPayable > 0m)
            {
                // calculate payment charges on the new payable amount
                // (note this should be removed and is only temporary because payment charges are being
                // calculated in the workbook).
                var paymentCharges = GetPaymentCharges(priceBreakdown, payablePriceExcludingPaymentCharge, allowRefund, priceBreakdown.MerchantFeesGst > 0);
                this.PayablePrice = payablePriceExcludingPaymentCharge + paymentCharges;
            }

            // Update the price history with the adjusted price
            this.CompoundPrice = parentCompoundPrice.Update(this.ApplicablePrice);
        }

        private void CalculateForNewPolicyPeriod(
            CachingJObjectWrapper jObject,
            LocalDate inceptionDate,
            LocalDate expiryDate)
        {
            this.JObject = jObject.JObject;
            PriceBreakdown priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(jObject);
            var priceComponentFilter = PriceComponentFilter.DefaultFixedComponentFilter;
            var fixedAndScalablePrice = new FixedAndScalablePrice(priceBreakdown, priceComponentFilter);
            var intervalPrice = new IntervalPrice(
                fixedAndScalablePrice,
                inceptionDate,
                expiryDate);
            this.ParentRefund = null;
            this.ApplicablePrice = intervalPrice;
            this.PayablePrice = priceBreakdown;
            this.CompoundPrice = new CompoundPrice(intervalPrice);
        }

        /// <summary>
        /// Gets a calculation trigger value.
        /// </summary>
        /// <returns>Returns an object of ICalculationTrigger interface.</returns>
        private IEnumerable<ICalculationTrigger> GetCalculationTriggers()
        {
            List<ICalculationTrigger> calculationTrigger = new List<ICalculationTrigger>();

            var model = JsonConvert.DeserializeObject<CalculationDataTriggerModel>(this.Json) ?? null;
            if (model?.Triggers != null)
            {
                void GetCalculationTrigger()
                {
                    foreach (var key1 in model.Triggers.Keys)
                    {
                        var value = model.Triggers[key1];
                        foreach (var key2 in value.Keys)
                        {
                            if (value[key2])
                            {
                                calculationTrigger.Add(this.triggers.FirstOrDefault(t => t.Name == key1));
                                ////return;
                            }
                        }
                    }
                }

                GetCalculationTrigger();
            }

            return calculationTrigger;
        }
    }
}

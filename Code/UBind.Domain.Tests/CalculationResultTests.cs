// <copyright file="CalculationResultTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class CalculationResultTests
    {
        [Fact]
        public void CreateForNewPolicyPeriod_CreatesPayableAmountWithCorrectBasePremiumComponent()
        {
            // Arrange
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            var calculationResultData = new CachingJObjectWrapper(this.GetSampleJson());
            var quoteData = QuoteFactory.QuoteDataRetriever(new CachingJObjectWrapper(formDataJson), calculationResultData);

            // Act
            var sut = Domain.ReadWriteModel.CalculationResult.CreateForNewPolicy(calculationResultData, quoteData);

            // Assert
            Assert.Equal(82.64m, sut.PayablePrice.BasePremium);
        }

        [Fact]
        public void CreateForNewPolicyPeriod_CreatesCalculationResultWithCorrectlyParsedState()
        {
            // Arrange
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            var calculationResultData = new CachingJObjectWrapper(this.GetSampleJson());
            var quoteData = QuoteFactory.QuoteDataRetriever(new CachingJObjectWrapper(formDataJson), calculationResultData);

            // Act
            var sut = Domain.ReadWriteModel.CalculationResult.CreateForNewPolicy(calculationResultData, quoteData);

            // Assert
            Assert.Equal("premiumComplete", sut.CalculationResultState);
        }

        [Fact]
        public void CreateForNewPolicyPeriod_CreatesCalculationResultWithCorrectlyParsedTriggers_WhenJsonContainsTriggers()
        {
            // Arrange
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            var calculationResultData = new CachingJObjectWrapper(this.GetReviewSampleJson());
            var quoteData = QuoteFactory.QuoteDataRetriever(new CachingJObjectWrapper(formDataJson), calculationResultData);

            // Act
            var sut = Domain.ReadWriteModel.CalculationResult.CreateForNewPolicy(calculationResultData, quoteData);

            // Assert
            Assert.True(sut.HasReviewCalculationTriggers);
            Assert.True(sut.Triggers.Count() == 1);
        }

        [Fact]
        public void CreateForNewPolicyPeriod_CreatesCalculationResultWithNoTriggers_WhenJsonContainsNoTriggers()
        {
            // Arrange
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            var calculationResultData = new CachingJObjectWrapper(this.GetSampleJson());
            var quoteData = QuoteFactory.QuoteDataRetriever(new CachingJObjectWrapper(formDataJson), calculationResultData);

            // Act
            var sut = Domain.ReadWriteModel.CalculationResult.CreateForNewPolicy(calculationResultData, quoteData);

            // Assert
            Assert.Null(sut.Triggers.LastOrDefault());
            Assert.False(sut.HasDeclinedReferralTriggers);
            Assert.False(sut.HasErrorCalculationTriggers);
            Assert.False(sut.HasSoftReferralTriggers);
            Assert.False(sut.HasHardReferralTriggers);
            Assert.False(sut.HasReviewCalculationTriggers);
        }

        [Theory]
        [InlineData(20000, 0, 297, 300, 20000, 0, 297, 300, 0, 0, 0)] // No charges when premium has not changed.
        [InlineData(20000, 5000, 297, 300, 20000, 5000, 297, 300, 5855, 55, 300)] // Merchant fee pro-rated, but transaction costs fixed, when positive payment required.
        [InlineData(20000, 0, 242, 300, 10000, 00, 121, 300, -6050, 0, 0)] // No charges when adjustment results in refund.
        public void CreateForAdjustment_CalculatesPaymentCorrectly(
            int initialBasePremiumInCents,
            int initialBrokerFeeInCents,
            int initialMerchantFeeInCents,
            int initialTransactionCostsInCents,
            int newBasePremiumInCents,
            int newBrokerFeeInCents,
            int newMerchantFeeInCents,
            int newTransactionCostsInCents,
            int expectedTotalPayableInCents,
            int expectedMerchantFeesInCents,
            int expectedTransactionCostsInCents)
        {
            // Arrange
            // Convert from integer cent amounds since InlineData does not support decimals.
            decimal initialBasePremium = initialBasePremiumInCents / 100m;
            decimal initialBrokerFee = initialBrokerFeeInCents / 100m;
            decimal initialMerchantFee = initialMerchantFeeInCents / 100m;
            decimal initialTransactionCosts = initialTransactionCostsInCents / 100m;
            decimal newBasePremium = newBasePremiumInCents / 100m;
            decimal newBrokerFee = newBrokerFeeInCents / 100m;
            decimal newMerchantRate = newMerchantFeeInCents / 100m;
            decimal newTransactionCosts = newTransactionCostsInCents / 100m;
            decimal expectedTotalPayable = expectedTotalPayableInCents / 100m;
            decimal expectedMerchantFees = expectedMerchantFeesInCents / 100m;
            decimal expectedTransactionCosts = expectedTransactionCostsInCents / 100m;

            var policyStartDate = new NodaTime.LocalDate(2020, 4, 16);
            var policyEndDate = policyStartDate.PlusDays(20);
            var formDataJson = FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDatesInDays(policyStartDate);
            var formData = new CachingJObjectWrapper(formDataJson);
            var originalCalculationResultJson = this.GetSampleJsonWithTransactionFee(
                initialBasePremium,
                initialBrokerFee,
                initialMerchantFee,
                initialTransactionCosts);
            var originalCalculationResultData = new CachingJObjectWrapper(originalCalculationResultJson);
            var quoteData = QuoteFactory.QuoteDataRetriever(formData, originalCalculationResultData);
            var originalCalculationResult = Domain.ReadWriteModel.CalculationResult.CreateForNewPolicy(
                originalCalculationResultData, quoteData);
            var newCalculationResultJson = this.GetSampleJsonWithTransactionFee(
                newBasePremium,
                newBrokerFee,
                newMerchantRate,
                newTransactionCosts);
            var newCalculationResultData = new CachingJObjectWrapper(newCalculationResultJson);
            var newQuoteData = QuoteFactory.QuoteDataRetriever(formData, newCalculationResultData);

            // Act
            var sut = Domain.ReadWriteModel.CalculationResult.CreateForAdjustment(
                newCalculationResultData,
                newQuoteData,
                originalCalculationResult.CompoundPrice,
                true,
                policyStartDate,
                policyEndDate);

            // Assert
            sut.PayablePrice.MerchantFees.Should().Be(expectedMerchantFees, "Merchant fee");
            sut.PayablePrice.TransactionCosts.Should().Be(expectedTransactionCosts, "Transaction Costs");
            sut.PayablePrice.TotalPayable.Should().Be(expectedTotalPayable, "Total Payable");
        }

        [Theory]
        [InlineData(20000, 100, 297, 300, 0, 100, 297, 300, 1, 807)] // , DisplayName = "Calculate correct refund If Cancelled 1 day prior to expiry date With Broker Fee"
        [InlineData(20000, 0, 297, 300, 0, 0, 297, 300, 5, 4033)] // DisplayName = "Calculate correct refund If Cancelled 5 days prior to expiry date With out Broker Fee"
        [InlineData(20000, 0, 297, 300, 0, 100, 297, 300, 0, 0)] // DisplayName = "No refund if policy cancelled on the expiration date"
        public void CreateCancellation_CalculatesRefundCorrectly(
        int initialBasePremiumInCents,
        int initialBrokerFeeInCents,
        int initialMerchantFeeInCents,
        int initialTransactionCostsInCents,
        int newBasePremiumInCents,
        int newBrokerFeeInCents,
        int newMerchantFeeInCents,
        int newTransactionCostsInCents,
        int numberOfDaysToExpire,
        int expectedRefundPremiumCents)
        {
            // Arrange
            // Convert from integer cent amounds since DataRow does not support decimals.
            decimal initialBasePremium = initialBasePremiumInCents / 100m;
            decimal initialBrokerFee = initialBrokerFeeInCents / 100m;
            decimal initialMerchantFee = initialMerchantFeeInCents / 100m;
            decimal initialTransactionCosts = initialTransactionCostsInCents / 100m;
            decimal newBasePremium = newBasePremiumInCents / 100m;
            decimal newBrokerFee = newBrokerFeeInCents / 100m;
            decimal newMerchantRate = newMerchantFeeInCents / 100m;
            decimal newTransactionCosts = newTransactionCostsInCents / 100m;
            decimal expectedRefundPremium = expectedRefundPremiumCents / 100m;
            var duration = 30;

            var startDate = new NodaTime.LocalDate(2020, 7, 22);
            var endDate = startDate.PlusDays(duration);
            var formDataJson = FormDataJsonFactory.GetSampleWithStartEndAndEffectiveAndCancelllationDatesInDays(startDate, duration, numberOfDaysToExpire);
            var formData = new CachingJObjectWrapper(formDataJson);

            var cancelFormDataJson = FormDataJsonFactory.GetSampleWithStartEndAndEffectiveAndCancelllationDatesInDays(startDate, duration, numberOfDaysToExpire);
            var cancelFormData = new CachingJObjectWrapper(cancelFormDataJson);

            // var expectedRefundPremium = (numberOfDaysToExpire / duration) * initialBasePremium;
            var originalCalculationResultJson = this.GetSampleJsonWithTransactionFee(
                initialBasePremium,
                initialBrokerFee,
                initialMerchantFee,
                initialTransactionCosts);
            var originalCalculationResultData = new CachingJObjectWrapper(originalCalculationResultJson);
            var quoteData = QuoteFactory.QuoteDataRetriever(formData, originalCalculationResultData);
            var originalCalculationResult = Domain.ReadWriteModel.CalculationResult.CreateForNewPolicy(
                originalCalculationResultData, quoteData);
            var newCalculationResultJson = this.GetSampleJsonWithTransactionFee(
                newBasePremium,
                newBrokerFee,
                newMerchantRate,
                newTransactionCosts);
            var newCalculationResultData = new CachingJObjectWrapper(newCalculationResultJson);
            var newQuoteData = QuoteFactory.QuoteDataRetriever(cancelFormData, newCalculationResultData);

            // Act
            var sut = Domain.ReadWriteModel.CalculationResult.CreateForCancellation(
                newCalculationResultData,
                newQuoteData,
                originalCalculationResult.CompoundPrice,
                true,
                startDate,
                endDate);

            // Assert
            Math.Round(expectedRefundPremium, 2).Should().Be(Math.Round(sut.RefundBreakdown.TotalPremium, 2), "Refund amount should match Total Premium");
        }

        private string GetReviewSampleJson()
        {
            return "{\"state\":\"premiumComplete\",\"questions\":{\"ratingPrimary\":{\"something\": 1,\"ratingState\": \"ACT\"},\"ratingSecondary\":{\"travelCover\": \"No\",\"policyStartDate\": \"16/12/2019\",\"policyAdjustmentDate\": \"16/12/2019\",\"policyEndDate\": \"16/12/2020\"},\"contact\":{},\"details\":{},\"disclosure\":{\"history\": \"No\",\"insolvency\": \"No\",\"declaration\": \"Yes\"},\"paymentOptions\":{\"paymentOption\": \"Monthly\"},\"paymentMethods\":{\"paymentMethodSelect\": \"Visa\",\"paymentMethod\": \"Visa\"},\"review\":{},\"endorsement\":{},},\"risk1\":{\"settings\":{\"riskName\":\"Something\",\"fieldCodePrefix\":\"S\"},\"ratingFactors\":{\"something\":1,\"travelCover\":\"No\",\"ratingState\":\"ACT\",\"policyStartDate\":\"16/12/2019\"},\"checks\":{\"includeRisk\":true},\"premium\":{\"propertyCover\":50.07,\"travelCover\":0,\"basePremium\":50.07,\"premiumPercentACT\":1,\"premiumPercentNSW\":0,\"premiumPercentNT\":0,\"premiumPercentQLD\":0,\"premiumPercentSA\":0,\"premiumPercentTAS\":0,\"premiumPercentVIC\":0,\"premiumPercentWA\":0},\"other\":{\"something\":\"Val\"},\"statutoryRates\":{\"riskType\":\"General\",\"ESLApplicability\":\"Not Applicable\",\"jurisdiction\":\"ACT\",\"policyStartDate\":43815,\"ESLRate\":0,\"GSTRate\":0.1,\"SDRateACT\":0,\"SDRateNSW\":0.09,\"SDRateNT\":0.1,\"SDRateQLD\":0.09,\"SDRateSA\":0.11,\"SDRateTAS\":0.1,\"SDRateVIC\":0.1,\"SDRateWA\":0.1},\"payment\":{\"premium\":50.07,\"ESL\":0,\"GST\":5.007,\"SDACT\":0,\"SDNSW\":0,\"SDNT\":0,\"SDQLD\":0,\"SDSA\":0,\"SDTAS\":0,\"SDVIC\":0,\"SDWA\":0,\"total\":55.077}},\"risk2\":{\"settings\":{\"riskName\":\"Else\",\"fieldCodePrefix\":\"E\"},\"ratingFactors\":{\"something\":1,\"travelCover\":\"No\",\"ratingState\":\"ACT\",\"policyStartDate\":\"16/12/2019\"},\"checks\":{\"includeRisk\":true},\"premium\":{\"propertyCover\":0,\"travelCover\":0,\"basePremium\":0,\"premiumPercentACT\":1,\"premiumPercentNSW\":0,\"premiumPercentNT\":0,\"premiumPercentQLD\":0,\"premiumPercentSA\":0,\"premiumPercentTAS\":0,\"premiumPercentVIC\":0,\"premiumPercentWA\":0},\"other\":{\"something\":\"Val\"},\"statutoryRates\":{\"riskType\":\"Professional Indemnity\",\"ESLApplicability\":\"Applicable\",\"jurisdiction\":\"ACT\",\"policyStartDate\":43815,\"ESLRate\":0.3,\"GSTRate\":0.1,\"SDRateACT\":0,\"SDRateNSW\":0.05,\"SDRateNT\":0.1,\"SDRateQLD\":0.09,\"SDRateSA\":0.11,\"SDRateTAS\":0.1,\"SDRateVIC\":0.1,\"SDRateWA\":0.1},\"payment\":{\"premium\":0,\"ESL\":0,\"GST\":0,\"SDACT\":0,\"SDNSW\":0,\"SDNT\":0,\"SDQLD\":0,\"SDSA\":0,\"SDTAS\":0,\"SDVIC\":0,\"SDWA\":0,\"total\":0}},\"triggers\":{\"review\":{\"inputValue1or3\":true},\"softReferral\":{\"history\":false,\"insolvency\":false},\"hardReferral\":{\"inputValue2or3\":false},\"decline\":{},\"error\":{\"noCoverSelected\":false},},\"payment\":{\"outputVersion\": 2,\"priceComponents\": {\"basePremium\": \"$50.07\",\"ESL\": \"$0.00\",\"premiumGST\": \"$5.01\",\"stampDutyACT\": \"$0.00\",\"stampDutyNSW\": \"$0.00\",\"stampDutyNT\": \"$0.00\",\"stampDutyQLD\": \"$0.00\",\"stampDutySA\": \"$0.00\",\"stampDutyTAS\": \"$0.00\",\"stampDutyVIC\": \"$0.00\",\"stampDutyWA\": \"$0.00\",\"stampDutyTotal\": \"$0.00\",\"totalPremium\": \"$55.08\",\"commission\": \"$10.01\",\"commissionGST\": \"$1.00\",\"brokerFee\": \"$0.00\",\"brokerFeeGST\": \"$0.00\",\"underwriterFee\": \"$0.00\",\"underwriterFeeGST\": \"$0.00\",\"interest\": \"$0.00\",\"merchantFees\": \"$0.83\",\"transactionCosts\": \"$1.80\",\"totalGST\": \"$5.01\",\"totalPayable\": \"$57.70\",},\"total\": {\"premium\": \"$50.07\",\"ESL\": \"$0.00\",\"GST\": \"$5.01\",\"stampDuty\": \"$0.00\",\"serviceFees\": \"$0.00\",\"interest\": \"$0.00\",\"merchantFees\": \"$0.83\",\"transactionCosts\": \"$1.80\",\"payable\": \"$57.70\"},\"instalments\": {\"instalmentsPerYear\": \"12\",\"instalmentAmount\": \"$4.81\"}}}";
        }

        private string GetSampleJson()
        {
            return "{\"state\":\"premiumComplete\",\"questions\":{\"ratingPrimary\":{\"blah\": \"foo\",\"ratingState\": \"VIC\"},\"ratingSecondary\":{\"travelCover\": \"No\",\"policyStartDate\": \"30 / 05 / 2018\"},\"contact\":{},\"personal\":{},\"disclosure\":{\"history\": \"No\",\"insolvency\": \"No\",\"declaration\": \"Yes\"},\"paymentOptions\":{\"paymentOption\": \"Yearly\"},\"paymentMethods\":{\"paymentMethod\": \"Credit Card\",\"creditCardExpiryDate\": \"12 / 21\"},\"successPage\":{},},\"risk1\":{\"settings\":{\"riskName\":\"Something\",\"fieldCodePrefix\":\"S\"},\"ratingFactors\":{\"ratingState\":\"VIC\",\"travelCover\":\"No\",\"ratingState\":\"VIC\",\"policyStartDate\":43250},\"checks\":{\"includeRisk\":true},\"premium\":{\"netBasePremium\":100,\"travelCover\":0,\"totalPremium\":100,\"basePremium\":82.6446280991735},\"other\":{\"something\":\"Val\"},\"statutoryRates\":{\"riskType\":\"General\",\"ESLApplicability\":\"Not Applicable\",\"jurisdiction\":\"VIC\",\"policyStartDate\":43250,\"ESLRate\":0,\"GSTRate\":0.1,\"SDRate\":0.1},\"payment\":{\"premium\":82.6446280991735,\"ESL\":0,\"GST\":8.26446280991735,\"SD\":9.09090909090909,\"total\":100}},\"risk2\":{\"settings\":{\"riskName\":\"Public Liability\",\"keyPrefix\":\"PL\"},\"ratingFactors\":{\"ratingState\":\"VIC\",\"policyStartDate\":43250},\"checks\":{\"includeRisk\":false},\"premium\":{\"basepremium\":0,\"totalPremium\":0},\"other\":{\"Something\":\"Val\"},\"statutoryRates\":{\"riskType\":\"General\",\"ESLApplicability\":\"Applicable\",\"jurisdiction\":\"VIC\",\"policyStartDate\":43250,\"ESLRate\":0,\"GSTRate\":0.1,\"SDRate\":0.1},\"payment\":{\"premium\":0,\"ESL\":0,\"GST\":0,\"SD\":0,\"total\":0}},\"triggers\":{\"softReferral\":{\"history\":false,\"insolvency\":false},\"hardReferral\":{},\"decline\":{},\"error\":{\"noCoverSelected\":false},},\"payment\":{\"total\": {\"premium\": \"82.64\",\"ESL\": \"0.00\",\"GST\": \"8.26\",\"stampDuty\": \"9.09\",\"serviceFees\": \"0.00\",\"interest\": \"0.00\",\"merchantFees\": \"0.00\",\"transactionCosts\": \"0.00\",\"payable\": \"100.00\"},\"instalments\": {\"instalmentsPerYear\": \"1\",\"instalmentAmount\": \"100.00\"}}}";
        }

        private string GetSampleJsonWithTransactionFee(
            decimal basePremium = 200m,
            decimal brokerFee = 0m,
            decimal merchantFees = 2.97m,
            decimal transactionCosts = 3m)
        {
            const decimal GstRate = 0.1m;
            var premiumGst = (basePremium * GstRate).RoundToWholeCents();
            var stampDuty = ((basePremium + premiumGst) * 0.1m).RoundToWholeCents();
            var brokerFeeGst = (brokerFee * GstRate).RoundToWholeCents();
            return $@"{{
    ""state"": ""premiumComplete"",
    ""payment"" : {{
        ""outputVersion"": 2,
        ""currencyCode"": ""AUD"",
        ""priceComponents"": {{
            ""basePremium"": ""{basePremium}"",
            ""esl"": ""0.00"",
            ""premiumGst"": ""{premiumGst}"",
            ""stampDutyAct"": ""0.00"",
            ""stampDutyNsw"": ""0.00"",
            ""stampDutyNt"": ""0.00"",
            ""stampDutyQld"": ""0.00"",
            ""stampDutySa"": ""0.00"",
            ""stampDutyTas"": ""0.00"",
            ""stampDutyWa"": ""0.00"",
            ""stampDutyVic"": ""{stampDuty}"",
            ""commission"": ""0.00"",
            ""commissionGst"": ""0.00"",
            ""brokerFee"": ""{brokerFee}"",
            ""brokerFeeGst"": ""{brokerFeeGst}"",
            ""underwriterFee"": ""0.00"",
            ""underwriterFeeGst"": ""0.00"",
            ""interest"": ""0.00"",
            ""merchantFees"": ""{merchantFees}"",
            ""transactionCosts"": ""{transactionCosts}""
        }}
    }}
}}";
        }
    }
}

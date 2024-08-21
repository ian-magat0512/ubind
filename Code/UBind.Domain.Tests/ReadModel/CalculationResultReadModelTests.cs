// <copyright file="CalculationResultReadModelTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.ReadModel
{
    using UBind.Domain.ReadModel;
    using Xunit;

    public class CalculationResultReadModelTests
    {
        [Fact]
        public void PayablePrice_ReturnsPayablePrice_WhenJsonContainsFullSerializedCalculationResult()
        {
            // Arrange
            var json = @"{
                ""FormDataId"":""46ebe68c-6450-435c-833f-24ca1dfa8607"",
                ""Json"":""{\""state\"":\""bindingQuote\"",\""questions\"":{\""ratingPrimary\"":{\""occupation\"": \""Air Conditioning Service Trade\"",\""liabilityLimit\"": \""$5,000,000.00\"",\""tools\"": \""$5,000.00\"",\""turnover\"": \""150k-300k\"",\""employees\"": \""1\"",\""ratingState\"": \""VIC\""},\""ratingSecondary\"":{\""policyStartDate\"": \""04/03/2020\"",\""policyEndDate\"": \""04/03/2021\""},\""contact\"":{\""contactName\"": \""Adjustment json test 2\"",\""contactEmail\"": \""2@2.com\"",\""contactPhone\"": \""0412341234\"",\""contactMobile\"": \""0412341234\""},\""personal\"":{\""insuredFullName\"": \""Adjustment json test 2\"",\""residentialAddress\"": \""foo\"",\""residentialTown\"": \""foo\"",\""residentialState\"": \""VIC\"",\""residentialPostcode\"": \""3000\""},\""disclosure\"":{\""liability\"": \""No\"",\""general\"": \""No\"",\""declaration\"": \""Yes\""},\""paymentOptions\"":{\""paymentOption\"": \""Yearly\""},\""paymentMethods\"":{\""paymentMethodSelect\"": \""Pay now with credit card\"",\""creditCardExpiryDate\"": \""12/22\""},\""successPage\"":{},},\""risk1\"":{\""settings\"":{\""riskName\"":\""General\"",\""fieldCodePrefix\"":\""G\""},\""ratingFactors\"":{\""tools\"":5000,\""ratingState\"":\""VIC\"",\""policyStartDate\"":\""04/03/2020\""},\""checks\"":{\""includeRisk\"":true},\""premium\"":{\""totalPremium\"":200,\""basePremium\"":200},\""other\"":{\""something\"":\""Val\""},\""statutoryRates\"":{\""riskType\"":\""General\"",\""ESLApplicability\"":\""Not Applicable\"",\""jurisdiction\"":\""VIC\"",\""policyStartDate\"":\""04/03/2020\"",\""ESLRate\"":0,\""GSTRate\"":0.1,\""SDRate\"":0.1},\""payment\"":{\""premium\"":200,\""ESL\"":0,\""GST\"":20,\""SD\"":22,\""total\"":242}},\""risk2\"":{\""settings\"":{\""riskName\"":\""Public Liability\"",\""keyPrefix\"":\""PL\""},\""ratingFactors\"":{\""liabilityLimit\"":5000000,\""turnover\"":\""150k-300k\"",\""ratingState\"":\""VIC\"",\""policyStartDate\"":\""04/03/2020\""},\""checks\"":{\""includeRisk\"":true},\""premium\"":{\""basepremium\"":450,\""totalPremium\"":450},\""other\"":{\""Something\"":\""Val\""},\""statutoryRates\"":{\""riskType\"":\""General\"",\""ESLApplicability\"":\""Applicable\"",\""jurisdiction\"":\""VIC\"",\""policyStartDate\"":\""04/03/2020\"",\""ESLRate\"":0,\""GSTRate\"":0.1,\""SDRate\"":0.1},\""payment\"":{\""premium\"":450,\""ESL\"":0,\""GST\"":45,\""SD\"":49.5,\""total\"":544.5}},\""triggers\"":{\""softReferral\"":{\""liabilityReferral\"":false,\""generalReferral\"":false},\""hardReferral\"":{},\""decline\"":{},\""error\"":{\""noCoverSelected\"":false},},\""payment\"":{\""total\"": {\""premium\"": \""$650.00\"",\""ESL\"": \""$0.00\"",\""GST\"": \""$65.00\"",\""stampDuty\"": \""$71.50\"",\""serviceFees\"": \""$0.00\"",\""interest\"": \""$0.00\"",\""merchantFees\"": \""$0.00\"",\""transactionCosts\"": \""$0.00\"",\""payable\"": \""$786.50\""},\""instalments\"": {\""instalmentsPerYear\"": \""1\"",\""instalmentAmount\"": \""$786.50\""},\""ctm\"":{\""Prem\"":\""$650.00\"",\""GST\"":\""$65.00\"",\""totalPG\"":\""$715.00\"",\""sDuty\"":\""$71.50\"",\""bFee\"":\""$0.00\"",\""bFeeGST\"":\""$0.00\"",\""totalBG\"":\""$0.00\"",\""total\"":\""$786.50\"",}}}"",
                ""ParentRefund"":{ ""BasePremium"":680.00,""Esl"":0.00,""PremiumGst"":68.00,""StampDuty"":74.80,""StampDutyAct"":0.0,""StampDutyNsw"":0.0,""StampDutyNt"":0.0,""StampDutyQld"":0.0,""StampDutySa"":0.0,""StampDutyTas"":0.0,""StampDutyVic"":0.0,""StampDutyWa"":0.0,""Commission"":0.0,""CommissionGst"":0.0,""BrokerFee"":0.0,""BrokerFeeGst"":0.0,""UnderwriterFee"":0.0,""UnderwriterFeeGst"":0.0,""TotalGst"":68.00,""ServiceFees"":0.0,""InterestCharges"":0.0,""MerchantFees"":0.0,""TransactionCosts"":0.0,""PeriodInDays"":0,""StampDutyTotal"":74.80,""TotalPremium"":822.80,""TotalPayable"":822.80},
                ""PayablePrice"":{""BasePremium"":-30.00,""Esl"":0.00,""PremiumGst"":-3.00,""StampDuty"":-3.30,""StampDutyAct"":0.0,""StampDutyNsw"":0.0,""StampDutyNt"":0.0,""StampDutyQld"":0.0,""StampDutySa"":0.0,""StampDutyTas"":0.0,""StampDutyVic"":0.0,""StampDutyWa"":0.0,""Commission"":0.0,""CommissionGst"":0.0,""BrokerFee"":0.0,""BrokerFeeGst"":0.0,""UnderwriterFee"":0.0,""UnderwriterFeeGst"":0.0,""TotalGst"":-3.00,""ServiceFees"":0.00,""InterestCharges"":0.00,""MerchantFees"":0.00,""TransactionCosts"":0.00,""PeriodInDays"":0,""StampDutyTotal"":-3.30,""TotalPremium"":-36.30,""TotalPayable"":-36.30},
                ""CalculationResultState"":""bindingQuote"",
                ""IsBindable"":true,
                ""HasSoftReferralTriggers"":false,
                ""HasEndorsementTriggers"":false,
                ""HasHardReferralTriggers"":false,
                ""HasDeclinedReferralTriggers"":false,
                ""HasErrorCalculationTriggers"":false,
                ""HasReviewCalculationTriggers"":false}";

            // Act
            var sut = new CalculationResultReadModel(json);

            // Assert
            Assert.Equal(-36.3m, sut.PayablePrice.TotalPayable);
        }

        [Fact]
        public void PayablePrice_ReturnsPayablePrice_WhenJsonContainsOnlyJsonProperty()
        {
            // Arrange
            var json = @"{""Json"": ""{\""state\"":\""bindingQuote\"",\""questions\"":{\""ratingPrimary\"":{\""occupation\"": \""Air Conditioning Service Trade\"",\""liabilityLimit\"": \""$10,000,000.00\"",\""tools\"": \""$5,000.00\"",\""turnover\"": \""150k-300k\"",\""employees\"": \""1\"",\""ratingState\"": \""VIC\""},\""ratingSecondary\"":{\""policyStartDate\"": \""04/03/2020\"",\""policyEndDate\"": \""04/03/2021\""},\""contact\"":{\""contactName\"": \""Adjustment json test 2\"",\""contactEmail\"": \""2@2.com\"",\""contactPhone\"": \""0412341234\"",\""contactMobile\"": \""0412341234\""},\""personal\"":{\""insuredFullName\"": \""Adjustment json test 2\"",\""residentialAddress\"": \""foo\"",\""residentialTown\"": \""foo\"",\""residentialState\"": \""VIC\"",\""residentialPostcode\"": \""3000\""},\""disclosure\"":{\""liability\"": \""No\"",\""general\"": \""No\"",\""declaration\"": \""Yes\""},\""paymentOptions\"":{\""paymentOption\"": \""Yearly\""},\""paymentMethods\"":{\""paymentMethodSelect\"": \""Pay now with credit card\""},\""successPage\"":{},},\""risk1\"":{\""settings\"":{\""riskName\"":\""General\"",\""fieldCodePrefix\"":\""G\""},\""ratingFactors\"":{\""tools\"":5000,\""ratingState\"":\""VIC\"",\""policyStartDate\"":\""04/03/2020\""},\""checks\"":{\""includeRisk\"":true},\""premium\"":{\""totalPremium\"":200,\""basePremium\"":200},\""other\"":{\""something\"":\""Val\""},\""statutoryRates\"":{\""riskType\"":\""General\"",\""ESLApplicability\"":\""Not Applicable\"",\""jurisdiction\"":\""VIC\"",\""policyStartDate\"":\""04/03/2020\"",\""ESLRate\"":0,\""GSTRate\"":0.1,\""SDRate\"":0.1},\""payment\"":{\""premium\"":200,\""ESL\"":0,\""GST\"":20,\""SD\"":22,\""total\"":242}},\""risk2\"":{\""settings\"":{\""riskName\"":\""Public Liability\"",\""keyPrefix\"":\""PL\""},\""ratingFactors\"":{\""liabilityLimit\"":10000000,\""turnover\"":\""150k-300k\"",\""ratingState\"":\""VIC\"",\""policyStartDate\"":\""04/03/2020\""},\""checks\"":{\""includeRisk\"":true},\""premium\"":{\""basepremium\"":480,\""totalPremium\"":480},\""other\"":{\""Something\"":\""Val\""},\""statutoryRates\"":{\""riskType\"":\""General\"",\""ESLApplicability\"":\""Applicable\"",\""jurisdiction\"":\""VIC\"",\""policyStartDate\"":\""04/03/2020\"",\""ESLRate\"":0,\""GSTRate\"":0.1,\""SDRate\"":0.1},\""payment\"":{\""premium\"":480,\""ESL\"":0,\""GST\"":48,\""SD\"":52.8,\""total\"":580.8}},\""triggers\"":{\""softReferral\"":{\""liabilityReferral\"":false,\""generalReferral\"":false},\""hardReferral\"":{},\""decline\"":{},\""error\"":{\""noCoverSelected\"":false},},\""payment\"":{\""total\"": {\""premium\"": \""$680.00\"",\""ESL\"": \""$0.00\"",\""GST\"": \""$68.00\"",\""stampDuty\"": \""$74.80\"",\""serviceFees\"": \""$0.00\"",\""interest\"": \""$0.00\"",\""merchantFees\"": \""$0.00\"",\""transactionCosts\"": \""$0.00\"",\""payable\"": \""$822.80\""},\""instalments\"": {\""instalmentsPerYear\"": \""1\"",\""instalmentAmount\"": \""$822.80\""},\""ctm\"":{\""Prem\"":\""$680.00\"",\""GST\"":\""$68.00\"",\""totalPG\"":\""$748.00\"",\""sDuty\"":\""$74.80\"",\""bFee\"":\""$0.00\"",\""bFeeGST\"":\""$0.00\"",\""totalBG\"":\""$0.00\"",\""total\"":\""$822.80\"",}}}""}";

            // Act
            var sut = new CalculationResultReadModel(json);

            // Assert
            Assert.Equal(822.80m, sut.PayablePrice.TotalPayable);
        }

        [Fact]
        public void PayablePrice_ReturnsZeroPayablePrice_WhenJsonIsNull()
        {
            // Arrange
            string json = null;

            // Act
            var sut = new CalculationResultReadModel(json);

            // Assert
            Assert.Equal(0m, sut.PayablePrice.TotalPayable);
        }
    }
}

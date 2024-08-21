// <copyright file="CalculationResultTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.ReadWriteModel;

using System.Linq;
using Newtonsoft.Json;
using NodaTime;
using UBind.Domain.Json;
using UBind.Domain.ReadWriteModel;
using UBind.Domain.Tests.Fakes;
using Xunit;

/// <summary>
/// Test class for calculation results.
/// </summary>
public class CalculationResultTests
{
    private const string SampleNewFormatCalcResultJson117 = "{\"payment\" : {\"outputVersion\":2,\"priceComponents\":{    \"basePremium\":\"$117.00\",   \"ESL\":\"$0.00\",   \"premiumGST\":\"$11.70\",   \"stampDutyACT\":\"$0.00\",   \"stampDutyNSW\":\"$0.00\",   \"stampDutyNT\":\"$0.00\",   \"stampDutyQLD\":\"$0.00\",   \"stampDutySA\":\"$0.00\",		   \"stampDutyTAS\":\"$0.00\",   \"stampDutyVIC\":\"$12.87\",   \"stampDutyWA\":\"$0.00\",   \"stampDutyTotal\":\"$12.87\",   \"totalPremium\":\"$141.57\",   \"commission\":\"$23.40\",   \"commissionGST\":\"$2.34\",   \"brokerFee\":\"$0.00\",   \"brokerFeeGST\":\"$0.00\",   \"underwriterFee\":\"$0.00\",   \"underwriterFeeGST\":\"$0.00\",   \"interest\":\"$0.00\",   \"merchantFees\":\"$2.12\",   \"transactionCosts\":\"$1.80\",   \"totalGST\":\"$11.70\",   \"totalPayable\":\"$145.49\",},\"total\":{    \"premium\":\"$117.00\",   \"ESL\":\"$0.00\",   \"GST\":\"$11.70\",   \"stampDuty\":\"$12.87\",   \"serviceFees\":\"$0.00\",   \"interest\":\"$0.00\",   \"merchantFees\":\"$2.12\",   \"transactionCosts\":\"$1.80\",   \"payable\":\"$145.49\"},\"instalments\":{    \"instalmentsPerYear\":\"12\",   \"instalmentAmount\":\"$12.12\"}}}";
    private const string SampleNewFormatCalcResultJson217 = "{\"payment\" : {\"outputVersion\":2,\"priceComponents\":{    \"basePremium\":\"$217.00\",   \"ESL\":\"$0.00\",   \"premiumGST\":\"$21.70\",   \"stampDutyACT\":\"$0.00\",   \"stampDutyNSW\":\"$0.00\",   \"stampDutyNT\":\"$0.00\",   \"stampDutyQLD\":\"$0.00\",   \"stampDutySA\":\"$0.00\",		   \"stampDutyTAS\":\"$0.00\",   \"stampDutyVIC\":\"$23.87\",   \"stampDutyWA\":\"$0.00\",   \"stampDutyTotal\":\"$23.87\",   \"totalPremium\":\"262.57\",   \"commission\":\"$23.40\",   \"commissionGST\":\"$2.34\",   \"brokerFee\":\"$0.00\",   \"brokerFeeGST\":\"$0.00\",   \"underwriterFee\":\"$0.00\",   \"underwriterFeeGST\":\"$0.00\",   \"interest\":\"$0.00\",   \"merchantFees\":\"$2.12\",   \"transactionCosts\":\"$1.80\",   \"totalGST\":\"$11.70\",   \"totalPayable\":\"$145.49\",},\"total\":{    \"premium\":\"$117.00\",   \"ESL\":\"$0.00\",   \"GST\":\"$11.70\",   \"stampDuty\":\"$12.87\",   \"serviceFees\":\"$0.00\",   \"interest\":\"$0.00\",   \"merchantFees\":\"$2.12\",   \"transactionCosts\":\"$1.80\",   \"payable\":\"$145.49\"},\"instalments\":{    \"instalmentsPerYear\":\"12\",   \"instalmentAmount\":\"$12.12\"}}}";

    [Fact]
    public void CalculationResult_RoundTripsSuccessfully_WhenIncludingCompoundPrice()
    {
        // Arrange
        var fixedComponents = PriceBreakdown.CreateFromCalculationFormatV1(
            new CalculatedPaymentTotal(0, 0, 0, 0, 10, 0, 0, 0, 10, "AUD"));
        var scalableComponents = PriceBreakdown.CreateFromCalculationFormatV1(
            new CalculatedPaymentTotal(100, 0, 0, 0, 0, 0, 0, 0, 0, "AUD"));
        var parentPriceComponents = new FixedAndScalablePrice(
            fixedComponents,
            scalableComponents);
        var parentStartDate = new LocalDate(2018, 1, 1);
        var parentEndDate = new LocalDate(2019, 1, 1);
        var parentPrice = new CompoundPrice(new IntervalPrice(parentPriceComponents, parentStartDate, parentEndDate));
        var sut = CalculationResult.CreateForAdjustment(
            new CachingJObjectWrapper(CalculationResultJsonFactory.Create()),
            QuoteDataFactory.GetSample(parentStartDate, parentEndDate, parentStartDate.PlusDays(100)),
            parentPrice,
            true,
            parentStartDate,
            parentEndDate);
        var serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
        };

        // Act
        var json = JsonConvert.SerializeObject(sut, serializerSettings);
        var result = JsonConvert.DeserializeObject<CalculationResult>(
            json, serializerSettings);

        // Assert
        Assert.Equal(10, result.CompoundPrice.IntervalPrices.First().FixedAndScalablePrice.FixedComponents.ServiceFees);
    }

    [Fact]
    public void CalculateRefund_CorrectlyRefunds_PartialAmountFromSingleInterval_FromCalculatedResultJson()
    {
        // Arrange
        var startDate = new LocalDate(2019, 7, 1);
        var calculationResult = CalculationResult.CreateForNewPolicy(
            new CachingJObjectWrapper(SampleNewFormatCalcResultJson117),
            QuoteDataFactory.GetSample(startDate, Period.FromDays(100)));
        var refundDate = startDate.PlusDays(65);

        // Act
        var refund = calculationResult.CompoundPrice.CalculateRefund(refundDate);

        // Assert
        var expectedRefund = 49.55m; // (117 + 11.7 + 12.87) * (35m / 100m)
        Assert.Equal(expectedRefund, refund.TotalPayable);
    }

    [Fact]
    public void CalculateRefund_CorrectlyRefundsAmountFromPartialLastInterval_WhenUsingNewFormatCalculatedResultJson()
    {
        // Arrange
        var startDate = new LocalDate(2019, 7, 1);
        var endDate = startDate.PlusDays(200);
        var parentCalculationResult = CalculationResult.CreateForNewPolicy(
            new CachingJObjectWrapper(SampleNewFormatCalcResultJson217),
            QuoteDataFactory.GetSample(startDate, endDate));
        var adjustmentDate = startDate.PlusDays(100);
        var childCalculationResult = CalculationResult.CreateForAdjustment(
            new CachingJObjectWrapper(SampleNewFormatCalcResultJson117),
            QuoteDataFactory.GetSample(startDate, endDate, adjustmentDate),
            parentCalculationResult.CompoundPrice,
            true,
            startDate,
            endDate);
        var refundDate = startDate.PlusDays(150);

        // Act
        var refund = childCalculationResult.CalculateRefund(refundDate);

        // Assert
        var expectedRefund = 35.39m; // (117 + 11.7 + 12.87) / 4
        Assert.Equal(expectedRefund, refund.TotalPayable);
    }

    [Fact]
    public void CalculateRefund_CorrectlyRefunds_AmountFromWholeLastIntervalNewFormat_fromCalculatedResultJson()
    {
        // Arrange
        var startDate = new LocalDate(2019, 7, 1);
        var endDate = startDate.PlusDays(200);
        var parentCalculationResult = CalculationResult.CreateForNewPolicy(
            new CachingJObjectWrapper(SampleNewFormatCalcResultJson217),
            QuoteDataFactory.GetSample(startDate, endDate));
        var adjustmentDate = startDate.PlusDays(100);
        var childCalculationResult = CalculationResult.CreateForAdjustment(
            new CachingJObjectWrapper(SampleNewFormatCalcResultJson117),
            QuoteDataFactory.GetSample(startDate, endDate, adjustmentDate),
            parentCalculationResult.CompoundPrice,
            true,
            startDate,
            endDate);

        // Act
        var refund = childCalculationResult.CalculateRefund(adjustmentDate);

        // Assert
        var expectedRefund = 70.78m; // (117 + 11.7 + 12.87) / 2
        ////var expectedRefund = 141.57m; // (117 + 11.7 + 12.87)
        Assert.Equal(expectedRefund, refund.TotalPayable);
    }

    [Fact]
    public void CalculateRefund_CorrectlyRefunds_ActualData()
    {
        // Arrange
        var actualJsonData = "{\"state\":\"premiumComplete\",\"questions\":{\"ratingPrimary\":{\"something\": 123467,\"ratingState\": \"QLD\"},\"ratingSecondary\":{\"travelCover\": \"Yes\",\"policyStartDate\": \"12/11/2019\",\"policyAdjustmentDate\": \"12/11/2019\",\"policyEndDate\": \"12/11/2020\"},\"contact\":{},\"personal\":{},\"disclosure\":{\"history\": \"No\",\"insolvency\": \"No\",\"declaration\": \"Yes\"},\"paymentOptions\":{\"paymentOption\": \"Monthly\"},\"paymentMethods\":{\"paymentMethodSelect\": \"Visa\",\"paymentMethod\": \"Visa\"},\"successPage\":{},},\"risk1\":{\"settings\":{\"riskName\":\"Something\",\"fieldCodePrefix\":\"S\"},\"ratingFactors\":{\"something\":123467,\"travelCover\":\"Yes\",\"ratingState\":\"QLD\",\"policyStartDate\":\"12/11/2019\"},\"checks\":{\"includeRisk\":true},\"premium\":{\"propertyCover\":8692.69,\"travelCover\":60,\"basePremium\":8752.69,\"premiumPercentACT\":0,\"premiumPercentNSW\":0,\"premiumPercentNT\":0,\"premiumPercentQLD\":1,\"premiumPercentSA\":0,\"premiumPercentTAS\":0,\"premiumPercentVIC\":0,\"premiumPercentWA\":0},\"other\":{\"something\":\"Val\"},\"statutoryRates\":{\"riskType\":\"General\",\"ESLApplicability\":\"Not Applicable\",\"jurisdiction\":\"QLD\",\"policyStartDate\":43781,\"ESLRate\":0,\"GSTRate\":0.1,\"SDRateACT\":0,\"SDRateNSW\":0.09,\"SDRateNT\":0.1,\"SDRateQLD\":0.09,\"SDRateSA\":0.11,\"SDRateTAS\":0.1,\"SDRateVIC\":0.1,\"SDRateWA\":0.1},\"payment\":{\"premium\":8752.69,\"ESL\":0,\"GST\":875.269,\"SDACT\":0,\"SDNSW\":0,\"SDNT\":0,\"SDQLD\":866.51631,\"SDSA\":0,\"SDTAS\":0,\"SDVIC\":0,\"SDWA\":0,\"total\":9627.959}},\"risk2\":{\"settings\":{\"riskName\":\"Else\",\"fieldCodePrefix\":\"E\"},\"ratingFactors\":{\"something\":123467,\"travelCover\":\"Yes\",\"ratingState\":\"QLD\",\"policyStartDate\":\"12/11/2019\"},\"checks\":{\"includeRisk\":true},\"premium\":{\"propertyCover\":0,\"travelCover\":0,\"basePremium\":0,\"premiumPercentACT\":0,\"premiumPercentNSW\":0,\"premiumPercentNT\":0,\"premiumPercentQLD\":1,\"premiumPercentSA\":0,\"premiumPercentTAS\":0,\"premiumPercentVIC\":0,\"premiumPercentWA\":0},\"other\":{\"something\":\"Val\"},\"statutoryRates\":{\"riskType\":\"Professional Indemnity\",\"ESLApplicability\":\"Applicable\",\"jurisdiction\":\"QLD\",\"policyStartDate\":43781,\"ESLRate\":0.3,\"GSTRate\":0.1,\"SDRateACT\":0,\"SDRateNSW\":0.05,\"SDRateNT\":0.1,\"SDRateQLD\":0.09,\"SDRateSA\":0.11,\"SDRateTAS\":0.1,\"SDRateVIC\":0.1,\"SDRateWA\":0.1},\"payment\":{\"premium\":0,\"ESL\":0,\"GST\":0,\"SDACT\":0,\"SDNSW\":0,\"SDNT\":0,\"SDQLD\":0,\"SDSA\":0,\"SDTAS\":0,\"SDVIC\":0,\"SDWA\":0,\"total\":0}},\"triggers\":{\"softReferral\":{\"history\":false,\"insolvency\":false},\"hardReferral\":{},\"decline\":{},\"error\":{\"noCoverSelected\":false},},\"payment\":{\"outputVersion\": 2,\"priceComponents\": {\"basePremium\": \"$8,752.69\",\"ESL\": \"$0.00\",\"premiumGST\": \"$875.27\",\"stampDutyACT\": \"$0.00\",\"stampDutyNSW\": \"$0.00\",\"stampDutyNT\": \"$0.00\",\"stampDutyQLD\": \"$866.52\",\"stampDutySA\": \"$0.00\",\"stampDutyTAS\": \"$0.00\",\"stampDutyVIC\": \"$0.00\",\"stampDutyWA\": \"$0.00\",\"stampDutyTotal\": \"$866.52\",\"totalPremium\": \"$10,494.48\",\"commission\": \"$1,750.54\",\"commissionGST\": \"$175.05\",\"brokerFee\": \"$0.00\",\"brokerFeeGST\": \"$0.00\",\"underwriterFee\": \"$0.00\",\"underwriterFeeGST\": \"$0.00\",\"interest\": \"$0.00\",\"merchantFees\": \"$157.42\",\"transactionCosts\": \"$1.80\",\"totalGST\": \"$875.27\",\"totalPayable\": \"$10,653.69\",},\"total\": {\"premium\": \"$8,752.69\",\"ESL\": \"$0.00\",\"GST\": \"$875.27\",\"stampDuty\": \"$866.52\",\"serviceFees\": \"$0.00\",\"interest\": \"$0.00\",\"merchantFees\": \"$157.42\",\"transactionCosts\": \"$1.80\",\"payable\": \"$10,653.69\"},\"instalments\": {\"instalmentsPerYear\": \"12\",\"instalmentAmount\": \"$887.81\"}}}";
        var startDate = new LocalDate(2019, 11, 12); // 12th Nov 2019
        var endDate = new LocalDate(2020, 11, 12); // 366 days later (due to leap year!)
        var parentCalculationResult = CalculationResult.CreateForNewPolicy(
            new CachingJObjectWrapper(actualJsonData),
            QuoteDataFactory.GetSample(startDate, endDate));
        var refundDate = startDate.PlusDays(81);

        // Act
        var refund = parentCalculationResult.CalculateRefund(refundDate); // 81 days after start date

        // Assert
        decimal expectedRefund = 8171.93m; // = 8,171.9311...
        Assert.Equal(expectedRefund, refund.TotalPayable);
    }

    [Fact]
    public void Calculation_RefundComponent_IfPayablePriceTotalPayable_NegativeValue_RefundIsGreaterthanZero()
    {
        // Arrange
        var startDate = new LocalDate(2019, 7, 1);
        var endDate = startDate.PlusDays(200);
        var parentCalculationResult = CalculationResult.CreateForNewPolicy(
            new CachingJObjectWrapper(SampleNewFormatCalcResultJson217),
            QuoteDataFactory.GetSample(startDate, endDate));
        var adjustmentDate = startDate.PlusDays(100);
        var childCalculationResult = CalculationResult.CreateForAdjustment(
            new CachingJObjectWrapper(SampleNewFormatCalcResultJson117),
            QuoteDataFactory.GetSample(startDate, endDate, adjustmentDate),
            parentCalculationResult.CompoundPrice,
            true,
            startDate,
            endDate);

        // Act
        var totalPayable = childCalculationResult.PayablePrice.TotalPayable;
        var refundComponent = childCalculationResult.RefundBreakdown;

        // Assert
        var expectedRefund = 60.50m; // (Merch + trans now excluded)
        Assert.Equal(refundComponent.TotalPayable, expectedRefund);
        Assert.Equal(totalPayable, expectedRefund * -1);
    }

    [Fact]
    public void Calculation_RefundComponent_IfPayablePriceTotalPayable_PostiveValue_RefundIsZero()
    {
        // Arrange
        var startDate = new LocalDate(2019, 7, 1);
        var endDate = startDate.PlusDays(200);
        var parentCalculationResult = CalculationResult.CreateForNewPolicy(
            new CachingJObjectWrapper(SampleNewFormatCalcResultJson217),
            QuoteDataFactory.GetSample(startDate, endDate));

        // Act
        var totalPayable = parentCalculationResult.PayablePrice.TotalPayable;
        var refundComponent = parentCalculationResult.RefundBreakdown;

        // Assert
        var expectedRefund = 0.0m;
        Assert.Equal(refundComponent.TotalPayable, expectedRefund);
        Assert.Equal(266.49m, totalPayable);
    }
}

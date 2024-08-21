// <copyright file="CalculationJsonSanitizerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Services
{
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Services;
    using Xunit;

    public class CalculationJsonSanitizerTests
    {
        [Fact]
        public void Sanitize_ThrowsJsonSanitizationException_WhenItCannotSanitizeJson()
        {
            // Arrange
            var pseudoJson = "{\"foo\": \"bar\", qwerty}";
            var sut = new CalculationJsonSanitizer();

            // Act + Assert
            Assert.Throws<JsonSanitizationException>(() => sut.Sanitize(pseudoJson));
        }

        [Fact]
        public void Sanitize_CorrectlyWrapsNA_InCalculationOutput()
        {
            // Arrange
            var pseudoJson = "{\"state\":#N/A,\"questions\":{\"ratingPrimary\":{\"occupation\": \"Alarm Installer\",\"ratingState\": \"ACT\",\"liabilityLimit\": \"$10,000,000.00\",\"tools\": \"$5,000.00\",\"turnover\": \"$0 - $250k\",\"employees\": \"1\",\"policyExcess\": \"$250.00\"},\"ratingSecondary\":{\"policyStartDate\": \"21/09/2018\",\"policyEndDate\": \"21/09/2019\"},\"contact\":{},#N/A\"disclosure\":{\"declaration\": \"Yes\"},\"paymentOptions\":{\"paymentOption\": \"Monthly\"},\"paymentMethods\":{\"paymentMethodSelect\": \"Pay now with credit card\"},\"successPage\":{},},\"risk1\":{\"settings\":{\"riskName\":\"General\",\"fieldCodePrefix\":\"G\"},\"ratingFactors\":{\"tools\":\"5000\",\"ratingState\":\"ACT\",\"policyStartDate\":\"21/09/2018\"},\"checks\":{\"includeRisk\":true},\"premium\":{\"totalPremium\":200,\"basePremium\":200},\"other\":{\"something\":\"Val\"},\"statutoryRates\":{\"riskType\":\"General\",\"ESLApplicability\":\"Not Applicable\",\"jurisdiction\":\"ACT\",\"policyStartDate\":\"21/09/2018\",\"ESLRate\":0,\"GSTRate\":0.1,\"SDRate\":0},\"payment\":{\"premium\":200,\"ESL\":0,\"GST\":20,\"SD\":0,\"total\":220}},\"risk2\":{\"settings\":{\"riskName\":\"Public Liability\",\"keyPrefix\":\"PL\"},\"ratingFactors\":{\"liabilityLimit\":\"10000000\",\"turnover\":\"$0 - $250k\",\"ratingState\":\"ACT\",\"policyStartDate\":\"21/09/2018\"},\"checks\":{\"includeRisk\":true},\"premium\":{\"basepremium\":380,\"totalPremium\":380},\"other\":{\"Something\":\"Val\"},\"statutoryRates\":{\"riskType\":\"General\",\"ESLApplicability\":\"Applicable\",\"jurisdiction\":\"ACT\",\"policyStartDate\":\"21/09/2018\",\"ESLRate\":0,\"GSTRate\":0.1,\"SDRate\":0},\"payment\":{\"premium\":380,\"ESL\":0,\"GST\":38,\"SD\":0,\"total\":418}},\"triggers\":{\"softReferral\":{\"liabilityReferral\":false,\"generalReferral\":false,\"toolsover10000\":false},\"hardReferral\":{},\"decline\":{\"employees\":false},\"error\":{\"noCoverSelected\":false},},\"payment\":{\"total\": {\"premium\": \"$580.00\",\"ESL\": \"$0.00\",\"GST\": \"$58.00\",\"stampDuty\": \"$0.00\",\"serviceFees\": \"$0.00\",\"interest\": \"$0.00\",\"merchantFees\": \"$0.00\",\"transactionCosts\": \"$0.00\",\"payable\": \"$638.00\"},\"instalments\": {\"instalmentsPerYear\": \"12\",\"instalmentAmount\": \"$53.17\"},\"ctm\":{\"Prem\":\"$580.00\",\"GST\":\"$58.00\",\"totalPG\":\"$638.00\",\"sDuty\":\"$0.00\",\"bFee\":\"$0.00\",\"bFeeGST\":\"$0.00\",\"totalBG\":\"$0.00\",\"total\":\"$638.00\",}}}";
            var sut = new CalculationJsonSanitizer();

            // Act
            var json = sut.Sanitize(pseudoJson);

            // Assert
            // If json is parsable, then test has passed.
            var jobject = JObject.Parse(json);
        }
    }
}

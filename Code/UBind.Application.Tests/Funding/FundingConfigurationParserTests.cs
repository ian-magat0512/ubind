// <copyright file="FundingConfigurationParserTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Funding
{
    using FluentAssertions;
    using UBind.Application.Funding;
    using UBind.Application.Funding.Iqumulate;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="FundingConfigurationParserTests" />.
    /// </summary>
    public class FundingConfigurationParserTests
    {
        /// <summary>
        /// The Parse_ParsesIqumulateConfigurationCorrectly_ForValidJson.
        /// </summary>
        [Fact]
        public void Parse_ParsesIqumulateConfigurationCorrectly_ForValidJson()
        {
            // Arrange
            var json = @"{
    ""type"": ""IQumulate"",
    ""default"": {
        ""BaseUrl"": ""testScriptUrl"",
        ""PaymentMethod"": ""testPaymentMethod"",
        ""AffinitySchemeCode"": ""testAffinitySchemeCode"",
        ""IntroducerContactEmail"": ""testIntroducerContactEmail"",
        ""PolicyClassCode"": ""testPolicyClassCode"",
        ""PolicyUnderwriterCode"": ""testPolicyUnderwriterCode"",
    },
    ""quoteDataLocator"": {
        ""insuredName"": {
            ""object"": ""CalculationResult"",
            ""path"": ""questions.personal.insuredFullName""
        },
        ""inceptionDate"": {
            ""object"": ""CalculationResult"",
            ""path"": ""questions.ratingSecondary.policyStartDateFormatted""
        },
        ""expiryDate"": {
            ""object"": ""CalculationResult"",
            ""path"": ""questions.ratingSecondary.policyEndDateFormatted""
        },
        ""totalPremium"": {
            ""object"": ""CalculationResult"",
            ""path"": ""payment.total.premium""
        },
        ""contactAddressLine1"": {
            ""object"": ""CalculationResult"",
            ""path"": ""questions.personal.residentialAddress""
        },
        ""contactAddressSuburb"": {
            ""object"": ""CalculationResult"",
            ""path"": ""questions.personal.residentialTown""
        },
        ""contactAddressPostcode"": {
            ""object"": ""CalculationResult"",
            ""path"": ""questions.personal.residentialPostcode""
        },
        ""contactAddressState"": {
            ""object"": ""CalculationResult"",
            ""path"": ""questions.personal.residentialState""
        }
    }
}";

            // Act
            var configurationModel = FundingConfigurationParser.Parse(json);

            // Assert
            configurationModel.Should().BeOfType<IqumulateConfigurationModel>();
            var iqumulateConfigurationModel = configurationModel as IqumulateConfigurationModel;
            Assert.Equal("testScriptUrl", iqumulateConfigurationModel.Default.BaseUrl);
            Assert.Equal("testPaymentMethod", iqumulateConfigurationModel.Default.PaymentMethod);
            Assert.Equal("testAffinitySchemeCode", iqumulateConfigurationModel.Default.AffinitySchemeCode);
            Assert.Equal("testIntroducerContactEmail", iqumulateConfigurationModel.Default.IntroducerContactEmail);
            Assert.Equal("testPolicyClassCode", iqumulateConfigurationModel.Default.PolicyClassCode);
            Assert.Equal("testPolicyUnderwriterCode", iqumulateConfigurationModel.Default.PolicyUnderwriterCode);
        }
    }
}

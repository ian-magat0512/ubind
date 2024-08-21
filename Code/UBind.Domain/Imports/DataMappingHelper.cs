// <copyright file="DataMappingHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Imports
{
    using System;
    using Newtonsoft.Json.Linq;

    public class DataMappingHelper
    {
        private static readonly string DefaultFormDataMapping = @"{ ""formModel"": { ""stateACT"": ""StateACT"", ""stateNSW"": ""StateNSW"", ""stateVIC"": ""StateVIC"", ""stateQLD"": ""StateQLD"", ""stateSA"": ""StateSA"", ""stateWA"": ""StateWA"", ""stateTAS"": ""StateTAS"", ""stateNT"": ""StateNT"", ""stateOverseas"": ""stateOverseas"", ""policyStartDate"": ""PolicyStartDate"", ""policyAdjustmentDate"": ""PolicyAdjustmentDate"", ""policyEndDate"": ""PolicyEndDate"", ""contactName"": ""ContactName"", ""contactEmail"": ""ContactEmail"", ""contactPhone"": ""ContactPhone"", ""contactMobile"": ""ContactMobile"", ""businessAddress"": ""BusinessAddress"", ""businessTown"": ""BusinessTown"", ""businessState"": ""BusinessState"", ""businessPostcode"": ""BusinessPostcode"", ""postalAddress"": ""PostalAddress"", ""postalTown"": ""PostalTown"", ""postalState"": ""PostalState"", ""postalPostcode"": ""PostalPostcode"", ""policyInceptionDate"": ""PolicyInceptionDate"", ""policyExpiryDate"": ""PolicyExpiryDate"", ""finalPremium"": ""FinalPremium"", ""adjustedBrokerFee"": ""AdjustedBrokerFee"" } }";
        private static readonly string DefaultCalculationResultMapping = @"{ ""state"": ""State"", ""questions"": { ""ratingPrimary"": { ""policyInceptionDate"": ""PolicyInceptionDate"", ""stateACT"": ""StateACT"", ""stateNSW"": ""StateNSW"", ""stateVIC"": ""StateVIC"", ""stateQLD"": ""StateQLD"", ""stateSA"": ""StateSA"", ""stateWA"": ""StateWA"", ""stateTAS"": ""StateTAS"", ""stateNT"": ""StateNT"" }, ""ratingSecondary"": { ""policyExpiryDate"": ""PolicyExpiryDate"" }, ""contact"": { ""contactName"": ""ContactName"", ""contactEmail"": ""ContactEmail"", ""contactPhone"": ""ContactPhone"", ""contactMobile"": ""ContactMobile"" }, ""details"": { ""insuredFullName"": ""InsuredFullName"" }, ""businessDetails"": { ""businessAddress"": ""BusinessAddress"", ""businessTown"": ""BusinessTown"", ""businessState"": ""BusinessState"", ""businessPostcode"": ""BusinessPostcode"", ""postalAddress"": ""PostalAddress"", ""postalTown"": ""PostalTown"", ""postalState"": ""PostalState"", ""postalPostcode"": ""PostalPostcode"" } }, ""risk1"": { ""settings"": { ""riskName"": ""RiskName"", ""fieldCodePrefix"": ""FieldCodePrefix"" }, ""ratingFactors"": { ""startup"": ""Startup"", ""professionalIndemnityLimit"": ""ProfessionalIndemnityLimit"", ""actualRevenueLastYear"": ""ActualRevenueLastYear"", ""projectedRevenueNextYear"": ""ProjectedRevenueNextYear"", ""member"": ""IsMember"", ""stateACT"": ""StateACT"", ""stateNSW"": ""StateNSW"", ""stateNT"": ""StateNT"", ""stateQLD"": ""StateQLD"", ""stateSA"": ""StateSA"", ""stateTAS"": ""StateTAS"", ""stateVIC"": ""StateVIC"", ""stateWA"": ""StateWA"", ""stateOverseas"": ""StateOverseas"", ""NSWStampDutyExemption"": ""NSWStampDutyExemption"", ""policyStartDate"": ""PolicyStartDate"", ""endorsementPILoadingFactor"": ""EndorsementPILoadingFactor"", ""endorsementPIAdditionalPremium"": ""EndorsementPIAdditionalPremium"" }, ""checks"": { ""hasActiveEndorsementTrigger"": ""HasActiveEndorsementTrigger"", ""includeRisk"": ""IncludeRisk"" }, ""premium"": { ""applicableRevenue"": ""ApplicableRevenue"", ""premiumForMembers"": ""PremiumForMembers"", ""premiumForNonMembers"": ""PremiumForNonMembers"", ""basePremium"": ""BasePremium"", ""applicableLoadingFactor"": ""ApplicableLoadingFactor"", ""applicableAdditionalPremium"": ""ApplicableAdditionalPremium"", ""loadedPremium"": ""LoadedPremium"", ""premiumPercentACT"": ""PremiumPercentACT"", ""premiumPercentNSW"": ""PremiumPercentNSW"", ""premiumPercentNT"": ""PremiumPercentNT"", ""premiumPercentQLD"": ""PremiumPercentQLD"", ""premiumPercentSA"": ""PremiumPercentSA"", ""premiumPercentTAS"": ""PremiumPercentTAS"", ""premiumPercentVIC"": ""PremiumPercentVIC"", ""premiumPercentWA"": ""PremiumPercentWA"" }, ""statutoryRates"": { ""riskType"": ""RiskType"", ""ESLApplicability"": ""ESLApplicability"", ""NSWStampDutyExemption"": ""NSWStampDutyExemption"", ""jurisdiction"": ""Jurisdiction"", ""policyStartDate"": ""PolicyStartDate"", ""ESLRate"": ""ESLRate"", ""GSTRate"": ""GSTRate"", ""SDRateACT"": ""SDRateACT"", ""SDRateNSW"": ""SDRateNSW"", ""SDRateNT"": ""SDRateNT"", ""SDRateQLD"": ""SDRateQLD"", ""SDRateSA"": ""SDRateSA"", ""SDRateTAS"": ""SDRateTAS"", ""SDRateVIC"": ""SDRateVIC"", ""SDRateWA"": ""SDRateWA"" }, ""payment"": { ""premium"": ""Premium"", ""ESL"": ""ESL"", ""GST"": ""GST"", ""SDACT"": ""SDACT"", ""SDNSW"": ""SDNSW"", ""SDNT"": ""SDNT"", ""SDQLD"": ""SDQLD"", ""SDSA"": ""SDSA"", ""SDTAS"": ""SDTAS"", ""SDVIC"": ""SDVIC"", ""SDWA"": ""SDWA"", ""total"": ""Total"" } }, ""risk2"": { ""settings"": { ""riskName"": ""RiskName"", ""fieldCodePrefix"": ""FieldCodePrefix"" }, ""ratingFactors"": { ""publicLiabilityLimit"": ""PublicLiabilityLimit"", ""projectedRevenueNextYear"": ""ProjectedRevenueNextYear"", ""member"": ""IsMember"", ""stateACT"": ""StateACT"", ""stateNSW"": ""StateNSW"", ""stateNT"": ""StateNT"", ""stateQLD"": ""StateQLD"", ""stateSA"": ""StateSA"", ""stateTAS"": ""StateTAS"", ""stateVIC"": ""StateVIC"", ""stateWA"": ""StateWA"", ""stateOverseas"": ""StateOverseas"", ""NSWStampDutyExemption"": ""NSWStampDutyExemption"", ""policyStartDate"": ""PolicyStartDate"", ""endorsementPLLoadingFactor"": ""EndorsementPLLoadingFactor"", ""endorsementPLAdditionalPremium"": ""EndorsementPLAdditionalPremium"" }, ""checks"": { ""hasActiveEndorsementTrigger"": ""HasActiveEndorsementTrigger"", ""includeRisk"": ""IncludeRisk"" }, ""premium"": { ""premiumForMembers"": ""PremiumForMembers"", ""premiumForNonMembers"": ""PremiumForNonMembers"", ""basePremium"": ""BasePremium"", ""applicableLoadingFactor"": ""ApplicableLoadingFactor"", ""applicableAdditionalPremium"": ""ApplicableAdditionalPremium"", ""loadedPremium"": ""LoadedPremium"", ""premiumPercentACT"": ""PremiumPercentACT"", ""premiumPercentNSW"": ""PremiumPercentNSW"", ""premiumPercentNT"": ""PremiumPercentNT"", ""premiumPercentQLD"": ""PremiumPercentQLD"", ""premiumPercentSA"": ""PremiumPercentSA"", ""premiumPercentTAS"": ""PremiumPercentTAS"", ""premiumPercentVIC"": ""PremiumPercentVIC"", ""premiumPercentWA"": ""PremiumPercentWA"" }, ""statutoryRates"": { ""riskType"": ""RiskType"", ""ESLApplicability"": ""ESLApplicability"", ""NSWStampDutyExemption"": ""NSWStampDutyExemption"", ""jurisdiction"": ""Jurisdiction"", ""policyStartDate"": ""PolicyStartDate"", ""ESLRate"": ""ESLRate"", ""GSTRate"": ""GSTRate"", ""SDRateACT"": ""SDRateACT"", ""SDRateNSW"": ""SDRateNSW"", ""SDRateNT"": ""SDRateNT"", ""SDRateQLD"": ""SDRateQLD"", ""SDRateSA"": ""SDRateSA"", ""SDRateTAS"": ""SDRateTAS"", ""SDRateVIC"": ""SDRateVIC"", ""SDRateWA"": ""SDRateWA"" }, ""payment"": { ""premium"": ""Premium"", ""ESL"": ""ESL"", ""GST"": ""GST"", ""SDACT"": ""SDACT"", ""SDNSW"": ""SDNSW"", ""SDNT"": ""SDNT"", ""SDQLD"": ""SDQLD"", ""SDSA"": ""SDSA"", ""SDTAS"": ""SDTAS"", ""SDVIC"": ""SDVIC"", ""SDWA"": ""SDWA"", ""total"": ""Total"" } }, ""payment"": { ""outputVersion"": 2, ""priceComponents"": { ""basePremium"": ""BasePremium"", ""ESL"": ""ESL"", ""premiumGST"": ""PremiumGST"", ""stampDutyACT"": ""StampDutyACT"", ""stampDutyNSW"": ""StampDutyNSW"", ""stampDutyNT"": ""StampDutyNT"", ""stampDutyQLD"": ""StampDutyQLD"", ""stampDutySA"": ""StampDutySA"", ""stampDutyTAS"": ""StampDutyTAS"", ""stampDutyVIC"": ""StampDutyVIC"", ""stampDutyWA"": ""StampDutyWA"", ""stampDutyTotal"": ""StampDutyTotal"", ""totalPremium"": ""TotalPremium"", ""commission"": ""Commission"", ""commissionGST"": ""CommissionGST"", ""brokerFee"": ""BrokerFee"", ""brokerFeeGST"": ""BrokerFeeGST"", ""underwriterFee"": ""UnderwriterFee"", ""underwriterFeeGST"": ""UnderwriterFeeGST"", ""interest"": ""Interest"", ""merchantFees"": ""MerchantFees"", ""transactionCosts"": ""TransactionCosts"", ""totalGST"": ""TotalGST"", ""totalPayable"": ""TotalPayable"" }, ""instalments"": { ""instalmentsPerYear"": ""InstallmentsPerYear"", ""instalmentAmount"": ""InstallmentAmount"" } } }";

        public static JToken DefaultFormDataMap
        {
            get
            {
                return JToken.Parse(DefaultFormDataMapping);
            }
        }

        public static JToken DefaultCalculationResultMap
        {
            get
            {
                return JToken.Parse(DefaultCalculationResultMapping);
            }
        }

        /// <summary>
        /// Traverses the a token and invokes the given property action against said token.
        /// </summary>
        /// <param name="node">The token to be checked.</param>
        /// <param name="propertyAction">The action to be invoked.</param>
        public static void WalkNode(JToken node, Action<JProperty> propertyAction = null)
        {
            if (node.Type == JTokenType.Object)
            {
                foreach (JProperty child in node.Children<JProperty>())
                {
                    propertyAction?.Invoke(child);

                    WalkNode(child.Value, propertyAction);
                }
            }
            else if (node.Type == JTokenType.Array)
            {
                foreach (JToken child in node.Children())
                {
                    WalkNode(child, propertyAction);
                }
            }
        }

        /// <summary>
        /// Adds default values to a created calculation result token from import.
        /// </summary>
        /// <param name="calculationResultToken">The calculation result token.</param>
        /// <returns>The calculation result in string format.</returns>
        public static string SetCalculationDefaults(JObject calculationResultToken)
        {
            var hasState = calculationResultToken.SelectToken("state") != null;
            if (!hasState)
            {
                calculationResultToken["state"] = "bindingQuote";
            }

            var hasOutputVersion = calculationResultToken.SelectToken("payment.outputVersion") != null;
            if (!hasOutputVersion)
            {
                var paymentTotal = calculationResultToken.SelectToken("payment.priceComponents");
                if (paymentTotal != null)
                {
                    calculationResultToken["payment"]["outputVersion"] = 2;
                }
                else
                {
                    // default output version.
                    calculationResultToken["payment"]["outputVersion"] = 1;
                }
            }

            var paymentToken = calculationResultToken.SelectToken("payment.priceComponents");
            if (paymentToken != null)
            {
                foreach (JProperty child in paymentToken.Children<JProperty>())
                {
                    if (child.Value == null || child.Value.Type == JTokenType.Null || string.IsNullOrEmpty(child.Value.ToString()))
                    {
                        child.Value = "0";
                    }
                }

                calculationResultToken["payment"]["priceComponents"] = paymentToken;
            }

            return calculationResultToken.ToString();
        }
    }
}

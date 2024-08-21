// <copyright file="RatingFactorConstants.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Allows enumeration of All Rating factor properties.
    /// </summary>
    public static class RatingFactorConstants
    {
        /// <summary>
        /// Rating factor properties for FormData.
        /// </summary>
        public static class FormData
        {
            public const string GnafId = "gnafId";
            public const string PolicyStartDate = "policyStartDate";
            public const string BusinessType = "businessType";
            public const string AggregateLimit = "aggregateLimit";
            public const string BusinessRevenueLastYear = "businessRevenueLastYear";
            public const string RatingState = "ratingState";
            public const string InsuredName = "insuredName";
            public const string StatementofFactsAgreement = "statementofFactsAgreement";
        }

        /// <summary>
        /// Rating factor properties for Quote.
        /// </summary>
        public static class Quote
        {
            public const string LastPremium = "Last Premium";
            public const string QuoteType = "Quote Type";
            public const string QuoteState = "Quote State";
            public const string Environment = "Environment";
        }

        /// <summary>
        /// Rating factor properties for Claim.
        /// </summary>
        public static class Claim
        {
            /// <summary>
            /// The rating factor for Total Claims Amount property.
            /// </summary>
            public const string TotalClaimsAmount = "Total Claims Amount";
        }

        /// <summary>
        /// Rating factor properties for DVA Perils.
        /// </summary>
        public static class DvaPerils
        {
            /// <summary>
            /// The rating factor property for Ica Zone.
            /// </summary>
            public const string IcaZone = "Ica Zone";

            /// <summary>
            /// The rating factor property for Gnaf.
            /// </summary>
            public const string GnafId = "Gnaf Id";

            /// <summary>
            /// The rating factor for Peril Storm property.
            /// </summary>
            public const string StormRate = "DSH Underwriter Peril Storm";

            /// <summary>
            /// The rating factor for Peril Bushfire property.
            /// </summary>
            public const string BushfireRate = "DSH Underwriter Peril Bushfire";

            /// <summary>
            /// The rating factor for Peril Earthquake property.
            /// </summary>
            public const string EarthquakeRate = "DSH Underwriter Peril Earthquake";

            /// <summary>
            /// The rating factor for Peril Flood property.
            /// </summary>
            public const string FloodRate = "DSH Underwriter Peril Flood";

            /// <summary>
            /// The rating factor for Peril Cyclone property.
            /// </summary>
            public const string CycloneRate = "DSH Underwriter Peril Cyclone";
        }

        /// <summary>
        /// Rating factor properties for KBI/Cyber-Insurance.
        /// </summary>
        public static class KBICyberInsurance
        {
            public const string AgileQuoteId = "agileQuoteId";
            public const string Premium = "Premium";
            public const string PremiumSalesTax = "Premium Sales Tax";
            public const string StampDuty = "Stamp Duty";
            public const string FireServicesLevy = "Fire Services Levy";
            public const string FireServicesLevySalesTax = "Fire Services Levy Sales Tax";
            public const string AdminFee = "Admin Fee";
            public const string BrokerFee = "Broker fee";
            public const string AdminFeeSalesTax = "Admin Fee Sales Tax";
            public const string BrokerFeeSalesTax = "Broker Fee Sales Tax";
            public const string GrandTotal = "Grand Total";
            public const string TotalSalesTax = "Total Sales Tax";
            public const string ThirdPartyFeeDescription = "Third Party Fee Description";
            public const string ThirdPartyFeeSalesTax = "Third Party Fee Sales Tax";
            public const string ThirdPartyFee = "Third Party Fee";
            public const string SalesTaxLabel = "Sales Tax Label";
            public const string Currency = "Currency";
            public const string Excess = "Excess";
            public const string ExpiresAt = "Expires At";
        }
    }
}

// <copyright file="FundingApplicationMapper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765


////namespace UBind.Application.Funding
////{
////    using System;
////    using System.Collections.Generic;
////    using System.Linq;
////    using System.Text;
////    using System.Threading.Tasks;
////    using NodaTime;
////    using UBind.Application.Funding.PremiumFunding;
////    using UBind.Domain;
////    using UBind.Domain.Aggregates.Quote;
////    using UBind.Domain.Extensions;

////    /// <summary>
////    /// For Mapping application data to a Funding Application.
////    /// </summary>////    public class FundingApplicationMapper
////    {
////        /// <summary>
////        /// Create a new Funding Application, using data from a given application and calculation result.
////        /// </summary>////        /// <param name="quote">The application to use data from.</param>
////        /// <param name="calculationResultId">The calculation result to use data from.</param>
////        /// <returns>A new funding application.</returns>
////        public FundingApplication CreateFundingApplication(QuoteAggregate quote, Guid calculationResultId)
////        {
////            var quoteData = quote.GetQuoteData(calculationResultId);
////            var fundingApplication = new FundingApplication(
////                quoteData.InsuredName,
////                quoteData.TotalPremium,
////                ContractType.Commercial,
////                PaymentFrequency.Monthly,
////                SystemClock.Instance.Now().InZone(Timezones.AET).Date,
////                SettlementDays.Fifteen,
////                12,
////                "Foo Towers",
////                "Fooville",
////                4444,
////                State.VIC,
////                "0312341234",
////                "0412341234",
////                PaymentType.CreditCard,
////                "Foo Corp",
////                "POL001",
////                12,
////                "Foo");

////            return fundingApplication;
////        }
////    }
////}

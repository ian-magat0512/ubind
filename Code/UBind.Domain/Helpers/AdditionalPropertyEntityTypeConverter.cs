// <copyright file="AdditionalPropertyEntityTypeConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using Humanizer;
    using UBind.Domain.Enums;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// Converts type enums to the AdditionalPropertyEntityType enum value.
    /// </summary>
    public class AdditionalPropertyEntityTypeConverter
    {
        public static AdditionalPropertyEntityType FromQuoteType(QuoteType quoteType)
        {
            switch (quoteType)
            {
                case QuoteType.NewBusiness:
                    return AdditionalPropertyEntityType.NewBusinessQuote;
                case QuoteType.Adjustment:
                    return AdditionalPropertyEntityType.AdjustmentQuote;
                case QuoteType.Renewal:
                    return AdditionalPropertyEntityType.RenewalQuote;
                case QuoteType.Cancellation:
                    return AdditionalPropertyEntityType.CancellationQuote;
                default:
                    throw new InvalidOperationException("When converting from QuoteType to "
                        + "AdditionalPropertyEntityType, we came across an unknown QuoteType value: "
                        + $"\"{quoteType.Humanize()}\"");
            }
        }

        public static AdditionalPropertyEntityType FromPolicyTransaction(PolicyTransaction transaction)
        {
            var result = transaction is NewBusinessTransaction
                ? AdditionalPropertyEntityType.NewBusinessPolicyTransaction
                : transaction is AdjustmentTransaction
                    ? AdditionalPropertyEntityType.NewBusinessPolicyTransaction
                    : transaction is RenewalTransaction
                        ? AdditionalPropertyEntityType.RenewalPolicyTransaction
                        : transaction is CancellationTransaction
                            ? AdditionalPropertyEntityType.CancellationPolicyTransaction
                            : AdditionalPropertyEntityType.None;

            if (result == AdditionalPropertyEntityType.None)
            {
                throw new InvalidOperationException("When converting from a PolicyTransaction type to "
                    + "AdditionalPropertyEntityType, we came across an unknown PolicyTransaction type: "
                    + $"\"{transaction.GetType().Name}\"");
            }

            return result;
        }
    }
}

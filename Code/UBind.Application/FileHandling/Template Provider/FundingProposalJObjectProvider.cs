// <copyright file="FundingProposalJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Payment template JSON object provider.
    /// </summary>
    public class FundingProposalJObjectProvider : IJObjectProvider
    {
        /// <inheritdoc/>
        public JObject JsonObject { get; private set; } = new JObject();

        /// <inheritdoc/>
        public Task CreateJsonObject(ApplicationEvent applicationEvent)
        {
            dynamic jsonObject = new JObject();
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            var fundingProposal = quote.AcceptedProposal;
            if (fundingProposal != null)
            {
                jsonObject.FundingProposalId = fundingProposal.ExternalId;
                jsonObject.FundingProposalAmountFunded = fundingProposal.PaymentBreakdown.AmountFunded.ToDollarsAndCents();
                jsonObject.FundingProposalPaymentFrequency = fundingProposal.PaymentBreakdown.PaymentFrequency.ToString();
                jsonObject.FundingProposalNumberOfInstalments = fundingProposal.PaymentBreakdown.NumberOfInstalments;
                jsonObject.FundingProposalInitialInstalmentAmount = fundingProposal.PaymentBreakdown.InitialInstalmentAmount.ToDollarsAndCents();
                jsonObject.FundingProposalRegularInstalmentAmount = fundingProposal.PaymentBreakdown.RegularInstalmentAmount.ToDollarsAndCents();
            }

            IJsonObjectParser parser
                = new GenericJObjectParser(string.Empty, jsonObject);
            if (parser.JsonObject != null)
            {
                this.JsonObject = parser.JsonObject;
            }

            return Task.CompletedTask;
        }
    }
}

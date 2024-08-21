// <copyright file="IqumulateRequestModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using Humanizer;
    using Newtonsoft.Json;
    using UBind.Application.Funding.Iqumulate;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// For returning data to the client so it can request IQumulate premium funding.
    /// </summary>
    public class IqumulateRequestModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IqumulateRequestModel"/> class.
        /// </summary>
        /// <param name="data">The request data to use.</param>
        public IqumulateRequestModel(IqumulateRequestData data)
        {
            if (data.QuoteData.Policies[0].PolicyInceptionDate == null)
            {
                throw new ErrorException(Errors.Payment.Funding.FurtherInformationRequired("policy inception date", data.IsMutual));
            }

            if (data.QuoteData.Policies[0].PolicyExpiryDate == null)
            {
                throw new ErrorException(Errors.Payment.Funding.FurtherInformationRequired("policy expiry date", data.IsMutual));
            }

            data.QuoteData.General.Region = data.QuoteData.General.Region ?? UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.Region.Australia.Humanize();
            data.QuoteData.General.PaymentFrequency = data.QuoteData.General.PaymentFrequency ?? UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.PaymentFrequency.Monthly.Humanize();
            data.QuoteData.General.PaymentMethod = data.QuoteData.General.PaymentMethod ?? data.Configuration.PaymentMethod;
            data.QuoteData.Introducer.AffinitySchemeCode = data.QuoteData.Introducer.AffinitySchemeCode ?? data.Configuration.AffinitySchemeCode;
            data.QuoteData.Policies[0].PolicyClassCode = data.QuoteData.Policies[0].PolicyClassCode ?? data.Configuration.PolicyClassCode;
            data.QuoteData.Policies[0].PolicyUnderwriterCode = data.QuoteData.Policies[0].PolicyUnderwriterCode ?? data.Configuration.PolicyUnderwriterCode;
            data.QuoteData.Introducer.IntroducerContactEmail = data.QuoteData.Introducer.IntroducerContactEmail ?? data.Configuration.IntroducerContactEmail;
            data.QuoteData.Client.TelephoneNumber = data.QuoteData.Client.TelephoneNumber ?? data.TelephoneNumber;
            data.QuoteData.Client.MobileNumber = data.QuoteData.Client.MobileNumber ?? data.MobileNumber;
            data.QuoteData.Client.Email = data.QuoteData.Client.Email ?? data.CustomerEmail;

            // Since we don't have a policy number until payment is made, we're going to use the quote number instead
            data.QuoteData.Policies[0].PolicyNumber = data.QuoteData.Policies[0].PolicyNumber ?? data.QuoteNumber;

            this.BaseUrl = data.Configuration.BaseUrl;
            this.ActionUrl = data.Configuration.ActionUrl;
            this.MessageOriginUrl = data.Configuration.MessageOriginUrl;
            this.QuoteNumber = data.QuoteNumber;
            this.AcceptanceConfirmationField = data.Configuration.AcceptanceConfirmationField;
            this.IQumulateRequestData = data.QuoteData;
        }

        /// <summary>
        /// Gets the Base url.
        /// </summary>
        public string BaseUrl { get; private set; }

        /// <summary>
        /// Gets the Action Url.
        /// </summary>
        public string ActionUrl { get; private set; }

        /// <summary>
        /// Gets the Message origin URL.
        /// </summary>
        public string MessageOriginUrl { get; private set; }

        /// <summary>
        /// Gets the quotue number (which we are using instead of policy number!).
        /// </summary>
        [JsonProperty]
        public string QuoteNumber { get; private set; }

        /// <summary>
        /// Gets the Acceptance Confirmation Field to be set in the client code.
        /// </summary>
        [JsonProperty]
        public string AcceptanceConfirmationField { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IQumulateQuoteData IQumulateRequestData { get; private set; }
    }
}

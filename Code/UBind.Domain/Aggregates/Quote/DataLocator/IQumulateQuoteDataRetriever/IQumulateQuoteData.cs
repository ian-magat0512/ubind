// <copyright file="IQumulateQuoteData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.Iqumulate
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents data which can be pulled from quote data and consumed by IQumulate for the purpose
    /// of seeding an IQumulate Premium Funding application.
    /// </summary>
    public class IQumulateQuoteData
    {
        /// <summary>
        /// Gets or sets General data for IQumulate requests.
        /// </summary>
        [JsonProperty]
        public Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.GeneralData General { get; set; }

        /// <summary>
        /// Gets or sets Introducer data for IQumulate requests.
        /// </summary>
        [JsonProperty]
        public Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.IntroducerData Introducer { get; set; }

        /// <summary>
        /// Gets or sets Client data for IQumulate requests.
        /// </summary>
        [JsonProperty]
        public Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.ClientData Client { get; set; }

        /// <summary>
        /// Gets or sets Policies data for IQumulate requests.
        /// </summary>
        [JsonProperty]
        public List<Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.PolicyData> Policies { get; set; }
    }
}

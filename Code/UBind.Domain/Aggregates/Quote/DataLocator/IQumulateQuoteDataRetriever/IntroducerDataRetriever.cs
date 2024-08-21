// <copyright file="IntroducerDataRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using UBind.Domain.Json;

    public class IntroducerDataRetriever : BaseDataRetriever
    {
        /// <inheritdoc/>
        public override object Retrieve(IIQumulateQuoteDatumLocations config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            return new IntroducerData
            {
                AffinitySchemeCode = this.GetDataValue(config.AffinitySchemeCode, formData, calculationData),
                IntroducerContactEmail = this.GetDataValue(config.IntroducerContactEmail, formData, calculationData),
            };
        }
    }
}

// <copyright file="IQumulateQuoteDataRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Json;

    /// <summary>
    /// This class is needed because we need a retriever for all quote data.
    /// </summary>
    public class IQumulateQuoteDataRetriever
    {
        private readonly IIQumulateQuoteDatumLocations? configuration;
        private readonly CachingJObjectWrapper formData;
        private readonly CachingJObjectWrapper calculationData;

        private readonly Dictionary<IQumulateQuoteDataField, BaseDataRetriever> quoteDataRetrievers
            = new Dictionary<IQumulateQuoteDataField, BaseDataRetriever>()
            {
                { IQumulateQuoteDataField.General, new GeneralDataRetriever() },
                { IQumulateQuoteDataField.Introducer, new IntroducerDataRetriever() },
                { IQumulateQuoteDataField.Client, new ClientDataRetriever() },
                { IQumulateQuoteDataField.Policies, new PoliciesDataRetriever() },
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardQuoteDataRetriever"/> class.
        /// </summary>
        /// <param name="configuration">The data location configuration.</param>
        /// <param name="formData">The form data.</param>
        /// <param name="calculationData">The calculation data.</param>
        public IQumulateQuoteDataRetriever(
            IIQumulateQuoteDatumLocations? configuration,
            CachingJObjectWrapper formData,
            CachingJObjectWrapper calculationData)
        {
            this.configuration = configuration;
            this.formData = formData;
            this.calculationData = calculationData;
        }

        /// <summary>
        /// Method for retrieving quote data and convert it to specified data type.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="quoteData">The quote data.</param>
        /// <returns>The quote data value.</returns>
        public T Retrieve<T>(IQumulateQuoteDataField quoteData)
        {
            var dataRetriever = this.quoteDataRetrievers[quoteData];
            var value = dataRetriever.Retrieve(this.configuration, this.formData, this.calculationData);
            if (value == null)
            {
                return default(T);
            }

            var t = typeof(T);
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                t = Nullable.GetUnderlyingType(t);
            }

            return (T)Convert.ChangeType(value, t);
        }

        /// <summary>
        /// Method for retrieving quote data.
        /// </summary>
        /// <param name="quoteData">The quote data.</param>
        /// <returns>The quote data value in string.</returns>
        public string Retrieve(IQumulateQuoteDataField quoteData)
        {
            return this.Retrieve<string>(quoteData);
        }
    }
}

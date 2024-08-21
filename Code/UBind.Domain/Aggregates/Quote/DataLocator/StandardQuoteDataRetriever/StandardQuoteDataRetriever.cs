// <copyright file="StandardQuoteDataRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever
{
    using System;
    using System.Collections.Generic;
    using Humanizer;
    using UBind.Domain.Aggregates.Quote.DataLocator;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Json;

    /// <summary>
    /// This class is needed because we need a retriever for all quote data.
    /// </summary>
    public class StandardQuoteDataRetriever
    {
        private readonly IDataLocatorConfig configuration;
        private readonly CachingJObjectWrapper formData;
        private readonly CachingJObjectWrapper calculationData;

        private readonly Dictionary<StandardQuoteDataField, BaseFieldRetriever> quoteDataRetrievers
            = new Dictionary<StandardQuoteDataField, BaseFieldRetriever>()
            {
                { StandardQuoteDataField.InsuredName, new InsuredNameFieldRetriever() },
                { StandardQuoteDataField.CustomerName, new CustomerNameFieldRetriever() },
                { StandardQuoteDataField.CustomerEmail, new CustomerEmailFieldRetriever() },
                { StandardQuoteDataField.CustomerMobile, new CustomerMobileFieldRetriever() },
                { StandardQuoteDataField.CustomerPhone, new CustomerPhoneFieldRetriever() },
                { StandardQuoteDataField.InceptionDate, new InceptionDateFieldRetriever() },
                { StandardQuoteDataField.InceptionTime, new InceptionTimeFieldRetriever() },
                { StandardQuoteDataField.EffectiveDate, new EffectiveDateFieldRetriever() },
                { StandardQuoteDataField.CancellationEffectiveDate, new CancellationEffectiveDateFieldRetriever() },
                { StandardQuoteDataField.ExpiryDate, new ExpiryDateFieldRetriever() },
                { StandardQuoteDataField.ExpiryTime, new ExpiryTimeFieldRetriever() },
                { StandardQuoteDataField.CurrencyCode, new CurrencyCodeFieldRetriever() },
                { StandardQuoteDataField.Address, new AddressFieldRetriever() },
                { StandardQuoteDataField.TradingName, new TradingNameFieldRetriever() },
                { StandardQuoteDataField.Abn, new AbnFieldRetriever() },
                { StandardQuoteDataField.NumberOfInstallments, new NumberOfInstallmentsFieldRetriever() },
                { StandardQuoteDataField.IsRunOffPolicy, new IsRunOffPolicyFieldRetriever() },
                { StandardQuoteDataField.BusinessEndDate, new BusinessEndDateFieldRetriever() },
                { StandardQuoteDataField.TotalPremium, new TotalPremiumFieldRetriever() },
                { StandardQuoteDataField.QuoteTitle, new QuoteTitleFieldRetriever() },
                { StandardQuoteDataField.IsRefundApproved, new IsRefundApprovedFieldRetriever() },
                { StandardQuoteDataField.SaveCustomerPaymentDetails, new SaveCustomerPaymentDetailFieldRetriever() },
                { StandardQuoteDataField.UseSavedPaymentMethod, new UseSavedPaymentMethodFieldRetriever() },
                { StandardQuoteDataField.PaymentMethod, new PaymentMethodRetriever() },
                { StandardQuoteDataField.DebitOption, new DebitOptionRetriever() },
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardQuoteDataRetriever"/> class.
        /// </summary>
        /// <param name="configuration">The data location configuration.</param>
        /// <param name="formData">The form data.</param>
        /// <param name="calculationData">The calculation data.</param>
        public StandardQuoteDataRetriever(
            IDataLocatorConfig configuration,
            CachingJObjectWrapper formData,
            CachingJObjectWrapper calculationData)
        {
            this.configuration = configuration;
            this.formData = formData;
            this.calculationData = calculationData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardQuoteDataRetriever"/> class.
        /// </summary>
        /// <param name="formData">The form data.</param>
        /// <param name="calculationData">The calculation data.</param>
        public StandardQuoteDataRetriever(CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            this.configuration = new DefaultProductConfiguration();
            this.formData = formData;
            this.calculationData = calculationData;
        }

        /// <summary>
        /// Method for retrieving quote data and convert it to specified data type.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="quoteData">The quote data.</param>
        /// <returns>The quote data value.</returns>
        public T? Retrieve<T>(StandardQuoteDataField quoteData)
        {
            var dataRetriever = this.quoteDataRetrievers[quoteData];
            var value = dataRetriever.Retrieve(this.configuration, this.formData, this.calculationData);
            if (value == null)
            {
                return default;
            }

            var t = typeof(T);
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                t = Nullable.GetUnderlyingType(t);
            }
            if (t == null)
            {
                return default(T);
            }

            try
            {
                return (T)Convert.ChangeType(value, t);
            }
            catch (InvalidCastException)
            {
                var stringValue = value.ToString();
                return (T?)Convert.ChangeType(stringValue, t);
            }
            catch (Exception exception)
            {
                throw new ErrorException(Errors.General.BadRequest($"Failed to retrieve Quote Data with field name '${quoteData.Humanize().Pascalize()}'"), exception);
            }
        }

        /// <summary>
        /// Method for retrieving quote data.
        /// </summary>
        /// <param name="quoteData">The quote data.</param>
        /// <returns>The quote data value in string.</returns>
        public string? Retrieve(StandardQuoteDataField quoteData)
        {
            return this.Retrieve<string>(quoteData);
        }
    }
}

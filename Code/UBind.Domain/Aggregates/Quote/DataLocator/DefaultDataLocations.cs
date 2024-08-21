// <copyright file="DefaultDataLocations.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator
{
    using System.Collections.Generic;

    /// <summary>
    /// Default configuration for data locations.
    /// </summary>
    public class DefaultDataLocations : DataLocators
    {
        private static DefaultDataLocations instance;

        private DefaultDataLocations()
        {
            this.InsuredName = new List<DataLocation>() { new DataLocation(DataSource.FormData, "insuredFullName") };
            this.TotalPremium = new List<DataLocation>() { new DataLocation(DataSource.Calculation, "payment.total.premium") };
            this.CustomerName = new List<DataLocation>() { new DataLocation(DataSource.FormData, "contactName") };
            this.CustomerEmail = new List<DataLocation>() { new DataLocation(DataSource.FormData, "contactEmail") };
            this.CustomerMobile = new List<DataLocation>() { new DataLocation(DataSource.FormData, "contactMobile") };
            this.CustomerPhone = new List<DataLocation>() { new DataLocation(DataSource.FormData, "contactPhone") };
            this.CurrencyCode = new List<DataLocation>() { new DataLocation(DataSource.Calculation, "payment.currencyCode") };

            this.ContactAddressLine1 = new List<DataLocation>() { new DataLocation(DataSource.FormData, "contactAddressLine1") };
            this.ContactAddressSuburb = new List<DataLocation>() { new DataLocation(DataSource.FormData, "contactAddressSuburb") };
            this.ContactAddressState = new List<DataLocation>() { new DataLocation(DataSource.FormData, "contactAddressState") };
            this.ContactAddressPostcode = new List<DataLocation>() { new DataLocation(DataSource.FormData, "contactAddressPostcode") };

            this.TradingName = new List<DataLocation>() { new DataLocation(DataSource.FormData, "tradingName") };
            this.Abn = new List<DataLocation>() { new DataLocation(DataSource.FormData, "abn") };
            this.NumberOfInstallments = new List<DataLocation>() { new DataLocation(DataSource.FormData, "numberOfInstallments") };
            this.IsRunOffPolicy = new List<DataLocation>() { new DataLocation(DataSource.FormData, "runoffQuestion") };

            this.BusinessEndDate = new List<DataLocation>() { new DataLocation(DataSource.FormData, "businessEndDate") };
            this.CancellationEffectiveDate = new List<DataLocation>() { new DataLocation(DataSource.FormData, "cancellationDate") };
            this.IsRefundApproved = new List<DataLocation>() { new DataLocation(DataSource.Calculation, "isRefundApproved") };
            this.EffectiveDate = new List<DataLocation>() { new DataLocation(DataSource.FormData, "effectiveDate") };
            this.ExpiryDate = new List<DataLocation>()
            {
                new DataLocation(DataSource.FormData, "policyEndDate"),
                new DataLocation(DataSource.FormData, "policyExpiryDate"),
                new DataLocation(DataSource.FormData, "expiryDate"),
            };
            this.InceptionDate = new List<DataLocation>()
            {
                new DataLocation(DataSource.FormData, "policyStartDate"),
                new DataLocation(DataSource.FormData, "policyInceptionDate"),
                new DataLocation(DataSource.FormData, "inceptionDate"),
            };
            this.PaymentMethod = new List<DataLocation>() { new DataLocation(DataSource.FormData, "paymentMethod"), };
            this.DebitOption = new List<DataLocation>() { new DataLocation(DataSource.FormData, "debitOption"), };
        }

        /// <summary>
        /// Gets the singleton instance of the default quote data locations.
        /// </summary>
        public static DefaultDataLocations Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DefaultDataLocations();
                }

                return instance;
            }
        }
    }
}

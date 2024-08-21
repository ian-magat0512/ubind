// <copyright file="PoliciesDataRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using System.Collections.Generic;
    using System.Globalization;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;

    /// <summary>
    /// This class is needed to retrieve the abn data from formdata or calculation.
    /// </summary>
    public class PoliciesDataRetriever : BaseDataRetriever
    {
        /// <inheritdoc/>
        public override object Retrieve(IIQumulateQuoteDatumLocations config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            var amount = this.GetDataValue(config.PolicyAmount, formData, calculationData);
            var inceptionDate = this.GetDataValue(config.PolicyInceptionDate, formData, calculationData).TryParseAsLocalDate();
            var expiryDate = this.GetDataValue(config.PolicyExpiryDate, formData, calculationData).TryParseAsLocalDate();
            var policy = new PolicyData
            {
                PolicyNumber = this.GetDataValue(config.PolicyNumber, formData, calculationData),
                InvoiceNumber = this.GetDataValue(config.InvoiceNumber, formData, calculationData),
                PolicyClassCode = this.GetDataValue(config.PolicyClassClode, formData, calculationData),
                PolicyUnderwriterCode = this.GetDataValue(config.PolicyUnderwiterCode, formData, calculationData),
                PolicyInceptionDate = this.LocateDateFormatted(inceptionDate.Value),
                PolicyExpiryDate = this.LocateDateFormatted(expiryDate.Value),
                PolicyAmount = decimal.TryParse(amount, out decimal d) ? d.ToString("0.00") : "0.00",
                DeftReferenceNumber = this.GetDataValue(config.DeftReferenceNumber, formData, calculationData),
            };

            // we currently only support one policy being funded at a time.
            return new List<PolicyData> { policy };
        }

        private string LocateDateFormatted(LocalDate? localDate)
        {
            if (localDate == null)
            {
                return null;
            }

            return ((LocalDate)localDate).ToString("yyyy-MM-dd", CultureInfo.GetCultureInfo("en-AU"));
        }
    }
}

// <copyright file="GeneralDataRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using UBind.Domain.Json;

    public class GeneralDataRetriever : BaseDataRetriever
    {
        /// <inheritdoc/>
        public override object Retrieve(IIQumulateQuoteDatumLocations config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            var commissionRate = this.GetDataValue(config.CommissionRate, formData, calculationData);
            var noOfInstallments = this.GetDataValue(config.NumberOfInstalments, formData, calculationData);
            var settlementDays = this.GetDataValue(config.SettlementDays, formData, calculationData);
            return new GeneralData()
            {
                Region = this.GetDataValue(config.Region, formData, calculationData),
                FirstInstalmentDate = this.GetDataValue(config.FirstInstalmentDate, formData, calculationData),
                PaymentFrequency = this.GetDataValue(config.PaymentFrequency, formData, calculationData),
                NumberOfInstalments = int.TryParse(noOfInstallments, out int i) ? i : default(int?),
                CommissionRate = decimal.TryParse(commissionRate, out decimal d) ? d : default(decimal?),
                SettlementDays = int.TryParse(settlementDays, out int s) ? s : default(int?),
                PaymentMethod = this.GetDataValue(config.PaymentMethod, formData, calculationData),
            };
        }
    }
}

// <copyright file="PremiumFundingContractData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.PremiumFunding
{
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// For holding contract data used in a proposal.
    /// </summary>
    public class PremiumFundingContractData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PremiumFundingContractData"/> class.
        /// </summary>
        /// <param name="quoteDataRetriever">The quote data retriever.</param>
        /// <param name="priceBreakdown">The price breakdown.</param>
        /// <param name="configuration">The premium funding configuration settings.</param>
        /// <param name="isMutual">identifier if its mutual.</param>
        /// <param name="clock">A clock for default values.</param>
        public PremiumFundingContractData(
            StandardQuoteDataRetriever quoteDataRetriever,
            PriceBreakdown priceBreakdown,
            IPremiumFundingConfiguration configuration,
            bool isMutual,
            IClock clock)
        {
            var customerName = quoteDataRetriever.Retrieve<string>(StandardQuoteDataField.CustomerName);
            var customerMobile = quoteDataRetriever.Retrieve<string>(StandardQuoteDataField.CustomerMobile);
            var customerPhone = quoteDataRetriever.Retrieve<string>(StandardQuoteDataField.CustomerPhone);
            var customerEmailAddress = quoteDataRetriever.Retrieve<string>(StandardQuoteDataField.CustomerEmail);
            var totalPremium = quoteDataRetriever.Retrieve<decimal?>(StandardQuoteDataField.TotalPremium);
            var address = quoteDataRetriever.Retrieve<Address>(StandardQuoteDataField.Address);
            var inceptionDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.InceptionDate);
            if (!totalPremium.HasValue)
            {
                throw new ErrorException(Errors.Payment.Funding.TotalPayableAmountMissing(isMutual));
            }

            var defaultPhoneNumber = customerMobile ?? customerPhone ?? "0412345678";

            // TODO: Confirm whether placeholders are required for address and phone.
            this.UsesPlaceholderData = !inceptionDate.HasValue
                || (customerName == null)
                || (customerMobile == null)
                || (address.Line1 == null)
                || (address.Suburb == null)
                || (address.Postcode == null)
                || (address.State == State.Unspecified);

            this.CustomerFullName = customerName ?? "Anonymous";
            this.CustomerEmail = customerEmailAddress ?? "no-email-address@provided.com";
            this.TotalPremiumAmount = priceBreakdown.TotalPayable;
            this.ContractType = configuration.ContractType;
            this.PaymentFrequency = configuration.PaymentFrequency;
            this.NumberOfMonths = configuration.NumberOfMonths;
            this.InceptionDate = inceptionDate.HasValue
                ? inceptionDate.Value
                : clock.Now().InZone(Timezones.AET).Date;
            this.SettlementDays = configuration.SettlementDays;
            this.AddressLine1 = address.Line1 ?? "Address not specified";
            this.Suburb = address.Suburb ?? "Suburb not specified";
            this.Postcode = address.Postcode ?? "0";
            this.State = address.State != State.Unspecified
                ? address.State
                : State.VIC;
            this.HomePhone = customerPhone ?? defaultPhoneNumber;
            this.MobilePhone = customerMobile ?? defaultPhoneNumber;
            this.InsurerName = configuration.InsurerName;
            this.SettlementToName = configuration.SettlementToName;
            this.Commission = configuration.Commission;
        }

        [JsonConstructor]
        private PremiumFundingContractData()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the contract data uses placeholder data.
        /// </summary>
        /// <remarks>
        /// Using placeholder data that does not affect the pricing allows us to display the price to the use early,
        /// but contracts using placeholder data must be updated before acceptance.
        /// .</remarks>
        [JsonProperty]
        public bool UsesPlaceholderData { get; private set; }

        [JsonProperty]
        private string CustomerFullName { get; set; }

        [JsonProperty]
        private string CustomerEmail { get; set; }

        [JsonProperty]
        private decimal TotalPremiumAmount { get; set; }

        [JsonProperty]
        private ContractType ContractType { get; set; }

        [JsonProperty]
        private Frequency PaymentFrequency { get; set; }

        [JsonProperty]
        private int NumberOfMonths { get; set; }

        [JsonProperty]
        private LocalDate InceptionDate { get; set; }

        [JsonProperty]
        private LocalDate ExpiryDate { get; set; }

        [JsonProperty]
        private int SettlementDays { get; set; }

        [JsonProperty]
        private string AddressLine1 { get; set; }

        [JsonProperty]
        private string Suburb { get; set; }

        [JsonProperty]
        private State State { get; set; }

        [JsonProperty]
        private string Postcode { get; set; }

        [JsonProperty]
        private string HomePhone { get; set; }

        [JsonProperty]
        private string MobilePhone { get; set; }

        [JsonProperty]
        private string InsurerName { get; set; }

        [JsonProperty]
        private string SettlementToName { get; set; }

        [JsonProperty]
        private decimal Commission { get; set; }

        /// <summary>
        /// Modify the data for testing.
        /// </summary>
        public void UpdateContractDataForTesting()
        {
            this.TotalPremiumAmount = 0.01M;
        }

        /// <summary>
        /// Generate a new instance of <see cref="Contract"/> with the held data.
        /// </summary>
        /// <returns>A new instance of <see cref="Contract"/>.</returns>
        public Contract ToContract()
        {
            var inceptionDateUnixTime = (int)this.InceptionDate
                    .AtStartOfDayInZone(Timezones.AET)
                    .ToInstant()
                    .ToUnixTimeSeconds();

            // TODO: Calculate policy term from dates
            ////var termInMonths = Period.Between(this.InceptionDate, this.ExpiryDate, PeriodUnits.Months);
            var termInMonths = this.NumberOfMonths;
            var contract = new Contract
            {
                ClientName = this.CustomerFullName,
                ClientEmailAddress = this.CustomerEmail,
                TotalPremiumAmount = (double)this.TotalPremiumAmount,
                TypeOfContract = this.ContractType
                    .ConvertByName<ContractType, ContractTypeOfContract>(),
                PaymentFrequency = this.PaymentFrequency
                    .ConvertByName<Frequency, ContractPaymentFrequency>(),
                NumberOfMonths = this.NumberOfMonths,
                InceptionDate = inceptionDateUnixTime,
                SettlementDays = ((SettlementDays)this.SettlementDays)
                    .ConvertByValue<SettlementDays, ContractSettlementDays>(),
                Address = this.AddressLine1,
                Suburb = this.Suburb,
                PostCode = int.Parse(this.Postcode),
                State = this.State.ConvertByName<State, ContractState>(),
                PhoneNumber = this.HomePhone,
                MobileNumber = this.MobilePhone,
                Insurers = new System.Collections.ObjectModel.ObservableCollection<Insurer>
                {
                    new Insurer
                    {
                        Amount = (double)this.TotalPremiumAmount,
                        InceptionDate = inceptionDateUnixTime.ToString(),
                        Name = this.InsurerName,
                        Term = termInMonths,
                    },
                },
                Commission = (double)this.Commission,
            };
            var settlementTo = new SettlementTo
            {
                Name = this.SettlementToName,
                Amount = (double)this.TotalPremiumAmount,
            };
            contract.SettlementTo.Add(settlementTo);
            return contract;
        }
    }
}

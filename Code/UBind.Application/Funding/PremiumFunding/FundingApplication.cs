// <copyright file="FundingApplication.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.PremiumFunding
{
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// A premium funding application.
    /// </summary>
    public class FundingApplication
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingApplication"/> class.
        /// </summary>
        /// <param name="clientName">The client name.</param>
        /// <param name="totalPremiumAmount">The total amount being funded.</param>
        /// <param name="contractType">The contract type.</param>
        /// <param name="paymentFrequency">The payment frequency.</param>
        /// <param name="inceptionDate">The policy inception date.</param>
        /// <param name="settlementDays">The number of days to settlement.</param>
        /// <param name="numberOfMonths">The number of months.</param>
        /// <param name="address">The client address (i.e. first line).</param>
        /// <param name="suburb">The client suburb.</param>
        /// <param name="postcode">The client postcode.</param>
        /// <param name="state">The client state.</param>
        /// <param name="phoneNumber">The client phone number.</param>
        /// <param name="mobileNumber">The client mobile phone number.</param>
        /// <param name="paymentType">The client payment type.</param>
        /// <param name="insurerName">The insurer name.</param>
        /// <param name="policyNumber">The policy number.</param>
        /// <param name="termInMonths">The policy term in months.</param>
        /// <param name="settlementName">???.</param>
        public FundingApplication(
            string clientName,
            double totalPremiumAmount,
            ContractType contractType,
            PaymentFrequency paymentFrequency,
            LocalDate inceptionDate,
            SettlementDays settlementDays,
            int numberOfMonths,
            string address,
            string suburb,
            int postcode,
            State state,
            string phoneNumber,
            string mobileNumber,
            PaymentType paymentType,
            string insurerName,
            string policyNumber,
            int termInMonths,
            string settlementName)
        {
            this.ClientName = clientName;
            this.TotalPremiumAmount = totalPremiumAmount;
            this.ContractType = contractType;
            this.PaymentFrequency = paymentFrequency;
            this.InceptionDate = inceptionDate;
            this.SettlementDays = settlementDays;
            this.NumberOfMonths = numberOfMonths;
            this.Address = address;
            this.Suburb = suburb;
            this.Postcode = postcode;
            this.State = state;
            this.PhoneNumber = phoneNumber;
            this.MobilelNumber = mobileNumber;
            this.PaymentType = paymentType;
            this.InsurerName = insurerName;
            this.PolicyNumber = policyNumber;
            this.TermInMonths = termInMonths;
            this.SettlementName = settlementName;
        }

        /// <summary>
        /// Gets the client name.
        /// </summary>
        [JsonProperty]
        public string ClientName { get; private set; }

        /// <summary>
        /// Gets the total amount being funded.
        /// </summary>
        public double TotalPremiumAmount { get; }

        /// <summary>
        /// Gets the contract type.
        /// </summary>
        public ContractType ContractType { get; }

        /// <summary>
        /// Gets the payment frequency.
        /// </summary>
        public PaymentFrequency PaymentFrequency { get; }

        /// <summary>
        /// Gets the number of months.
        /// </summary>
        public int NumberOfMonths { get; }

        /// <summary>
        /// Gets the policy inception date.
        /// </summary>
        public LocalDate InceptionDate { get; }

        /// <summary>
        /// Gets the number of days until settlement.
        /// </summary>
        public SettlementDays SettlementDays { get; }

        /// <summary>
        /// Gets the client address (i.e. line 1).
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// Gets the client suburb.
        /// </summary>
        public string Suburb { get; }

        /// <summary>
        /// Gets the client postcode.
        /// </summary>
        public int Postcode { get; }

        /// <summary>
        /// Gets the client state.
        /// </summary>
        public State State { get; }

        /// <summary>
        /// Gets the client phone number.
        /// </summary>
        public string PhoneNumber { get; }

        /// <summary>
        /// Gets the client mobile number.
        /// </summary>
        public string MobilelNumber { get; }

        /// <summary>
        /// Gets the payment type (credit card or debit card).
        /// </summary>
        public PaymentType PaymentType { get; }

        /// <summary>
        /// Gets the insurer name.
        /// </summary>
        public string InsurerName { get; }

        /// <summary>
        /// Gets the policy number.
        /// </summary>
        public string PolicyNumber { get; }

        /// <summary>
        /// Gets the term in months.
        /// </summary>
        public int TermInMonths { get; }

        /// <summary>
        /// Gets ???.
        /// </summary>
        public string SettlementName { get; }
    }
}

// <copyright file="IPremiumFundingConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.PremiumFunding
{
    using UBind.Domain.Aggregates.Quote.DataLocator;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Configuration for Premium Funding service usage.
    /// </summary>
    public interface IPremiumFundingConfiguration : IDataLocatorConfig
    {
        /// <summary>
        /// Gets the username.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Gets the API version.
        /// </summary>
        string ApiVersion { get; }

        /// <summary>
        /// Gets the insurer name.
        /// </summary>
        string InsurerName { get; }

        /// <summary>
        /// Gets the contract type. Can be Commercial, Domestic or Strata.
        /// </summary>
        ContractType ContractType { get; }

        /// <summary>
        /// Gets the payment frequency.
        /// </summary>
        Frequency PaymentFrequency { get; }

        /// <summary>
        /// Gets a value indicating when you wish the contract to be settled.
        /// Options: 1: 1-7 Days, 15: 15 Days, 30: 30 Days, 45: 45 Days, 60: 60 Days.
        /// </summary>
        int SettlementDays { get; }

        /// <summary>
        /// Gets the number of months over which the contract is paid back - this determines the number of instalments.
        /// Range: 3 to 12, however the range may be limited by several factors.
        /// </summary>
        int NumberOfMonths { get; }

        /// <summary>
        /// Gets the name of entity to whom the settlement is to be made.
        /// </summary>
        string SettlementToName { get; }

        /// <summary>
        /// Gets the commission percentage to be used.
        /// </summary>
        decimal Commission { get; }
    }
}

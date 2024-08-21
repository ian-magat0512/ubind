// <copyright file="IEFundExpressProductConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.EFundExpress
{
    using UBind.Domain.Aggregates.Quote.DataLocator;

    /// <summary>
    /// Specifies config for using Principal Finance's EFundEpress service.
    /// </summary>
    public interface IEFundExpressProductConfiguration : IDataLocatorConfig
    {
        /// <summary>
        /// Gets broker login id.
        /// </summary>
        string BrokerLoginId { get; }

        /// <summary>
        /// Gets the Policy Class Code.
        /// </summary>
        string PolicyClassCode { get; }

        /// <summary>
        /// Gets the contract type.
        /// </summary>
        ContractType ContractType { get; }

        /// <summary>
        /// Gets a value indicating whether fortnightly installments should be used.
        /// </summary>
        bool FortnightlyInstalments { get; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Gets the name of the service to use (as specified in app.config).
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Gets or underwriter name.
        /// </summary>
        string UnderwriterName { get; }

        /// <summary>
        /// Gets the fixed interest rate for to use for the given client.
        /// </summary>
        decimal FixedInterestRate { get; }

        /// <summary>
        /// Gets the fixed number of installments for a given client.
        /// </summary>
        int NumberOfInstalments { get; }

        /// <summary>
        /// Gets the fee included in the first instalment.
        /// </summary>
        decimal FirstInstalmentFee { get; }

        /// <summary>
        /// Gets the soap client url to be used for the transaction.
        /// </summary>
        string ClientUrl { get; }
    }
}

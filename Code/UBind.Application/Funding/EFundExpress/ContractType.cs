// <copyright file="ContractType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.EFundExpress
{
    /// <summary>
    /// Contract types for eFundExpress.
    /// </summary>
    public enum ContractType
    {
        /// <summary>
        /// Default value - not a valid contract type.
        /// </summary>
        None = 0,

        /// <summary>
        /// All commercial policy types, Funding period between 6 - 12 months.
        /// </summary>
        Commercial,

        /// <summary>
        /// Unknown.
        /// </summary>
        Business,

        /// <summary>
        /// All personal lines policy types, fixed funding period of 12 monthly installments.
        /// </summary>
        PBM,

        /// <summary>
        /// All personal lines policy types, funding period may be designated between 6 - 12 months.
        /// </summary>
        PBMPlus,

        /// <summary>
        /// Unknown.
        /// </summary>
        Endorsement,

        /// <summary>
        /// Unknown.
        /// </summary>
        PBMRenewal,
    }
}

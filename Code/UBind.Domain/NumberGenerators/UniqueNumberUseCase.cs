// <copyright file="UniqueNumberUseCase.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.NumberGenerators
{
    /// <summary>
    /// Specifies a use case a unique number is for.
    /// </summary>
    /// <remarks>
    /// This is used to distinguish unique seed sequences.
    /// Different use cases use their own seed sequence, so seeds are not guarenteed to be unique across use cases.
    /// .</remarks>
    public enum UniqueNumberUseCase
    {
        /// <summary>
        /// Unique numbers are for generating quote numbers.
        /// </summary>
        QuoteNumber = 0,

        /// <summary>
        /// Unique numbers are for generating DEFT customer reference numbers (CRN).
        /// </summary>
        DeftCrn = 1,

        /// <summary>
        /// Unique numbers are for generating claim numbers.
        /// </summary>
        ClaimNumber = 2,

        /// <summary>
        /// Unique numbers are for generating payment reference numbers.
        /// </summary>
        PaymentReferenceNumber = 3,

        /// <summary>
        /// Unique numbers are for generating refund reference numbers.
        /// </summary>
        RefundReferenceNumber = 4,
    }
}

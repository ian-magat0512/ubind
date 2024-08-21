// <copyright file="CrnGenerationMethod.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Deft
{
    /// <summary>
    /// Type of CRN generation to be used.
    /// </summary>
    public enum CrnGenerationMethod
    {
        /// <summary>
        /// CRNs should be generated as a unique 10 digit number.
        /// </summary>
        Unique10DigitNumber = 0,

        /// <summary>
        /// CRNs should be generated using a fixed 4 digit prefix followed by
        /// a unique 6 digit suffix.
        /// </summary>
        /// <remarks>This format is required for WinBEAT reconiliation on PSC.</remarks>
        Fixed4DigitPrefxWithUniqueSixDigitSuffix = 1,
    }
}

// <copyright file="SystemAlertType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Distinguish various alert types.
    /// </summary>
    public enum SystemAlertType
    {
        /// <summary>
        /// Alert for when policy numbers are running low.
        /// </summary>
        PolicyNumbers = 0,

        /// <summary>
        /// Alert for when invoice numbers are running low.
        /// </summary>
        InvoiceNumbers = 1,

        /// <summary>
        /// Alert for when claim numbers are running low.
        /// </summary>
        ClaimNumbers = 2,

        /// <summary>
        /// Alert for when credit note numbers are running low.
        /// </summary>
        CreditNoteNumbers = 3,
    }
}

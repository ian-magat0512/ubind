// <copyright file="ZaiItemState.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Zai
{
    using System.ComponentModel;

    /// <summary>
    /// The different states an <see cref="ZaiEntities.Item"/> can be in.
    /// For a full list, please visit: https://developer.hellozai.com/docs/statuses.
    /// </summary>
    public enum ZaiItemState
    {
        /// <summary>
        /// Transaction has not started yet, no payment received or requested.
        /// </summary>
        Pending = 22000,

        /// <summary>
        /// Payment has been requested by the seller to the buyer.
        /// </summary>
        [Description("payment_required")]
        PaymentRequired = 22100,

        /// <summary>
        /// Direct debit payment has been triggered, awaiting for these funds to clear.
        /// </summary>
        [Description("payment_pending")]
        PaymentPending = 22150,

        /// <summary>
        /// Payment has been held as automatic triggers have been alerted. This will go through a manual review to move on to payment_deposited, or fraud_hold.
        /// </summary>
        [Description("payment_held")]
        PaymentHeld = 22175,

        /// <summary>
        /// A credit card payment has been authorized for capture.
        /// </summary>
        [Description("payment_authorized")]
        PaymentAuthorized = 22180,

        /// <summary>
        /// A previous credit card payment authorization has been voided.
        /// </summary>
        Voided = 22195,

        /// <summary>
        /// Payment has been successfully received in our escrow vault.
        /// </summary>
        [Description("payment_deposited")]
        PaymentDeposited,

        /// <summary>
        /// A dispute has been raised by either the buyer/seller, transaction is on hold until it is resolved.
        /// </summary>
        [Description("problem_flagged")]
        ProblemFlagged,

        /// <summary>
        /// The Item is completed, funds have been released.
        /// </summary>
        Completed,

        /// <summary>
        /// The item is refunded.
        /// </summary>
        [Description("refunded")]
        Refunded,

        /// <summary>
        /// The item is refund pending.
        /// </summary>
        [Description("refund_pending")]
        RefundPending,
    }
}

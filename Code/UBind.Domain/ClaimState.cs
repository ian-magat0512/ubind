// <copyright file="ClaimState.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Status and status type for claims.
    /// </summary>
    public static class ClaimState
    {
        /// <summary>
        /// Gets the name of the nascent status.
        /// </summary>
        public static string Nascent => nameof(ClaimState.Nascent);

        /// <summary>
        /// Gets the name of the incomplete state.
        /// </summary>
        public static string Incomplete => nameof(ClaimState.Incomplete);

        /// <summary>
        /// Gets the name of the appoved state.
        /// </summary>
        public static string Approved => nameof(ClaimState.Approved);

        /// <summary>
        /// Gets the name of the notified state.
        /// </summary>
        public static string Notified => nameof(ClaimState.Notified);

        /// <summary>
        /// Gets the name of the Acknowledged state.
        /// </summary>
        public static string Acknowledged => nameof(ClaimState.Acknowledged);

        /// <summary>
        /// Gets the name of the review state.
        /// </summary>
        public static string Review => nameof(ClaimState.Review);

        /// <summary>
        /// Gets the name of the assessment state.
        /// </summary>
        public static string Assessment => nameof(ClaimState.Assessment);

        /// <summary>
        /// Gets the name of the declined state.
        /// </summary>
        public static string Declined => nameof(ClaimState.Declined);

        /// <summary>
        /// Gets the name of the withdrawn state.
        /// </summary>
        public static string Withdrawn => nameof(ClaimState.Withdrawn);

        /// <summary>
        /// Gets the name of the complete state.
        /// </summary>
        public static string Complete => nameof(ClaimState.Complete);

        /// <summary>
        /// Gets the name of the settled state.
        /// </summary>
        public static string Settled => nameof(ClaimState.Settled);

        /// <summary>
        /// Get the corresponding state for an old claim status.
        /// </summary>
        /// <param name="claimStatus">The old status to map.</param>
        /// <returns>The corresponding new state.</returns>
        public static string FromClaimStatus(LegacyClaimStatus claimStatus)
        {
            var claimState = ClaimState.Incomplete;
            switch (claimStatus)
            {
                case LegacyClaimStatus.Cancelled:
                    claimState = ClaimState.Withdrawn;
                    break;
                case LegacyClaimStatus.Processing:
                case LegacyClaimStatus.New:
                case LegacyClaimStatus.Active:
                case LegacyClaimStatus.None:
                    claimState = ClaimState.Incomplete;
                    break;
                case LegacyClaimStatus.Accepted:
                    claimState = ClaimState.Approved;
                    break;
                case LegacyClaimStatus.Completed:
                case LegacyClaimStatus.CompletedOrCancelled:
                    claimState = ClaimState.Complete;
                    break;
                case LegacyClaimStatus.Rejected:
                    claimState = ClaimState.Declined;
                    break;
                default:
                    break;
            }

            return claimState;
        }
    }
}

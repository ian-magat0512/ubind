// <copyright file="ClaimActions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System.ComponentModel;

    /// <summary>
    /// The set of actions that can be performed on a quote.
    /// </summary>
    public enum ClaimActions
    {
        /// <summary>
        /// Progresses the claim to Incomplete state.
        /// </summary>
        Actualise,

        /// <summary>
        /// Approve the Claim automatically
        /// </summary>
        [Description("AutoApproval")]
        AutoApproval,

        /// <summary>
        /// Notify the claim
        /// </summary>
        Notify,

        /// <summary>
        /// Acknowledge the Claim
        /// </summary>
        [Description("Acknowledge")]
        Acknowledge,

        /// <summary>
        /// Returns the Claim to Incomplete status
        /// </summary>
        Return,

        /// <summary>
        /// Review the Claim
        /// </summary>
        [Description("ReviewReferral")]
        ReviewReferral,

        /// <summary>
        /// Approve the Claim
        /// </summary>
        [Description("ReviewApproval")]
        ReviewApproval,

        /// <summary>
        /// Refer the claim to an approver
        /// </summary>
        [Description("AssessmentReferral")]
        AssessmentReferral,

        /// <summary>
        /// Assessment Approval
        /// </summary>
        [Description("AssessmentApproval")]
        AssessmentApproval,

        /// <summary>
        /// Decline the claim
        /// </summary>
        Decline,

        /// <summary>
        /// Withdraw the claim.
        /// </summary>
        Withdraw,

        /// <summary>
        /// Withdraw the claim.
        /// </summary>
        Settle,
    }
}

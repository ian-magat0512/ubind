// <copyright file="PolicyTransactionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Email
{
    using System;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// policy transaction related to the email.
    /// </summary>
    public class PolicyTransactionData
    {
        /// <summary>
        /// Gets the policy transaction id.
        /// </summary>
        public Guid Id
        {
            get
            {
                return this.TransactionData.Id;
            }
        }

        /// <summary>
        /// Gets the policy id.
        /// </summary>
        public Guid PolicyId
        {
            get
            {
                return this.TransactionData.PolicyId;
            }
        }

        /// <summary>
        /// Gets the owner user id.
        /// </summary>
        public Guid? OwnerUserId
        {
            get
            {
                return this.TransactionData.OwnerUserId;
            }
        }

        /// <summary>
        /// Gets the organisation id.
        /// </summary>
        public Guid OrganisationId
        {
            get
            {
                return this.TransactionData.OrganisationId;
            }
        }

        /// <summary>
        /// Gets the customer id.
        /// </summary>
        public Guid? CustomerId
        {
            get
            {
                return this.TransactionData.CustomerId;
            }
        }

        /// <summary>
        /// Gets or sets the policy transaction data.
        /// </summary>
        public PolicyTransaction TransactionData { get; set; }

        /// <summary>
        /// Gets the policy transaction type (enum - "Purchased", "Adjusted", "Renewed", "Cancelled").
        /// </summary>
        public string TransactionType
        {
            get
            {
                return GetTransactionType(this.TransactionData);
            }
        }

        private static string GetTransactionType(UBind.Domain.ReadModel.Policy.PolicyTransaction transaction)
        {
            switch (transaction)
            {
                case NewBusinessTransaction a:
                    return "Purchase";
                case AdjustmentTransaction b:
                    return "Adjustment";
                case RenewalTransaction c:
                    return "Renewal";
                case CancellationTransaction d:
                    return "Cancellation";
                default:
                    return string.Empty;
            }
        }
    }
}

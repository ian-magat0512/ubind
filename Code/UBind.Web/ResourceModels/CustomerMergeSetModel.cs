// <copyright file="CustomerMergeSetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;

    /// <summary>
    /// Resource model for serving the response of the customer merge result.
    /// </summary>
    public class CustomerMergeSetModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerMergeSetModel"/> class.
        /// </summary>
        public CustomerMergeSetModel(
            string sourceCustomerDisplayName,
            string targetCustomerDisplayName,
            Guid targetCustomerId)
        {
            this.SourceCustomerDisplayName = sourceCustomerDisplayName;
            this.TargetCustomerDisplayName = targetCustomerDisplayName;
            this.TargetCustomerId = targetCustomerId;
        }

        /// <summary>
        /// Gets the display name of the source customer.
        /// </summary>
        public string SourceCustomerDisplayName { get; private set; }

        /// <summary>
        /// Gets the display name of the target customer.
        /// </summary>
        public string TargetCustomerDisplayName { get; private set; }

        /// <summary>
        /// Gets the ID of the target customer record.
        /// </summary>
        public Guid TargetCustomerId { get; private set; }
    }
}

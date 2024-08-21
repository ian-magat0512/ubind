// <copyright file="PolicyData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Email
{
    using System;

    /// <summary>
    /// policy of the IEmailSummary model.
    /// </summary>
    public class PolicyData
    {
        /// <summary>
        /// Gets or sets the policy id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the policy number.
        /// </summary>
        public string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets the owner user id.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the organisation id of the record.
        /// </summary>
        public Guid? OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the customer id of the record.
        /// </summary>
        public Guid? CustomerId { get; set; }
    }
}

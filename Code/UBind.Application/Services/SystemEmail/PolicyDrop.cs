// <copyright file="PolicyDrop.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.SystemEmail
{
    using System;
    using global::DotLiquid;

    /// <summary>
    /// A drop model for Policy.
    /// </summary>
    public class PolicyDrop : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyDrop"/> class.
        /// </summary>
        /// <param name="id">The policy ID.</param>
        /// <param name="number">The policy number.</param>
        /// <param name="expiryDate">The policy expire date.</param>
        public PolicyDrop(Guid id, string number, string expiryDate)
        {
            this.Id = id;
            this.Number = number;
            this.ExpiryDate = expiryDate;
        }

        /// <summary>
        /// Gets the ID of the policy.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the policy number.
        /// </summary>
        public string Number { get; }

        /// <summary>
        /// Gets the policy expire date.
        /// </summary>
        public string ExpiryDate { get; }
    }
}

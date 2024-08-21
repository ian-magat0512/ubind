// <copyright file="CustomerOwnership.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Represents the relationship between a customer and owner.
    /// </summary>
    public class CustomerOwnership : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerOwnership"/> class.
        /// </summary>
        public CustomerOwnership()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerOwnership"/> class.
        /// </summary>
        /// <param name="ownerId">The ID of the customer record's owner (otherwise known as Referrer).</param>
        /// <param name="customerId">The ID of the owned record.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public CustomerOwnership(Guid ownerId, Guid customerId, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.OwnerId = ownerId;
            this.CustomerId = customerId;
        }

        /// <summary>
        /// Gets the ID of the owner being referenced in the relationship, otherwise known as referrer.
        /// </summary>
        public Guid OwnerId { get; private set; }

        /// <summary>
        /// Gets the ID of the customer being referenced in the relationship.
        /// </summary>
        public Guid CustomerId { get; private set; }
    }
}

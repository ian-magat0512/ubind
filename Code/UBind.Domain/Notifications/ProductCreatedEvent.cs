// <copyright file="ProductCreatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Notifications
{
    using System;
    using NodaTime;

    /// <summary>
    /// An event class for adding default values for newly created user.
    /// </summary>
    public class ProductCreatedEvent : DomainEvent
    {
        public ProductCreatedEvent(
            Product.Product product,
            Guid? performingUserId,
            Instant createdTimestamp)
            : base(performingUserId, createdTimestamp)
        {
            this.Product = product;
        }

        /// <summary>
        /// Gets the user aggregate.
        /// </summary>
        public Product.Product Product { get; }
    }
}

// <copyright file="ProductEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product
{
    using System;
    using NodaTime;

    /// <summary>
    /// Represents an event that has occured for a product.
    /// </summary>
    public class ProductEvent : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductEvent"/> class.
        /// </summary>
        /// <param name="type">The type of the event.</param>
        /// <param name="createdTimestamp">The time the.</param>
        public ProductEvent(ProductEventType type, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductEvent"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        private ProductEvent()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public ProductEventType Type { get; private set; }
    }
}

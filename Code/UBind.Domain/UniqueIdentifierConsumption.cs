// <copyright file="UniqueIdentifierConsumption.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// A record of the consumption of a unique identifier.
    /// </summary>
    public class UniqueIdentifierConsumption : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueIdentifierConsumption"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        public UniqueIdentifierConsumption()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueIdentifierConsumption"/> class.
        /// </summary>
        /// <param name="createdTimestamp">The time the identifier is inserted/created.</param>
        public UniqueIdentifierConsumption(Instant createdTimestamp)
            : base(default(Guid), createdTimestamp)
        {
        }

        /// <summary>
        /// Gets the unique identifier that has been consumed.
        /// </summary>
        /// <remarks>Navigation proeprty for EF.</remarks>
        public UniqueIdentifier UniqueIdenitfier { get; private set; }
    }
}

// <copyright file="UniqueIdentifier.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// An identifier for an application or corollary that should be unique for a given product, environment and type,
    /// where type is either application or a type of corollary.
    /// </summary>
    public class UniqueIdentifier : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueIdentifier"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        public UniqueIdentifier()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueIdentifier"/> class.
        /// </summary>
        /// <param name="type">The type of the identifier.</param>
        /// <param name="tenantId">The ID of the tenant which the identifier is for.</param>
        /// <param name="productId">The Id of the product which the identifier is for.</param>
        /// <param name="createdTimestamp">The time the identifier is inserted/created.</param>
        /// <param name="environment">The deployment environment the identifier is for.</param>
        /// <param name="identifier">The identifier itself.</param>
        public UniqueIdentifier(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            IdentifierType type,
            string identifier,
            Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Type = type;
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = environment;
            this.Identifier = identifier;
        }

        /// <summary>
        /// Gets the type of entity the identifier is for.
        /// </summary>
        public IdentifierType Type { get; private set; }

        /// <summary>
        /// Gets the ID of the tenant the reference number is for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the product the reference number is for.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the deployment environment the reference number is for.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the actual number in text.
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Gets a record of the consumption of this unique identifier.
        /// </summary>
        public UniqueIdentifierConsumption Consumption { get; private set; }

        /// <summary>
        /// Mark the identifier as having been used.
        /// </summary>
        /// <param name="timestamp">The time this identifier was consumed.</param>
        /// <returns>The identifier value.</returns>
        public string Consume(Instant timestamp)
        {
            this.Consumption = new UniqueIdentifierConsumption(timestamp);
            return this.Identifier;
        }
    }
}

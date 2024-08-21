// <copyright file="ClaimNumber.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// A claim number for use in a particular product.
    /// </summary>
    public class ClaimNumber : Entity<Guid>, IReferenceNumber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimNumber"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        public ClaimNumber()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimNumber"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the claim number is for.</param>
        /// <param name="productId">The ID of the product the claim number is for.</param>
        /// <param name="environment">The environment the claim number is for.</param>
        /// <param name="number">The claim number.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public ClaimNumber(Guid tenantId, Guid productId, DeploymentEnvironment environment, string number, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = environment;
            this.Number = number;
        }

        /// <summary>
        /// Gets the ID of the tenant the claim number is for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the product the claim number is for.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the deployment environment the claim number is for.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets a the claim number for assignment.
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a claim number has already been used.
        /// </summary>
        public bool IsAssigned { get; private set; }

        /// <summary>
        /// Use the number for claims, tagging the object as assigned.
        /// </summary>
        /// <returns>The claim number.</returns>
        public string Consume()
        {
            this.IsAssigned = true;
            return this.Number;
        }

        /// <summary>
        /// Use the number for claims, tagging the object as un-assigned.
        /// </summary>
        public void UnConsume()
        {
            this.IsAssigned = false;
        }
    }
}

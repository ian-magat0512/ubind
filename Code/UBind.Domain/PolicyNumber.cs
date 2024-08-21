// <copyright file="PolicyNumber.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// A policy number for use in a particular product.
    /// </summary>
    public class PolicyNumber : Entity<Guid>, IReferenceNumber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyNumber"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        public PolicyNumber()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyNumber"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the number is for.</param>
        /// <param name="productId">The ID of the product the number is for.</param>
        /// <param name="environment">The deployment environment the policy number is for.</param>
        /// <param name="number">The policy number itself.</param>
        /// <param name="createdTimestamp">The time the policy number was created.</param>
        public PolicyNumber(
             Guid tenantId, Guid productId, DeploymentEnvironment environment, string number, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = environment;
            this.Number = number;
        }

        /// <summary>
        /// Gets the ID of the tenant the policy number is for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the product the policy number is for.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the deployment environment the policy number is for.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy number has already been used.
        /// </summary>
        public bool IsAssigned { get; private set; }

        /// <summary>
        /// Gets the actual policy number text.
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Take the number for use in a policy.
        /// </summary>
        /// <returns>The policy number.</returns>
        public string Consume()
        {
            if (this.IsAssigned)
            {
                throw new ErrorException(Errors.Policy.PolicyNumberAlreadyAssigned(this.Number));
            }
            this.IsAssigned = true;
            return this.Number;
        }

        /// <summary>
        /// Un-assign number.
        /// </summary>
        public void UnConsume()
        {
            this.IsAssigned = false;
        }
    }
}

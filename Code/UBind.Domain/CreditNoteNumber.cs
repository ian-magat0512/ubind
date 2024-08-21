// <copyright file="CreditNoteNumber.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// An credit note number for use in a particular product.
    /// </summary>
    /// <remarks>Parameterless constructor.</remarks>
    public class CreditNoteNumber : Entity<Guid>, IReferenceNumber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreditNoteNumber"/> class.
        /// </summary>
        public CreditNoteNumber()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditNoteNumber"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the credit note number is for.</param>
        /// <param name="productId">The ID of the product for which the credit note number is for.</param>
        /// <param name="createdTimestamp">The time the credit note number is inserted/created.</param>
        /// <param name="environment">The deployment environment the credit note number is for.</param>
        /// <param name="number">The credit note number for use of the product.</param>
        public CreditNoteNumber(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, string number, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = environment;
            this.Number = number;
        }

        /// <summary>
        /// Gets the ID of the product the credit note number is for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the product the credit note number is for.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the deployment environment the credit note number is for.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the actual credit note number in text.
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the credit note number has already been used.
        /// </summary>
        public bool IsAssigned { get; set; }

        /// <summary>
        /// Takes the number for use in an credit note.
        /// </summary>
        /// <returns>The credit note number.</returns>
        public string Consume()
        {
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

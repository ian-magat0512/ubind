// <copyright file="InvoiceNumber.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// An invoice number for use in a particular product.
    /// </summary>
    /// <remarks>Parameterless constructor.</remarks>
    public class InvoiceNumber : Entity<Guid>, IReferenceNumber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceNumber"/> class.
        /// </summary>
        public InvoiceNumber()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceNumber"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the invoice number is for.</param>
        /// <param name="productId">The Id of the product for which the invoice number is for.</param>
        /// <param name="createdTimestamp">The time the invoice number is inserted/created.</param>
        /// <param name="environment">The deployment environment the invoice number is for.</param>
        /// <param name="number">The invoice number for use of the product.</param>
        public InvoiceNumber(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, string number, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = environment;
            this.Number = number;
        }

        /// <summary>
        /// Gets the ID of the product the invoice number is for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the product the invoice number is for.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the deployment environment the invoice number is for.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the actual invoice number in text.
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the invoice number has already been used.
        /// </summary>
        public bool IsAssigned { get; set; }

        /// <summary>
        /// Takes the number for use in an invoice.
        /// </summary>
        /// <returns>The invoice number.</returns>
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

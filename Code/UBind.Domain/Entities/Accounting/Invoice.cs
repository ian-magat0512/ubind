// <copyright file="Invoice.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Accounting
{
    using System;
    using NodaTime;

    /// <summary>
    /// An invoice, a commercial document for basic accounting.
    /// Since there is a class named Invoice already, we named this NewInvoice.
    /// </summary>
    public class Invoice : CommercialDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Invoice"/> class.
        /// </summary>
        /// <param name="createdTimestamp">The created time of this invoice.</param>
        /// <param name="number">The invoice number.</param>
        /// <param name="dueDateTime">The invoice due date.</param>
        /// <param name="breakdownId">The payment breakdown.</param>
        public Invoice(
            Guid tenantId,
            Instant createdTimestamp,
            IReferenceNumber number,
            Instant dueDateTime,
            Guid? breakdownId)
              : base(tenantId, createdTimestamp, number, dueDateTime, breakdownId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invoice"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        public Invoice()
        {
        }
    }
}

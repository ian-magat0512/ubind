// <copyright file="CreditNote.cs" company="uBind">
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
    /// Since there is a class named CreditNote already, we named this NewCreditNote.
    /// </summary>
    public class CreditNote : CommercialDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreditNote"/> class.
        /// </summary>
        /// <param name="createdTimestamp">The created time of this invoice.</param>
        /// <param name="number">The invoice number.</param>
        /// <param name="dueDateTime">The creditnote due date.</param>
        /// <param name="breakdownId">The credit note breakdown.</param>
        public CreditNote(
            Guid tenantId,
            Instant createdTimestamp,
            IReferenceNumber number,
            Instant dueDateTime,
            Guid? breakdownId)
              : base(tenantId, createdTimestamp, number, dueDateTime, breakdownId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditNote"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        public CreditNote()
        {
        }
    }
}
